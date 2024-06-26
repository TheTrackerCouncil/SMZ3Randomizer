using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum HistoryEventType
{
    None,
    [Description("Entered")]
    EnteredRegion,
    [Description("Picked up")]
    TrackedItem,
    [Description("Defeated")]
    BeatBoss
}
