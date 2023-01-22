using System.ComponentModel;

namespace Randomizer.Data.Options;

/// <summary>
/// Enum for the different behaviors for pressing the item cancel button in SM
/// </summary>
public enum ItemCancelBehavior
{
    [Description("Press to deselect item (Vanilla)")]
    Vanilla,
    [Description("Hold and press shoot to fire supers/power bombs")]
    Hold,
    [Description("Press to toggle supers/pbs on or off")]
    Toggle
}
