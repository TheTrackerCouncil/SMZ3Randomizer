using System;

using Microsoft.Extensions.Logging;

using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Tracking.VoiceCommands;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides new <see cref="Tracker"/> instances.
    /// </summary>
    public class TrackerFactory
    {
        private readonly TrackerConfig _config;
        private readonly LocationConfig _locationConfig;
        private readonly IWorldAccessor _worldAccessor;
        private readonly TrackerModuleFactory _moduleFactory;
        private readonly ILogger<Tracker> _logger;
        private readonly RandomizerContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerFactory"/> class
        /// with the specified dependencies.
        /// </summary>
        /// <param name="config">The tracking configuration.</param>
        /// <param name="locationConfig">The location configuration.</param>
        /// <param name="worldAccessor">
        /// Used to get the world to track in.
        /// </param>
        /// <param name="moduleFactory">
        /// Used to provide the tracking speech recognition syntax.
        /// </param>
        /// <param name="logger">Used to write logging information.</param>
        public TrackerFactory(TrackerConfig config,
            LocationConfig locationConfig,
            IWorldAccessor worldAccessor,
            TrackerModuleFactory moduleFactory,
            ILogger<Tracker> logger,
            RandomizerContext dbContext)
        {
            _config = config;
            _locationConfig = locationConfig;
            _worldAccessor = worldAccessor;
            _moduleFactory = moduleFactory;
            _logger = logger;
            _dbContext = dbContext;
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
            return Instance = new(_config, _locationConfig, _worldAccessor, _moduleFactory, _logger, options, _dbContext);
        }
    }
}
