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
    [Reactive] public List<TrackerWindowPanelViewModel> Panels { get; set; } = [];

    public string LayoutName { get; set; } = "";

    public IBrush Background { get; set; } = new SolidColorBrush(new Color(255, 0, 0, 0));

    public MaterialIconKind ConnectedIcon => AutoTrackerEnabled ? MaterialIconKind.Link : MaterialIconKind.LinkOff;

    public IBrush? ConnectedColor =>
        AutoTrackerConnected ? Brushes.LimeGreen : AutoTrackerEnabled ? Brushes.White : Brushes.Firebrick;

    public IBrush StatusBarBackground => IsInGoMode ? Brushes.Green : new SolidColorBrush(new Color(255, 45, 45, 45));

    public IBrush StatusBarBorder => IsInGoMode ? Brushes.Transparent : new SolidColorBrush(new Color(255, 53, 53, 53));

    public bool PegWorldMode { get; set; }
    public bool ShaktoolMode { get; set; }

    [Reactive] public string TimeString { get; set; } = "00:00";

    [Reactive]
    [ReactiveLinkedProperties(nameof(ConnectedIcon), nameof(ConnectedColor))]
    public bool AutoTrackerEnabled { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(ConnectedColor))]
    public bool AutoTrackerConnected { get; set; }

    [Reactive] public SnesConnectorType ConnectorType { get; set; }

    [Reactive]
    [ReactiveLinkedProperties(nameof(StatusBarBackground), nameof(StatusBarBorder))]
    public bool IsInGoMode { get; set; }

    [Reactive] public bool ShowSpeechRecognition { get; set; } = true;

    [Reactive] public string SpeechConfidence { get; set; } = "Voice Disabled";

    [Reactive] public string SpeechPhrase { get; set; } = "";

    [Reactive]
    [ReactiveLinkedProperties(nameof(SpeechToolTip), nameof(SpeechIcon))]
    public bool VoiceEnabled { get; set; }

    public string SpeechToolTip => VoiceEnabled ? "Confidence of last recognized voice command. Double click to disable voice recognition." : "Voice recognition disabled. Double click to attempt to enable voice recognition.";

    public MaterialIconKind SpeechIcon => VoiceEnabled ? MaterialIconKind.Microphone : MaterialIconKind.MicOff;

    [Reactive]
    [ReactiveLinkedProperties(nameof(IsDisabledConnector), nameof(IsSniConnector), nameof(IsUsb2SnesConnector), nameof(IsLuaConnector), nameof(IsLuaCrowdControlConnector), nameof(IsLuaEmoTrackerConnector))]
    public SnesConnectorType SnesConnectorType { get; set; } = SnesConnectorType.None;

    public bool IsDisabledConnector => SnesConnectorType == SnesConnectorType.None;
    public bool IsSniConnector => SnesConnectorType == SnesConnectorType.Sni;
    public bool IsUsb2SnesConnector => SnesConnectorType == SnesConnectorType.Usb2Snes;
    public bool IsLuaConnector => SnesConnectorType == SnesConnectorType.Lua;
    public bool IsLuaCrowdControlConnector => SnesConnectorType == SnesConnectorType.LuaCrowdControl;
    public bool IsLuaEmoTrackerConnector => SnesConnectorType == SnesConnectorType.LuaEmoTracker;

    public GeneratedRom? Rom { get; set; }

    public List<UILayout> Layouts { get; set; } = [];

    public bool OpenTrackWindow { get; set; }

    public UILayout? PrevLayout { get; set; }

    public UILayout? CurrentLayout { get; set; }
}
