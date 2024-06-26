using System.ComponentModel;
using TrackerCouncil.Shared;

namespace TrackerCouncil.Smz3.Shared.Enums;

[DefaultValue(None)]
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum KeysanityMode
{
    [Description("None")]
    None,

    [Description("Both Games")]
    Both,

    [Description("Zelda Only")]
    Zelda,

    [Description("Metroid Only")]
    SuperMetroid,
}
