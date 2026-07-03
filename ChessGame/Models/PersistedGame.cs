using System.Text.Json;

namespace ChessGame.Models;

public sealed class PersistedGame
{
    public string Id { get; set; } = string.Empty;
    public string? WhiteToken { get; set; }
    public string? BlackToken { get; set; }
    public string? WhiteConnectionId { get; set; }
    public string? BlackConnectionId { get; set; }
    public RoomStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public string? GameStateJson { get; set; }

    public PersistedGame()
    {
    }

    public PersistedGame(ChessRoom room)
    {
        UpdateFromRoom(room);
        Id = room.RoomCode;
        CreatedAt = room.CreatedAt;
    }

    public void UpdateFromRoom(ChessRoom room)
    {
        Id = room.RoomCode;
        WhiteToken = room.WhiteToken;
        BlackToken = room.BlackToken;
        WhiteConnectionId = room.WhiteConnectionId;
        BlackConnectionId = room.BlackConnectionId;
        Status = room.Status;
        CreatedAt = room.CreatedAt;
        LastActivityAt = DateTime.UtcNow;
        DisconnectedAt = room.DisconnectedAt;
        GameStateJson = JsonSerializer.Serialize(room.Game);
    }

    public ChessRoom? ToRoom()
    {
        var room = new ChessRoom(Id)
        {
            WhiteConnectionId = WhiteConnectionId,
            BlackConnectionId = BlackConnectionId,
            WhiteToken = WhiteToken,
            BlackToken = BlackToken,
            Status = Status,
            CreatedAt = CreatedAt,
            DisconnectedAt = DisconnectedAt
        };

        if (!string.IsNullOrWhiteSpace(GameStateJson))
        {
            room.Game = JsonSerializer.Deserialize<ChessGameState>(GameStateJson) ?? new ChessGameState();
        }

        return room;
    }
}
