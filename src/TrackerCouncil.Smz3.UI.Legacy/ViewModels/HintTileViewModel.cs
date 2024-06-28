using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.UI.Legacy.ViewModels;

public class HintTileViewModel
{
    public HintTileViewModel(PlayerHintTile hintTile)
    {
        PlayerHintTile = hintTile;
    }

    public PlayerHintTile PlayerHintTile { get; init; }

    public string Name => PlayerHintTile.LocationKey;
    public string Quality => PlayerHintTile.Usefulness?.ToString() ?? "";
}
