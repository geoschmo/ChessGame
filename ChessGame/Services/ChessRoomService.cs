using ChessGame.Models;

namespace ChessGame.Services;

public sealed class ChessRoomService
{
    private static readonly char[] RoomAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
    private readonly Dictionary<string, ChessRoom> rooms = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> connectionToRoom = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> tokenToRoom = new(StringComparer.Ordinal);
    private readonly GamePersistenceService persistence;
    private readonly Random random = new();
    private readonly object syncRoot = new();

    public ChessRoomService(GamePersistenceService persistence)
    {
        this.persistence = persistence;
    }

    public ChessRoom CreateRoom(string connectionId, string? playerToken)
    {
        lock (syncRoot)
        {
            var roomCode = GenerateRoomCode();
            var room = new ChessRoom(roomCode);
            var token = string.IsNullOrWhiteSpace(playerToken) ? GeneratePlayerToken() : playerToken;

            room.AddPlayer(connectionId, token);
            rooms[roomCode] = room;
            connectionToRoom[connectionId] = roomCode;
            tokenToRoom[token] = roomCode;
            persistence.SaveGame(room);

            return room;
        }
    }

    public ChessRoom? JoinRoom(string roomCode, string connectionId, string? playerToken)
    {
        lock (syncRoot)
        {
            if (!rooms.TryGetValue(roomCode, out var room))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(playerToken) && room.ReconnectPlayer(connectionId, playerToken))
            {
                connectionToRoom[connectionId] = roomCode;
                tokenToRoom[playerToken] = roomCode;
                persistence.SaveGame(room);
                return room;
            }

            if (room.IsFull())
            {
                return null;
            }

            var token = string.IsNullOrWhiteSpace(playerToken) ? GeneratePlayerToken() : playerToken;
            room.AddPlayer(connectionId, token);
            connectionToRoom[connectionId] = roomCode;
            tokenToRoom[token] = roomCode;
            persistence.SaveGame(room);
            return room;
        }
    }

    public ChessRoom? GetRoomByConnection(string connectionId)
    {
        lock (syncRoot)
        {
            return connectionToRoom.TryGetValue(connectionId, out var roomCode) &&
                rooms.TryGetValue(roomCode, out var room)
                ? room
                : null;
        }
    }

    public ChessRoom? RestoreAndReconnect(string token, string connectionId)
    {
        lock (syncRoot)
        {
            if (tokenToRoom.TryGetValue(token, out var roomCode) &&
                rooms.TryGetValue(roomCode, out var room) &&
                room.ReconnectPlayer(connectionId, token))
            {
                connectionToRoom[connectionId] = roomCode;
                persistence.SaveGame(room);
                return room;
            }

            var persisted = persistence.GetGameByToken(token);
            var restored = persisted?.ToRoom();
            if (restored == null || !restored.ReconnectPlayer(connectionId, token))
            {
                return null;
            }

            rooms[restored.RoomCode] = restored;
            connectionToRoom[connectionId] = restored.RoomCode;
            if (restored.WhiteToken != null)
            {
                tokenToRoom[restored.WhiteToken] = restored.RoomCode;
            }

            if (restored.BlackToken != null)
            {
                tokenToRoom[restored.BlackToken] = restored.RoomCode;
            }

            persistence.SaveGame(restored);
            return restored;
        }
    }

    public PersistedGame? GetActiveGameForToken(string token)
    {
        return persistence.GetGameByToken(token);
    }

    public void HandlePlayerDisconnect(string connectionId)
    {
        lock (syncRoot)
        {
            if (!connectionToRoom.TryGetValue(connectionId, out var roomCode) ||
                !rooms.TryGetValue(roomCode, out var room))
            {
                return;
            }

            room.RemovePlayer(connectionId);
            connectionToRoom.Remove(connectionId);

            if (room.Game.MoveHistory.Count > 0 || room.Status == RoomStatus.Active)
            {
                room.Status = RoomStatus.WaitingForReconnect;
                room.DisconnectedAt = DateTime.UtcNow;
                persistence.SaveGame(room);
                return;
            }

            if (room.IsEmpty())
            {
                rooms.Remove(roomCode);
                persistence.DeleteGame(roomCode);
            }
            else
            {
                persistence.SaveGame(room);
            }
        }
    }

    public object BuildState(ChessRoom room, string? viewerConnectionId = null)
    {
        var viewerColor = viewerConnectionId == null ? null : room.GetColorForConnection(viewerConnectionId);

        return new
        {
            roomCode = room.RoomCode,
            status = room.Status.ToString(),
            viewerColor = viewerColor?.ToString(),
            whiteConnected = room.WhiteConnectionId != null,
            blackConnected = room.BlackConnectionId != null,
            game = room.Game
        };
    }

    private string GenerateRoomCode()
    {
        string roomCode;

        do
        {
            roomCode = new string(Enumerable.Range(0, 6)
                .Select(_ => RoomAlphabet[random.Next(RoomAlphabet.Length)])
                .ToArray());
        }
        while (rooms.ContainsKey(roomCode));

        return roomCode;
    }

    private static string GeneratePlayerToken() => Guid.NewGuid().ToString("N");
}
