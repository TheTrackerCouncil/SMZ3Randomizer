using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ItemPlacementRule
    {    
        [Description("Anywhere")]
        Anywhere,

        [Description("Dungeons and Metroid")]
        DungeonsAndMetroid,

        [Description("Crystal dungeons and Metroid")]
        CrystalDungeonsAndMetroid,

        [Description("Opposite game")]
        OppositeGame,

        [Description("Same game")]
        SameGame,
    }
}
