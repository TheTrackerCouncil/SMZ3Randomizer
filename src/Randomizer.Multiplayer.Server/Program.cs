using Microsoft.Extensions.Options;
using Randomizer.Multiplayer.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSingleton<MultiplayerDbService>();
builder.Services.AddDbContextFactory<MultiplayerDbContext>();
builder.Services.Configure<SMZ3ServerSettings>(builder.Configuration.GetSection("SMZ3"));

var app = builder.Build();

var settings = app.Services.GetService<IOptions<SMZ3ServerSettings>>();
if (string.IsNullOrEmpty(settings?.Value.ServerUrl))
{
    throw new InvalidOperationException("The SMZ3 ServerUrl property needs to be populated in the appsettings.json file");
}

app.MapHub<MultiplayerHub>("");
app.Services.GetService<GameManager>();

app.Run();

