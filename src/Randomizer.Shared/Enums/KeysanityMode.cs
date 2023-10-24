using System.ComponentModel;

namespace Randomizer.Shared.Enums;

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
