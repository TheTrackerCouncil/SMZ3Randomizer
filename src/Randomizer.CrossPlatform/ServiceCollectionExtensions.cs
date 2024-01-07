using GitHubReleaseChecker;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizerLibrary;
using Randomizer.Abstractions;
using Randomizer.Data.Configuration;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Multiplayer.Client;
using Randomizer.SMZ3.ChatIntegration;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.AutoTracking;
using Randomizer.SMZ3.Tracking.VoiceCommands;
using Randomizer.SMZ3.Twitch;

namespace Randomizer.CrossPlatform;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Randomizer + Tracker
        services.AddConfigs();
        services.AddRandomizerServices();
        services.AddTracker()
            .AddOptionalModule<PegWorldModeModule>()
            .AddOptionalModule<SpoilerModule>()
            .AddOptionalModule<AutoTrackerModule>()
            .AddOptionalModule<MapModule>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<AutoTrackerBase, AutoTracker>();
        services.AddSingleton<ITrackerStateService, TrackerStateService>();
        services.AddMultiplayerServices();
        services.AddSingleton<SpriteService>();

        // Chat
        services.AddSingleton<IChatApi, TwitchChatAPI>();
        services.AddScoped<IChatClient, TwitchChatClient>();
        services.AddSingleton<IChatAuthenticationService, TwitchAuthenticationService>();

        // MSU Randomizer
        services.AddMsuRandomizerServices();

        // Misc
        services.AddGitHubReleaseCheckerServices();
        services.AddSingleton<IGameDbService, GameDbService>();
        services.AddTransient<SourceRomValidationService>();
        services.AddTransient<IGitHubConfigDownloaderService, GitHubConfigDownloaderService>();
        services.AddTransient<IGitHubSpriteDownloaderService, GitHubSpriteDownloaderService>();
        services.AddSingleton<OptionsFactory>();

        // Console
        services.AddTransient<ConsoleTrackerDisplayService>();

        return services;
    }
}
