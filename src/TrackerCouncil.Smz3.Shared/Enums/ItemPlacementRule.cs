using System.ComponentModel;
using TrackerCouncil.Shared;

namespace TrackerCouncil.Smz3.Shared.Enums;

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
