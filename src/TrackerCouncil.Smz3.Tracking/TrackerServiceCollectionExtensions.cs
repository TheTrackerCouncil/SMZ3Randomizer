using System;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharpHook;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Tracking.AutoTracking.AutoTrackerModules;
using TrackerCouncil.Smz3.Tracking.AutoTracking.MetroidStateChecks;
using TrackerCouncil.Smz3.Tracking.AutoTracking.ZeldaStateChecks;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.Tracking.Services.Speech;
using TrackerCouncil.Smz3.Tracking.TrackingServices;
using TrackerCouncil.Smz3.Tracking.VoiceCommands;

namespace TrackerCouncil.Smz3.Tracking;

/// <summary>
/// Provides methods for adding tracking services to a service collection.
/// </summary>
public static class TrackerServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required to start using Tracker.
    /// </summary>
    /// <param name="services">
    /// The service collection to add Tracker to.
    /// </param>
    /// <returns>A reference to <paramref name="services"/>.</returns>
    public static IServiceCollection AddTracker(this IServiceCollection services)
    {
        services.AddTrackerServices<TrackerService>();
        services.AddBasicTrackerModules<TrackerModuleFactory>();
        services.AddScoped<TrackerModuleFactory>();
        services.AddScoped<TrackerOptionsAccessor>();
        services.AddScoped<ITrackerTimerService, TrackerTimerService>();
        services.AddScoped<IHistoryService, HistoryService>();
        services.AddScoped<IPlayerProgressionService, PlayerProgressionService>();
        services.AddScoped<IUIService, UIService>();
        services.AddScoped<IWorldQueryService, WorldQueryService>();
        services.AddScoped<IRandomizerConfigService, RandomizerConfigService>();
        services.AddScoped<TrackerBase, Tracker>();
        services.AddSnesConnectorServices();

        if (OperatingSystem.IsWindows())
        {
            services.AddScoped<ICommunicator, TextToSpeechCommunicator>();
            services.AddScoped<AlwaysOnSpeechRecognitionService>();
            services.AddScoped<PushToTalkSpeechRecognitionService>();
            services.AddScoped<NullSpeechRecognitionService>();
            services.AddTransient<IMicrophoneService, MicrophoneService>();
            services.AddSingleton<IGlobalHook, TaskPoolGlobalHook>();
        }
        else if (OperatingSystem.IsLinux())
        {
            services.AddScoped<ICommunicator, PyTextToSpeechCommunicator>();
            services.AddScoped<PySpeechRecognitionService>();
            services.AddSingleton<IMicrophoneService, NullMicrophoneService>();
            services.AddScoped<NullSpeechRecognitionService>();
        }
        else
        {
            services.AddSingleton<IMicrophoneService, NullMicrophoneService>();
            services.AddScoped<NullSpeechRecognitionService>();
        }

        var assemblies = new[] { Assembly.GetExecutingAssembly() };

        var zeldaStateChecks = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && t.IsClass && t.GetInterface(nameof(IZeldaStateCheck)) == typeof(IZeldaStateCheck));
        foreach (var stateCheck in zeldaStateChecks)
        {
            services.Add(new ServiceDescriptor(typeof(IZeldaStateCheck), stateCheck, ServiceLifetime.Transient));
        }

        var metroidStateChecks = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && t.IsClass && t.GetInterface(nameof(IMetroidStateCheck)) == typeof(IMetroidStateCheck));
        foreach (var stateCheck in metroidStateChecks)
        {
            services.Add(new ServiceDescriptor(typeof(IMetroidStateCheck), stateCheck, ServiceLifetime.Transient));
        }

        var autoTrackerModules = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t != typeof(AutoTrackerModule) && t.IsAssignableTo(typeof(AutoTrackerModule)));
        foreach (var module in autoTrackerModules)
        {
            services.Add(new ServiceDescriptor(typeof(AutoTrackerModule), module, ServiceLifetime.Transient));
        }

        return services;
    }



    /// <summary>
    /// Enables the specified tracker module.
    /// </summary>
    /// <typeparam name="TModule">The type of module to enable.</typeparam>
    /// <param name="services">
    /// The service collection to add the tracker module to.
    /// </param>
    /// <returns>A reference to <paramref name="services"/>.</returns>
    public static IServiceCollection AddOptionalModule<TModule>(this IServiceCollection services)
        where TModule : TrackerModule
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<TrackerModule, TModule>());
        return services;
    }

    private static IServiceCollection AddBasicTrackerModules<TAssembly>(this IServiceCollection services)
    {
        var moduleTypes = typeof(TAssembly).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(TrackerModule)));

        foreach (var moduleType in moduleTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(TrackerModule), moduleType));
        }

        return services;
    }

    private static IServiceCollection AddTrackerServices<TAssembly>(this IServiceCollection services)
    {
        var interfaceNamespace = typeof(TrackerBase).Namespace;
        if (string.IsNullOrEmpty(interfaceNamespace))
        {
            throw new InvalidOperationException("Could not determine TrackerBase namespace");
        }

        var serviceTypes = typeof(TAssembly).Assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(TrackerService)));

        foreach (var serviceType in serviceTypes)
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped(typeof(TrackerService), serviceType));
            var serviceInterface = serviceType.GetInterfaces().FirstOrDefault(x => x.Namespace == interfaceNamespace);
            if (serviceInterface != null)
            {
                services.TryAddScoped(serviceInterface, serviceType);
            }
        }

        return services;
    }
}
