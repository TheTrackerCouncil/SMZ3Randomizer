using Randomizer.Multiplayer.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.MapHub<MultiplayerHub>("");

app.Run();

