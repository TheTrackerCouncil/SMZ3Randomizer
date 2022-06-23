using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared.Enums
{
    /// <summary>
    /// Enum for how frequently tracker should voice things
    /// </summary>
    public enum TrackerVoiceFrequency
    {
        [Description("All")]
        All,
        [Description("Disabled")]
        Disabled
    }
}
