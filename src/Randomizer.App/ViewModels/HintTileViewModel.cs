using Randomizer.Data.WorldData;
using Randomizer.Shared.Models;

namespace Randomizer.App.ViewModels;

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
