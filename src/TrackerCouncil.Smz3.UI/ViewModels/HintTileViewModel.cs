using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class HintTileViewModel(PlayerHintTile? hintTile)
{
    public PlayerHintTile? PlayerHintTile { get; init; } = hintTile;
    public string Name { get; init; } = hintTile?.LocationKey ?? "";
    public string Quality { get; init; } = hintTile?.Usefulness?.ToString() ?? "";
}
