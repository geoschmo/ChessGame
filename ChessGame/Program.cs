using System.Text.Json.Serialization;
using ChessGame.Hubs;
using ChessGame.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var dbPath = ResolveDatabasePath(builder.Environment.ContentRootPath);
builder.Services.AddSingleton(sp => new GamePersistenceService(dbPath));
builder.Services.AddSingleton<ChessRoomService>();

var app = builder.Build();

var pathBase = app.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<GameHub>("/gameHub");

app.Run();

static string? ResolveDatabasePath(string contentRootPath)
{
    var possiblePaths = new List<string>
    {
        Path.Combine(contentRootPath, "App_Data")
    };

    var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    if (!string.IsNullOrWhiteSpace(localAppData))
    {
        possiblePaths.Add(Path.Combine(localAppData, "ChessGame"));
    }

    var tempPath = Path.GetTempPath();
    if (!string.IsNullOrWhiteSpace(tempPath))
    {
        possiblePaths.Add(tempPath);
    }

    foreach (var basePath in possiblePaths)
    {
        try
        {
            Directory.CreateDirectory(basePath);
            var testFile = Path.Combine(basePath, ".write_test");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return Path.Combine(basePath, "chess.db");
        }
        catch
        {
            // Try the next persistence location.
        }
    }

    return null;
}
