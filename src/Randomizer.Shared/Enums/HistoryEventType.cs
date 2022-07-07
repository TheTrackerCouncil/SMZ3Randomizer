using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared.Enums
{
    public enum HistoryEventType
    {
        None,
        [Description("Entered")]
        EnteredRegion,
        [Description("Picked up")]
        TrackedItem,
        [Description("Defeated")]
        BeatBoss
    }
}
