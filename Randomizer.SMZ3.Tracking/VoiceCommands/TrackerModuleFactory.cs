using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Loads and manages tracker modules.
    /// </summary>
    public class TrackerModuleFactory
    {
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
            var modules = DiscoverModules(tracker);
            foreach (var module in modules)
            {
                module.LoadInto(engine);
            }

            return modules.SelectMany(x => x.Syntax)
                .ToImmutableSortedDictionary();
        }

        /// <summary>
        /// Returns all available tracker modules in the current assembly.
        /// </summary>
        /// <param name="tracker">The tracker instance to use.</param>
        /// <returns>
        /// A collection of initialized tracker modules in the current assembly.
        /// </returns>
        protected IEnumerable<TrackerModule> DiscoverModules(Tracker tracker)
        {
            var assembly = typeof(TrackerModuleFactory).Assembly;
            return DiscoverModules(tracker, assembly);
        }

        /// <summary>
        /// Returns all available tracker modules in the specified assembly.
        /// </summary>
        /// <param name="tracker">The tracker instance to use.</param>
        /// <param name="assembly">
        /// The assembly whose tracker modules to load.
        /// </param>
        /// <returns>
        /// A collection of initialized tracker modules in the <paramref
        /// name="assembly"/>.
        /// </returns>
        protected virtual IEnumerable<TrackerModule> DiscoverModules(Tracker tracker, Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(TrackerModule)))
                .Select(x => (TrackerModule)Activator.CreateInstance(x, tracker)!);
        }
    }
}
