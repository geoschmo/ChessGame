namespace ChessGame.Models;

public enum RoomStatus
{
    WaitingForPlayer,
    Active,
    WaitingForReconnect,
    Abandoned,
    Completed
}

public enum ChessStatus
{
    WaitingForOpponent,
    Playing,
    Check,
    Checkmate,
    Stalemate,
    Draw,
    Resigned
}

public enum PlayerColor
{
    White,
    Black
}

public sealed class ChessGameState
{
    public const string InitialFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public string Fen { get; set; } = InitialFen;
    public PlayerColor SideToMove { get; set; } = PlayerColor.White;
    public ChessStatus Status { get; set; } = ChessStatus.WaitingForOpponent;
    public PlayerColor? Winner { get; set; }
    public List<ChessMove> MoveHistory { get; set; } = new();
}

public sealed class ChessMove
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string? Promotion { get; set; }
    public string? Notation { get; set; }
    public string? FenAfterMove { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}
