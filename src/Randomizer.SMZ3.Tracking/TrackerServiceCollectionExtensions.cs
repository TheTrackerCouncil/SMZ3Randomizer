using System;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks;
using Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks;
using Randomizer.Data.Configuration;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.SMZ3.Tracking.VoiceCommands;
using Randomizer.Data;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.Tracking
{
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
            services.AddBasicTrackerModules<TrackerModuleFactory>();
            services.AddScoped<TrackerModuleFactory>();
            services.AddScoped<TrackerOptionsAccessor>();
            services.AddScoped<ITrackerTimerService, TrackerTimerService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ICommunicator, TextToSpeechCommunicator>();
            services.AddScoped<IUIService, UIService>();
            services.AddScoped<IWorldService, WorldService>();
            services.AddScoped<IRandomizerConfigService, RandomizerConfigService>();
            services.AddScoped<Tracker>();

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
    }
}
