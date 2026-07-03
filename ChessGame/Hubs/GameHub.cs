using ChessGame.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChessGame.Hubs;

public sealed class GameHub : Hub
{
    private readonly ChessRoomService roomService;

    public GameHub(ChessRoomService roomService)
    {
        this.roomService = roomService;
    }

    public async Task<object> CreateRoom(string? playerToken = null)
    {
        var room = roomService.CreateRoom(Context.ConnectionId, playerToken);
        await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomCode);

        return new
        {
            success = true,
            roomCode = room.RoomCode,
            playerColor = "White",
            playerToken = room.WhiteToken,
            state = roomService.BuildState(room, Context.ConnectionId)
        };
    }

    public async Task<object> JoinRoom(string roomCode, string? playerToken = null)
    {
        var room = roomService.JoinRoom(roomCode.ToUpperInvariant(), Context.ConnectionId, playerToken);
        if (room == null)
        {
            return new { success = false, message = "Room not found or is full." };
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomCode);

        var otherPlayer = room.GetOtherPlayer(Context.ConnectionId);
        if (otherPlayer != null)
        {
            await Clients.Client(otherPlayer).SendAsync("OpponentJoined");
        }

        return new
        {
            success = true,
            roomCode = room.RoomCode,
            playerColor = room.GetColorForConnection(Context.ConnectionId)?.ToString(),
            playerToken = room.GetTokenForConnection(Context.ConnectionId),
            state = roomService.BuildState(room, Context.ConnectionId)
        };
    }

    public Task<object> CheckForActiveGame(string playerToken)
    {
        var persisted = roomService.GetActiveGameForToken(playerToken);
        if (persisted == null)
        {
            return Task.FromResult<object>(new { hasActiveGame = false });
        }

        return Task.FromResult<object>(new
        {
            hasActiveGame = true,
            roomCode = persisted.Id,
            status = persisted.Status.ToString()
        });
    }

    public async Task<object> ReconnectToGame(string playerToken)
    {
        var room = roomService.RestoreAndReconnect(playerToken, Context.ConnectionId);
        if (room == null)
        {
            return new { success = false, message = "Could not reconnect to game." };
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomCode);

        var otherPlayer = room.GetOtherPlayer(Context.ConnectionId);
        if (otherPlayer != null)
        {
            await Clients.Client(otherPlayer).SendAsync("OpponentReconnected");
        }

        return new
        {
            success = true,
            roomCode = room.RoomCode,
            playerColor = room.GetColorForConnection(Context.ConnectionId)?.ToString(),
            state = roomService.BuildState(room, Context.ConnectionId),
            opponentConnected = otherPlayer != null
        };
    }

    public Task<object> MakeMove(string from, string to, string? promotion = null)
    {
        return Task.FromResult<object>(new
        {
            success = false,
            message = "The chess rules engine is not implemented yet."
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var room = roomService.GetRoomByConnection(Context.ConnectionId);
        var otherPlayer = room?.GetOtherPlayer(Context.ConnectionId);

        roomService.HandlePlayerDisconnect(Context.ConnectionId);

        if (otherPlayer != null)
        {
            await Clients.Client(otherPlayer).SendAsync("OpponentDisconnected", new { canReconnect = true });
        }

        await base.OnDisconnectedAsync(exception);
    }
}
