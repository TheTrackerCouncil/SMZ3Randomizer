using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class HintTileViewModel : ViewModelBase
{
    public HintTileViewModel(PlayerHintTile hintTile)
    {
        PlayerHintTile = hintTile;
        IsVisible = hintTile.HintState == HintState.Viewed;
        PlayerHintTile.HintStateUpdated += (_, _) =>
        {
            IsVisible = hintTile.HintState == HintState.Viewed;
        };
    }

    public PlayerHintTile PlayerHintTile { get; }
    public string Name => PlayerHintTile.LocationKey;
    public string Quality => PlayerHintTile.Usefulness.ToString() ?? "";
    [Reactive] public bool IsVisible { get; set; }
}
