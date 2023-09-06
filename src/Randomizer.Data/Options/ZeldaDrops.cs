using System.ComponentModel;

namespace Randomizer.Data.Options;

public enum ZeldaDrops
{
    [Description("Randomized")]
    Randomized,
    [Description("Vanilla")]
    Vanilla,
    [Description("Easy")]
    Easy,
    [Description("Difficult")]
    Difficult,
}
