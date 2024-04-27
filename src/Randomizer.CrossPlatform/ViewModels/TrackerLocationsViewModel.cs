using System.Collections.Generic;
using Randomizer.Shared.Enums;
using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class TrackerLocationsViewModel : ViewModelBase
{
    [Reactive] public List<HintTileViewModel> HintTiles { get; set; } = [];
    [Reactive] public List<MarkedLocationViewModel> MarkedLocations { get; set; } = [];
    [Reactive] public List<RegionViewModel> Regions { get; set; } = [];
    [Reactive] public RegionFilter Filter { get; set; }
    [Reactive] public bool ShowOutOfLogic { get; set; }
}
