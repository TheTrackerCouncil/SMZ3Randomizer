using System.Collections.Generic;
using Avalonia.Media;
using AvaloniaControls.Models;
using Material.Icons;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared.Models;
using ReactiveUI.Fody.Helpers;
using SnesConnectorLibrary;

namespace Randomizer.CrossPlatform.ViewModels;

public class TrackerWindowViewModel : ViewModelBase
{
    public List<TrackerWindowPanelViewModel> Panels { get; set; } = [];

    public IBrush Background { get; set; } = new SolidColorBrush(new Color(255, 0, 0, 0));

    public MaterialIconKind ConnectedIcon => AutoTrackerEnabled ? MaterialIconKind.Link : MaterialIconKind.LinkOff;

    public IBrush? ConnectedColor =>
        AutoTrackerConnected ? Brushes.LimeGreen : AutoTrackerEnabled ? Brushes.White : Brushes.Firebrick;

    [Reactive] public string TimeString { get; set; } = "";

    [Reactive]
    [ReactiveLinkedProperties(nameof(ConnectedIcon), nameof(ConnectedColor))]
    public bool AutoTrackerEnabled { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(ConnectedColor))]
    public bool AutoTrackerConnected { get; set; }

    [Reactive]
    public SnesConnectorType ConnectorType { get; set; }

    public GeneratedRom? Rom { get; set; }

    public List<UILayout> Layouts { get; set; } = [];

    public bool OpenTrackWindow { get; set; }
}
