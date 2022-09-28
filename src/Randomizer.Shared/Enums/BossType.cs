using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared.Enums
{
    public enum BossType
    {
        None,
        [Description("Kraid")]
        Kraid,
        [Description("Phantoon")]
        Phantoon,
        [Description("Draygon")]
        Draygon,
        [Description("Ridley")]
        Ridley,
    }
}
