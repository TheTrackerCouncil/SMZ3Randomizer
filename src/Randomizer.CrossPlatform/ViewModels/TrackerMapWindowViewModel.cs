using System.Collections.Generic;
using System.IO;
using Avalonia;
using AvaloniaControls.Models;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class TrackerMapWindowViewModel : ViewModelBase
{
    public List<TrackerMap> Maps { get; set; } = new();

    [Reactive]
    [ReactiveLinkedProperties(nameof(MainImage))]
    public TrackerMap? SelectedMap { get; set; }

    public string MainImage => SelectedMap == null
        ? ""
        : Path.Combine(Sprite.SpritePath, "Maps", SelectedMap.Image);

    [Reactive] public List<TrackerMapLocationViewModel> Locations { get; set; } = [];

    public Size GridSize { get; set; }

    public Size MapSize { get; set; }
}
