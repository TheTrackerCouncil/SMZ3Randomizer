using System.Data.SqlTypes;
using System.Linq;
using Avalonia.Controls;
using AvaloniaControls.Extensions;
using GitHubReleaseChecker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MSURandomizer;
using MSURandomizerLibrary;
using NAudio.MediaFoundation;
using Randomizer.Abstractions;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.Views;
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
            .AddOptionalModule<AutoTrackerVoiceModule>()
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
        services.AddMsuRandomizerAppServices();

        // Misc
        services.AddGitHubReleaseCheckerServices();
        services.AddSingleton<IGameDbService, GameDbService>();
        services.AddTransient<SourceRomValidationService>();
        services.AddTransient<IGitHubConfigDownloaderService, GitHubConfigDownloaderService>();
        services.AddTransient<IGitHubSpriteDownloaderService, GitHubSpriteDownloaderService>();
        services.AddSingleton<OptionsFactory>();
        services.AddSingleton<IMicrophoneService, NullMicrophoneService>();

        services.AddAvaloniaControlServices<Program>();
        services.AddWindows<Program>();
        services.AddTransient<OptionsWindowService>();
        services.AddTransient<SoloRomListPanel>();
        services.AddTransient<SoloRomListService>();
        services.AddTransient<GenerationSettingsWindowService>();

        return services;
    }

    public static IServiceCollection AddWindows<TAssembly>(this IServiceCollection services)
    {
        var windowTypes = typeof(TAssembly).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(Window)));

        foreach (var windowType in windowTypes)
        {
            services.TryAddTransient(windowType);
        }

        return services;
    }
}
