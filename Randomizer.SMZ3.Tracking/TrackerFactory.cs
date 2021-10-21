using System;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.VoiceCommands;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides new <see cref="Tracker"/> instances.
    /// </summary>
    public class TrackerFactory
    {
        private readonly TrackerConfigProvider _configProvider;
        private readonly IWorldAccessor _worldAccessor;
        private readonly TrackerModuleFactory _moduleFactory;
        private readonly ILogger<Tracker> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerFactory"/> class
        /// with the specified dependencies.
        /// </summary>
        /// <param name="configProvider">
        /// Used to provide the tracking configuration.
        /// </param>
        /// <param name="worldAccessor">
        /// Used to get the world to track in.
        /// </param>
        /// <param name="moduleFactory">
        /// Used to provide the tracking speech recognition syntax.
        /// </param>
        /// <param name="logger">Used to write logging information.</param>
        public TrackerFactory(TrackerConfigProvider configProvider,
            IWorldAccessor worldAccessor,
            TrackerModuleFactory moduleFactory,
            ILogger<Tracker> logger)
        {
            _configProvider = configProvider;
            _worldAccessor = worldAccessor;
            _moduleFactory = moduleFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets a previously created <see cref="Tracker"/> instance.
        /// </summary>
        public Tracker? Instance { get; private set; }

        /// <summary>
        /// Returns a new <see cref="Tracker"/> instance with the specified
        /// options.
        /// </summary>
        /// <param name="options">Provides Tracker preferences.</param>
        /// <returns>
        /// A new <see cref="Tracker"/> instance with the specified <paramref
        /// name="options"/>.
        /// </returns>
        public Tracker Create(TrackerOptions options)
        {
            return Instance = new(_configProvider, _worldAccessor, _moduleFactory, _logger, options);
        }
    }
}
