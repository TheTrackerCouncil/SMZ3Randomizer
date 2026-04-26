using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

[DefaultValue(Closed)]
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum TourianBossDoor
{
    [Description("Require Crateria boss keycard")]
    Closed,

    [Description("Open by default")]
    Open,

    [Description("Random")]
    Random
}
