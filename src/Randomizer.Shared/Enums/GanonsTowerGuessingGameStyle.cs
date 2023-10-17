using System.ComponentModel;

namespace Randomizer.Shared.Enums;

public enum GanonsTowerGuessingGameStyle
{
    [Description("Exact guesses only")]
    RequireExact,

    [Description("Closest without going over")]
    ClosestWithoutGoingOver
}
