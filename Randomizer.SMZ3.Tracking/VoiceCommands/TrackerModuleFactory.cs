using System;
using System.Speech.Recognition;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class TrackerModuleFactory
    {
        public void LoadAll(Tracker tracker, SpeechRecognitionEngine engine)
        {
            var modules = new TrackerModule[] {
                new ItemTrackingModule(tracker),
                new PegWorldModeModule(tracker)
            };

            foreach (var module in modules)
            {
                module.LoadInto(engine);
            }
        }
    }
}
