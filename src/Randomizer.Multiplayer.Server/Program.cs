using Randomizer.Multiplayer.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();
var app = builder.Build();

app.MapHub<MultiplayerHub>("");

var provider = app.Services.GetService<GameManager>();

app.Run();

