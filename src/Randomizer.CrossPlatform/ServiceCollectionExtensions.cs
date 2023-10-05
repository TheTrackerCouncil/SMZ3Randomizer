using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary;
using Randomizer.Data.Configuration;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.Twitch;
using Randomizer.Sprites;

namespace Randomizer.CrossPlatform;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Randomizer + Tracker
        services.AddConfigs();
        services.AddRandomizerServices();

        services.AddSingleton<ITrackerStateService, TrackerStateService>();
        services.AddSingleton<SpriteService>();

        // Chat
        services.AddSingleton<IChatApi, TwitchChatAPI>();
        services.AddScoped<IChatClient, TwitchChatClient>();
        services.AddSingleton<IChatAuthenticationService, TwitchAuthenticationService>();

        // MSU Randomizer
        services.AddMsuRandomizerServices();

        // Misc
        services.AddSingleton<OptionsFactory>();
        services.AddSingleton<IGameDbService, GameDbService>();
        services.AddTransient<SourceRomValidationService>();

        return services;
    }
}
