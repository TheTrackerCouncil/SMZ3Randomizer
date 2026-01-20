using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using AvaloniaControls.Models;
using Material.Icons;
using ReactiveUI.SourceGenerators;
using SnesConnectorLibrary;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class TrackerWindowViewModel : ViewModelBase
{
    [Reactive] public partial List<TrackerWindowPanelViewModel> Panels { get; set; } = [];

    public string LayoutName { get; set; } = "";

    public IBrush Background { get; set; } = new SolidColorBrush(new Color(255, 0, 0, 0));

    public MaterialIconKind ConnectedIcon => AutoTrackerEnabled ? MaterialIconKind.Link : MaterialIconKind.LinkOff;

    public IBrush? ConnectedColor =>
        AutoTrackerConnected ? Brushes.LimeGreen : AutoTrackerEnabled ? Brushes.White : Brushes.Firebrick;

    public IBrush StatusBarBackground => IsInGoMode ? Brushes.Green : new SolidColorBrush(new Color(255, 45, 45, 45));

    public IBrush StatusBarBorder => IsInGoMode ? Brushes.Transparent : new SolidColorBrush(new Color(255, 53, 53, 53));

    public bool AddShadows { get; set; }
    public bool PegWorldMode { get; set; }
    public bool ShaktoolMode { get; set; }

    [Reactive] public partial string TimeString { get; set; } = "00:00";

    [Reactive]
    [ReactiveLinkedProperties(nameof(ConnectedIcon), nameof(ConnectedColor))]
    public partial bool AutoTrackerEnabled { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(ConnectedColor))]
    public partial bool AutoTrackerConnected { get; set; }

    [Reactive] public partial SnesConnectorType ConnectorType { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(StatusBarBackground), nameof(StatusBarBorder))]
    public partial bool IsInGoMode { get; set; }

    public bool ShowSpeechRecognition => !OperatingSystem.IsMacOS();

    [Reactive] public partial string SpeechConfidence { get; set; } = "Voice Disabled";

    [Reactive] public partial string SpeechPhrase { get; set; } = "";

    [Reactive]
    [ReactiveLinkedProperties(nameof(SpeechToolTip), nameof(SpeechIcon))]
    public partial bool VoiceEnabled { get; set; }
    public bool DisplayTimer { get; set; }

    public int IdealWindowWidth => Panels.Max(x => x.Column) * 34 + 10;
    public int IdealWindowHeight => Panels.Max(x => x.Row) * 34 + 10 + 50;
    [Reactive] public partial bool ShowResizeButton { get; set; }

    public string SpeechToolTip => VoiceEnabled ? "Confidence of last recognized voice command. Double click to disable voice recognition." : "Voice recognition disabled. Double click to attempt to enable voice recognition.";

    public MaterialIconKind SpeechIcon => VoiceEnabled ? MaterialIconKind.Microphone : MaterialIconKind.MicOff;

    [Reactive]
    [ReactiveLinkedProperties(nameof(IsDisabledConnector), nameof(IsSniConnector), nameof(IsUsb2SnesConnector), nameof(IsLuaConnector), nameof(IsLuaCrowdControlConnector), nameof(IsLuaEmoTrackerConnector))]
    public partial SnesConnectorType SnesConnectorType { get; set; } = SnesConnectorType.None;

    [Reactive] public partial bool AreCheatsEnabled { get; set; }

    public bool IsDisabledConnector => SnesConnectorType == SnesConnectorType.None;
    public bool IsSniConnector => SnesConnectorType == SnesConnectorType.Sni;
    public bool IsUsb2SnesConnector => SnesConnectorType == SnesConnectorType.Usb2Snes;
    public bool IsLuaConnector => SnesConnectorType == SnesConnectorType.Lua;
    public bool IsLuaCrowdControlConnector => SnesConnectorType == SnesConnectorType.LuaCrowdControl;
    public bool IsLuaEmoTrackerConnector => SnesConnectorType == SnesConnectorType.LuaEmoTracker;

    public GeneratedRom? Rom { get; set; }

    public List<UILayout> Layouts { get; set; } = [];

    public bool OpenTrackWindow { get; set; }

    public bool OpenSpeechWindow { get; set; }

    public UILayout? PrevLayout { get; set; }

    public UILayout? CurrentLayout { get; set; }
}
