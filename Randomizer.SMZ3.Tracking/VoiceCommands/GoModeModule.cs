using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class GoModeModule : TrackerModule
    {
        public GoModeModule(Tracker tracker) : base(tracker)
        {
            AddCommand("Toggle Go Mode", "Hey tracker, track Go Mode.", (tracker, result) =>
            {
                tracker.ToggleGoMode(result.Confidence);
            });
        }
    }
}
