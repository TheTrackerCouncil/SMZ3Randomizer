using System.ComponentModel;

namespace Randomizer.Data.Options;

public enum RunButtonBehavior
{
    [Description("Hold button to run (Vanilla)")]
    Vanilla,
    [Description("Automatically run and hold button to walk")]
    AutoRun,
}
