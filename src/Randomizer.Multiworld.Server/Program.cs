using Randomizer.Multiworld.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    ).Build();*/

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.MapHub<MultiworldHub>("/multiworld");

app.Run();

