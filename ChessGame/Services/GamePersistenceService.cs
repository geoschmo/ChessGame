using ChessGame.Models;
using LiteDB;

namespace ChessGame.Services;

public sealed class GamePersistenceService : IDisposable
{
    private readonly LiteDatabase? database;
    private readonly ILiteCollection<PersistedGame>? games;

    public GamePersistenceService(string? databasePath = null)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            return;
        }

        database = new LiteDatabase(databasePath);
        games = database.GetCollection<PersistedGame>("games");
        games.EnsureIndex(game => game.Id, true);
        games.EnsureIndex(game => game.WhiteToken);
        games.EnsureIndex(game => game.BlackToken);
    }

    public void SaveGame(ChessRoom room)
    {
        games?.Upsert(new PersistedGame(room));
    }

    public PersistedGame? GetGameByToken(string token)
    {
        return games?.FindOne(game =>
            (game.WhiteToken == token || game.BlackToken == token) &&
            (game.Status == RoomStatus.Active || game.Status == RoomStatus.WaitingForReconnect));
    }

    public void DeleteGame(string roomCode)
    {
        games?.Delete(roomCode);
    }

    public void CleanupOldGames(TimeSpan maxAge)
    {
        if (games == null)
        {
            return;
        }

        var cutoff = DateTime.UtcNow.Subtract(maxAge);
        games.DeleteMany(game => game.LastActivityAt < cutoff);
    }

    public void Dispose()
    {
        database?.Dispose();
    }
}
