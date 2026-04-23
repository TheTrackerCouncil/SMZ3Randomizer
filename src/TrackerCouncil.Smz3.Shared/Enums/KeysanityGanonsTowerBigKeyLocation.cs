using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

[DefaultValue(Anywhere)]
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum KeysanityGanonsTowerBigKeyLocation
{
    [Description("Randomly place anywhere")]
    Anywhere,

    [Description("Place in Ganon's Tower")]
    GanonsTower,

    [Description("50% Chance in Ganon's Tower")]
    Random
}
