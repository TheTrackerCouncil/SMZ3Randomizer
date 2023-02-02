using System.ComponentModel;

namespace Randomizer.Data.Options;

/// <summary>
/// Enum for the different behaviors for pressing the item cancel button in SM
/// </summary>
public enum ItemCancelBehavior
{
    [Description("Deselects item (Vanilla)")]
    Vanilla,
    [Description("Hold to keep supers/power bombs selected.")]
    HoldSupersOnly,
    [Description("Hold to keep missiles/supers/pbs selected. Item select changes weapon.")]
    Hold,
    [Description("Press to toggle supers/pbs on or off")]
    Toggle
}
