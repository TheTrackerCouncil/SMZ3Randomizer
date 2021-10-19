using System;

using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;

namespace Microsoft.Extensions.DependencyInjection
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
        /// <param name="trackerJsonPath">
        /// The path to the JSON track configuration file.
        /// </param>
        /// <returns>A reference to <paramref name="services"/>.</returns>
        public static IServiceCollection AddTracker<TWorldAccessor>(this IServiceCollection services,
            string trackerJsonPath)
            where TWorldAccessor : class, IWorldAccessor
        {
            services.AddSingleton<IWorldAccessor>(x => x.GetRequiredService<TWorldAccessor>());
            services.AddSingleton(new TrackerConfigProvider(trackerJsonPath));
            services.AddScoped<Tracker>();

            return services;
        }
    }
}
