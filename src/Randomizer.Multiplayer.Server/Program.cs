using Microsoft.Extensions.Options;
using Randomizer.Multiplayer.Server;
using Serilog;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(logger);

builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSingleton<MultiplayerDbService>();
builder.Services.AddDbContextFactory<MultiplayerDbContext>();
builder.Services.Configure<SMZ3ServerSettings>(builder.Configuration.GetSection("SMZ3"));

var app = builder.Build();

var settings = app.Services.GetService<IOptions<SMZ3ServerSettings>>();
if (string.IsNullOrEmpty(settings?.Value.ServerUrl))
{
    // Make sure we write to the console in case serilog is configured just for writing to logs
    Console.WriteLine("The SMZ3 ServerUrl property needs to be populated in the appsettings.json file");
    logger.Error("The SMZ3 ServerUrl property needs to be populated in the appsettings.json file");
    return;
}

app.MapHub<MultiplayerHub>("");
app.Services.GetService<GameManager>();

app.Run();

