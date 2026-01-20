using System.Collections.Generic;
using System.IO;
using Avalonia;
using AvaloniaControls.Models;
using ReactiveUI.SourceGenerators;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class TrackerMapWindowViewModel : ViewModelBase
{
    public List<TrackerMap> Maps { get; set; } = new();

    [Reactive]
    [ReactiveLinkedProperties(nameof(MainImage))]
    public partial TrackerMap? SelectedMap { get; set; }

    public string MainImage => SelectedMap == null
        ? ""
        : Path.Combine(Directories.SpritePath, "Maps", SelectedMap.Image);

    [Reactive] public partial List<TrackerMapLocationViewModel> Locations { get; set; } = [];
    [Reactive] public partial bool FinishedLoading { get; set; }

    public Size GridSize { get; set; }

    public Size MapSize { get; set; }

    public bool ShowOutOfLogicLocations { get; set; }
}
