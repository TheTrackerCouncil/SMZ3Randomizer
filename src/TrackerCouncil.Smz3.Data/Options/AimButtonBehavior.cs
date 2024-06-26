using System.ComponentModel;

namespace TrackerCouncil.Smz3.Data.Options;

public enum AimButtonBehavior
{
    [Description("Separate Aim Up/Down Buttons (Vanilla)")]
    Vanilla,
    [Description("Unified Aim Button & Quick Morph")]
    UnifiedAim,
}
