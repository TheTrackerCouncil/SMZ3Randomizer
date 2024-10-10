using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class TrackerLocationsViewModel : ViewModelBase
{
    [Reactive] public List<HintTileViewModel> HintTiles { get; set; } = [];
    [Reactive] public ObservableCollection<MarkedLocationViewModel> MarkedLocations { get; set; } = [];
    [Reactive] public List<RegionViewModel> Regions { get; set; } = [];
    [Reactive] public RegionFilter Filter { get; set; }
    [Reactive] public bool ShowOutOfLogic { get; set; }
}
