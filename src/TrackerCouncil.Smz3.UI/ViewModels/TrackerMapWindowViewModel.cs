using System.Collections.Generic;
using System.IO;
using Avalonia;
using AvaloniaControls.Models;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class TrackerMapWindowViewModel : ViewModelBase
{
    public List<TrackerMap> Maps { get; set; } = new();

    [Reactive]
    [ReactiveLinkedProperties(nameof(MainImage))]
    public TrackerMap? SelectedMap { get; set; }

    public string MainImage => SelectedMap == null
        ? ""
        : Path.Combine(Directories.SpritePath, "Maps", SelectedMap.Image);

    [Reactive] public List<TrackerMapLocationViewModel> Locations { get; set; } = [];
    [Reactive] public bool FinishedLoading { get; set; }

    public Size GridSize { get; set; }

    public Size MapSize { get; set; }

    public bool ShowOutOfLogicLocations { get; set; }
}
