using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.VoiceCommands;

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
        /// <typeparam name="TWorldAccessor">
        /// A type implementing <see cref="IWorldAccessor"/>. The type should
        /// already be registered as a singleton.
        /// </typeparam>
        /// <param name="services">
        /// The service collection to add Tracker to.
        /// </param>
        /// <returns>A reference to <paramref name="services"/>.</returns>
        public static IServiceCollection AddTracker<TWorldAccessor>(this IServiceCollection services)
            where TWorldAccessor : class, IWorldAccessor
        {
            services.AddBasicTrackerModules<TrackerModuleFactory>();
            services.AddScoped<TrackerModuleFactory>();

            services.AddSingleton<IWorldAccessor>(x => x.GetRequiredService<TWorldAccessor>());
            services.AddSingleton<TrackerConfigProvider>();
            services.AddSingleton(serviceProvider =>
            {
                var configProvider = serviceProvider.GetRequiredService<TrackerConfigProvider>();
                return configProvider.GetMapConfig();
            });
            services.AddSingleton(serviceProvider =>
            {
                var configProvider = serviceProvider.GetRequiredService<TrackerConfigProvider>();
                return configProvider.GetTrackerConfig();
            });
            services.AddSingleton(serviceProvider =>
            {
                var configProvider = serviceProvider.GetRequiredService<TrackerConfigProvider>();
                return configProvider.GetLocationConfig();
            });

            services.AddScoped<TrackerFactory>();
            services.AddScoped(serviceProvider =>
            {
                var factory = serviceProvider.GetRequiredService<TrackerFactory>();
                return factory.Instance ?? throw new InvalidOperationException("Cannot resolve Tracker instance before TrackerFactory.Create has been called.");
            });

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
