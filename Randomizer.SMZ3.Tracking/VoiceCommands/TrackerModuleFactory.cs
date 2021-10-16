using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class TrackerModuleFactory
    {
        public void LoadAll(Tracker tracker, SpeechRecognitionEngine engine)
        {
            var modules = DiscoverModules(tracker);

            foreach (var module in modules)
            {
                foreach (var rule in module.Syntax)
                    Debug.WriteLine($"{rule.Key}: {string.Join("\n    ", rule.Value)}");
                module.LoadInto(engine);
            }
        }

        protected IEnumerable<TrackerModule> DiscoverModules(Tracker tracker)
        {
            var assembly = typeof(TrackerModuleFactory).Assembly;
            return DiscoverModules(tracker, assembly);
        }

        protected virtual IEnumerable<TrackerModule> DiscoverModules(Tracker tracker, Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(TrackerModule)))
                .Select(x => (TrackerModule)Activator.CreateInstance(x, tracker)!);
        }
    }
}
