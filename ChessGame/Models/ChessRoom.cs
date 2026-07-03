namespace ChessGame.Models;

public sealed class ChessRoom
{
    public ChessRoom(string roomCode)
    {
        RoomCode = roomCode;
        CreatedAt = DateTime.UtcNow;
    }

    public string RoomCode { get; set; }
    public string? WhiteConnectionId { get; set; }
    public string? BlackConnectionId { get; set; }
    public string? WhiteToken { get; set; }
    public string? BlackToken { get; set; }
    public ChessGameState Game { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.WaitingForPlayer;

    public bool IsFull() => WhiteConnectionId != null && BlackConnectionId != null;

    public bool IsEmpty() => WhiteConnectionId == null && BlackConnectionId == null;

    public bool AddPlayer(string connectionId, string token)
    {
        if (WhiteConnectionId == null)
        {
            WhiteConnectionId = connectionId;
            WhiteToken = token;
            return true;
        }

        if (BlackConnectionId == null)
        {
            BlackConnectionId = connectionId;
            BlackToken = token;
            Status = RoomStatus.Active;
            Game.Status = ChessStatus.Playing;
            return true;
        }

        return false;
    }

    public void RemovePlayer(string connectionId)
    {
        if (WhiteConnectionId == connectionId)
        {
            WhiteConnectionId = null;
        }
        else if (BlackConnectionId == connectionId)
        {
            BlackConnectionId = null;
        }
    }

    public bool ReconnectPlayer(string connectionId, string token)
    {
        if (WhiteToken == token)
        {
            WhiteConnectionId = connectionId;
            CheckReconnectionComplete();
            return true;
        }

        if (BlackToken == token)
        {
            BlackConnectionId = connectionId;
            CheckReconnectionComplete();
            return true;
        }

        return false;
    }

    public string? GetOtherPlayer(string connectionId)
    {
        if (WhiteConnectionId == connectionId)
        {
            return BlackConnectionId;
        }

        if (BlackConnectionId == connectionId)
        {
            return WhiteConnectionId;
        }

        return null;
    }

    public string? GetTokenForConnection(string connectionId)
    {
        if (WhiteConnectionId == connectionId)
        {
            return WhiteToken;
        }

        if (BlackConnectionId == connectionId)
        {
            return BlackToken;
        }

        return null;
    }

    public PlayerColor? GetColorForConnection(string connectionId)
    {
        if (WhiteConnectionId == connectionId)
        {
            return PlayerColor.White;
        }

        if (BlackConnectionId == connectionId)
        {
            return PlayerColor.Black;
        }

        return null;
    }

    public PlayerColor? GetColorForToken(string token)
    {
        if (WhiteToken == token)
        {
            return PlayerColor.White;
        }

        if (BlackToken == token)
        {
            return PlayerColor.Black;
        }

        return null;
    }

    private void CheckReconnectionComplete()
    {
        if (WhiteConnectionId != null && BlackConnectionId != null)
        {
            Status = RoomStatus.Active;
            DisconnectedAt = null;
        }
    }
}
