using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

/// <summary>
/// Enum for how frequently tracker should voice things
/// </summary>
public enum TrackerVoiceFrequency
{
    [Description("All")]
    All,
    [Description("Disabled")]
    Disabled
}
