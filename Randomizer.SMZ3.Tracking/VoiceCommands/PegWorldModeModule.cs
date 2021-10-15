using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class PegWorldModeModule : TrackerModule
    {
        public PegWorldModeModule(Tracker tracker) : base(tracker)
        {
            AddCommand("StartPegWorldModeRule", new[] {
                "Hey tracker, we're going to Peg World!",
                "Hey tracker, let's go to Peg World!"
            }, (tracker, result) =>
            {
                var peg = tracker.Pegs.FirstOrDefault(x => !x.Pegged);
                if (peg != null)
                {
                    tracker.Peg(peg, result.Confidence);
                }
            });

            AddCommand("TrackPegRule", new[] {
                "Hey tracker, track Peg.",
                "Hey tracker, peg."
            }, (tracker, result) => tracker.StartPegWorldMode(result.Confidence));
        }
    }
}
