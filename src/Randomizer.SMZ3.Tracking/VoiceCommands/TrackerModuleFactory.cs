using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Speech.Recognition;

using Microsoft.Extensions.DependencyInjection;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Loads and manages tracker modules.
    /// </summary>
    public class TrackerModuleFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerModuleFactory"/>
        /// class.
        /// </summary>
        /// <param name="serviceProvider">
        /// Used to load available tracker modules.
        /// </param>
        public TrackerModuleFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Loads all available tracker modules into the specified speech
        /// recognition engine.
        /// </summary>
        /// <param name="tracker">The tracker instance.</param>
        /// <param name="engine">
        /// The speech recognition engine to initialize.
        /// </param>
        /// <returns>
        /// A dictionary that contains the loaded speech recognition syntax.
        /// </returns>
        public IReadOnlyDictionary<string, IEnumerable<string>> LoadAll(Tracker tracker, SpeechRecognitionEngine engine)
        {
            var modules = _serviceProvider.GetServices<TrackerModule>();
            foreach (var module in modules)
            {
                module.LoadInto(engine);
            }

            return modules.Where(x => !x.IsSecret)
                .SelectMany(x => x.Syntax)
                .ToImmutableSortedDictionary();
        }
    }
}
