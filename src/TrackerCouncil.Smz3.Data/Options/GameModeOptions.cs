using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.Options;

public class GameModeOptions
{
    public GameModeType SelectedGameModeType { get; set; }
    public int NumSpazersInPool { get; set; } = 30;
    public int RequiredSpazerCount { get; set; } = 20;
}
