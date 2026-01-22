using System.Collections.Generic;
using System.Collections.ObjectModel;
using AvaloniaControls.Models;
using ReactiveUI.SourceGenerators;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class TrackerLocationsViewModel : ViewModelBase
{
    [Reactive] public partial List<HintTileViewModel> HintTiles { get; set; } = [];
    [Reactive] public partial ObservableCollection<MarkedLocationViewModel> MarkedLocations { get; set; } = [];
    [Reactive] public partial List<RegionViewModel> Regions { get; set; } = [];
    [Reactive] public partial RegionFilter Filter { get; set; }
    [Reactive] public partial bool ShowOutOfLogic { get; set; }
    [Reactive] public partial bool FinishedLoading { get; set; }
}
