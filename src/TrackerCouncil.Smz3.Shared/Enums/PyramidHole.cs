using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

[DefaultValue(Closed)]
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum PyramidHole
{
    [Description("Opens after Ganon's Tower is completed")]
    Closed,

    [Description("Open by default")]
    Open,

    [Description("Random")]
    Random
}
