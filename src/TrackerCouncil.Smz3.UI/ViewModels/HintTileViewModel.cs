using System.Collections.Generic;
using System.Linq;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class HintTileViewModel : ViewModelBase
{
    private List<Location> _locations;

    public HintTileViewModel(PlayerHintTile hintTile, IEnumerable<Location> locations)
    {
        PlayerHintTile = hintTile;
        IsVisible = hintTile.HintState == HintState.Viewed;

        PlayerHintTile.HintStateUpdated += (_, _) =>
        {
            IsVisible = hintTile.HintState == HintState.Viewed;
        };

        _locations = locations.ToList();

        foreach (var location in _locations)
        {
            location.AccessibilityUpdated += (_, _) =>
            {
                UpdateOpacity();
            };
        }

        UpdateOpacity();
    }

    private void UpdateOpacity()
    {
        Opacity = _locations.Any(x => x.Accessibility is Accessibility.Available or Accessibility.AvailableWithKeys)
            ? 1.0
            : 0.33;
    }

    public PlayerHintTile PlayerHintTile { get; }
    public string Name => PlayerHintTile.LocationKey;
    public string Quality => PlayerHintTile.Usefulness.ToString() ?? "";
    [Reactive] public bool IsVisible { get; set; }
    [Reactive] public double Opacity { get; set; }
}
