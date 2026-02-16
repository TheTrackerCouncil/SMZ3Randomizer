using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum AutoTrackingMode
{
    [Description("Location-based - more sass")]
    Locations,
    [Description("(Experimental) Inventory-based for ALttPO - less wordy")]
    Inventory
}
