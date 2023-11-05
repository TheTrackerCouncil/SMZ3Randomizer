using Randomizer.Abstractions;
using Randomizer.Data.Options;
using Randomizer.Shared.Enums;

namespace Randomizer.SMZ3.GameModes;

public class GameModeBaseDefault : GameModeBase
{
    public override GameModeType GameModeType => GameModeType.Default;
    public override string Name => "Default";
    public override string Description => "Default";
}
