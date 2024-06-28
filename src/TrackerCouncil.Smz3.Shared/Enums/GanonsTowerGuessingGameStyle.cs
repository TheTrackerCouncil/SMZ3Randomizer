using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum GanonsTowerGuessingGameStyle
{
    [Description("Exact guesses only")]
    RequireExact,

    [Description("Closest without going over")]
    ClosestWithoutGoingOver
}
