using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared.Enums
{
    public enum RegionFilter
    {
        [Description("Both games")]
        None = 0,

        [Description("Zelda only")]
        ZeldaOnly = 1,

        [Description("Metroid only")]
        MetroidOnly = 2
    }
}
