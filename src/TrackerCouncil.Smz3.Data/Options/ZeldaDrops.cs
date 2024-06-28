using System.ComponentModel;

namespace TrackerCouncil.Smz3.Data.Options;

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
