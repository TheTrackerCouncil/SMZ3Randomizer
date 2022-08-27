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

        [Description("Dungeons and anywhere in Metroid")]
        DungeonsAndMetroid,

        [Description("Crystal dungeons and anywhere in Metroid")]
        CrystalDungeonsAndMetroid,

        [Description("Vanilla game, but randomized location")]
        SameGame,

        [Description("Opposite game")]
        OppositeGame,

        
    }
}
