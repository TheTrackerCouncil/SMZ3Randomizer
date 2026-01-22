using System;
using System.Linq;
using Avalonia.Controls;
using AvaloniaControls.Extensions;
using GitHubReleaseChecker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MSURandomizer;
using PySpeechService.Client;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Chat.Integration;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Multiplayer.Client;
using TrackerCouncil.Smz3.Tracking;
using TrackerCouncil.Smz3.Tracking.AutoTracking;
using TrackerCouncil.Smz3.Tracking.VoiceCommands;
using TrackerCouncil.Smz3.Chat.Twitch;
using TrackerCouncil.Smz3.SeedGenerator;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.Views;

namespace TrackerCouncil.Smz3.UI;

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
        services.AddSingleton<TrackerSpriteService>();
        services.AddTransient<SharedCrossplatformService>();

        // Chat
        services.AddSingleton<IChatApi, TwitchChatAPI>();
        services.AddScoped<IChatClient, TwitchChatClient>();
        services.AddSingleton<IChatAuthenticationService, TwitchAuthenticationService>();

        // MSU Randomizer
        services.AddMsuRandomizerUiServices();

        // Misc
        services.AddGitHubReleaseCheckerServices();
        services.AddSingleton<IGameDbService, GameDbService>();
        services.AddTransient<SourceRomValidationService>();
        services.AddTransient<IGitHubConfigDownloaderService, GitHubConfigDownloaderService>();
        services.AddSingleton<IGitHubFileSynchronizerService, GitHubFileSynchronizerService>();
        services.AddSingleton<OptionsFactory>();

        services.AddControlServices<MSURandomizer.App>();
        services.AddAvaloniaControlServices<Program>();
        services.AddWindows<Program>();
        services.AddTransient<OptionsWindowService>();
        services.AddTransient<SoloRomListPanel>();
        services.AddTransient<MultiRomListPanel>();
        services.AddTransient<GenerationSettingsWindowService>();

        if (OperatingSystem.IsLinux())
        {
            services.AddPySpeechService();
        }

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
