using Randomizer.Multiplayer.Server;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrEmpty(builder.Configuration.GetValue<string>("SMZ3:ServerUrl")))
{
    throw new InvalidOperationException("The SMZ3 ServerUrl property needs to be populated in the appsettings.json file");
}

builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();

var app = builder.Build();

app.MapHub<MultiplayerHub>("");
app.Services.GetService<GameManager>();

app.Run();

