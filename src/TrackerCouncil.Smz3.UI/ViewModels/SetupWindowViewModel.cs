using System;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using AvaloniaControls.Models;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class SetupWindowViewModel : ViewModelBase
{

    [Reactive,
     ReactiveLinkedProperties(nameof(Step2BorderOpacity), nameof(Step3BorderOpacity), nameof(Step4BorderOpacity),
         nameof(Step1ButtonOpacity), nameof(Step2ButtonOpacity), nameof(Step3ButtonOpacity),
         nameof(Step4ButtonOpacity), nameof(DisplayPage1), nameof(DisplayPage2), nameof(DisplayPage3),
         nameof(DisplayPage4), nameof(PreviousButtonEnabled), nameof(SkipStepVisible), nameof(NextButtonText),
         nameof(NextButtonEnabled))]
    public int StepNumber { get; set; } = 2;

    public float Step1ButtonOpacity => StepNumber == 1 ? 1f : 0.2f;
    public float Step2ButtonOpacity => StepNumber == 2 ? 1f : 0.2f;
    public float Step3ButtonOpacity => StepNumber == 3 ? 1f : 0.2f;
    public float Step4ButtonOpacity => StepNumber == 4 ? 1f : 0.2f;

    public float Step1BorderOpacity => 1;
    public float Step2BorderOpacity => StepNumber > 1 ? 1f : 0.2f;
    public float Step3BorderOpacity => StepNumber > 2 ? 1f : 0.2f;
    public float Step4BorderOpacity => StepNumber > 3 ? 1f : 0.2f;

    public bool PreviousButtonEnabled => StepNumber > 1;
    public bool SkipStepVisible => StepNumber == 1;
    public string NextButtonText => StepNumber == 4 ? "Close" : "Next";
    public bool NextButtonEnabled => (StepNumber == 1 && IsValidZeldaRom && IsValidMetroidRom) || StepNumber != 1;

    // Page 1 Properties
    public bool DisplayPage1 => StepNumber == 1;

    [Reactive, ReactiveLinkedProperties(nameof(ZeldaRomIconOpacity), nameof(ZeldaRomBrush), nameof(ZeldaRomIconKind), nameof(NextButtonEnabled))]
    public bool IsValidZeldaRom { get; set; }
    public float ZeldaRomIconOpacity => IsValidZeldaRom ? 1f : 0.2f;
    public IBrush ZeldaRomBrush => IsValidZeldaRom ? Brushes.LimeGreen : Brushes.White;
    public MaterialIconKind ZeldaRomIconKind => IsValidZeldaRom ? MaterialIconKind.CheckCircleOutline : MaterialIconKind.CircleOutline;
    [Reactive] public string ZeldaRomPath { get; set; } = "Not Selected";

    [Reactive, ReactiveLinkedProperties(nameof(MetroidRomIconOpacity), nameof(MetroidRomBrush), nameof(MetroidRomIconKind), nameof(NextButtonEnabled))]
    public bool IsValidMetroidRom { get; set; }
    public float MetroidRomIconOpacity => IsValidMetroidRom ? 1f : 0.2f;
    public IBrush MetroidRomBrush => IsValidMetroidRom ? Brushes.LimeGreen : Brushes.White;
    public MaterialIconKind MetroidRomIconKind => IsValidMetroidRom ? MaterialIconKind.CheckCircleOutline : MaterialIconKind.CircleOutline;
    [Reactive] public string MetroidRomPath { get; set; } = "Not Selected";

    // Page 2 Properties
    public bool DisplayPage2 => StepNumber == 2;
    [Reactive, ReactiveLinkedProperties(nameof(TestAutoTrackerEnabled))] public bool AutoTrackingDisable { get; set; } = true;
    public bool AutoTrackingLua { get; set; }
    public bool AutoTrackingEmoTracker { get; set; }
    [Reactive, ReactiveLinkedProperties(nameof(ConnectorIpAddressTextBoxEnabled))] public bool AutoTrackingUsb2Snes { get; set; }
    [Reactive, ReactiveLinkedProperties(nameof(ConnectorIpAddressTextBoxEnabled))] public bool AutoTrackingSni { get; set; }
    public string ConnectorIpAddress { get; set; } = "";
    public bool ConnectorIpAddressTextBoxEnabled => AutoTrackingUsb2Snes || AutoTrackingSni;
    public bool TestAutoTrackerEnabled => !AutoTrackingDisable && !IsConnecting;
    [Reactive] public IBrush AutoTrackerBrush { get; set; } = Brushes.White;
    [Reactive] public MaterialIconKind AutoTrackerIconKind { get; set; } = MaterialIconKind.CircleOutline;
    [Reactive] public float AutoTrackerOpacity { get; set; } = 0.2f;
    [Reactive] public string AutoTrackerMessage { get; set; } = "";
    [Reactive, ReactiveLinkedProperties(nameof(TestAutoTrackerEnabled))] public bool IsConnecting { get; set; }

    // Page 3 Properties
    public bool DisplayPage3 => StepNumber == 3;
    public bool TrackerVoiceEnabled { get; set; } = true;
    public bool TrackerVoiceDisabled { get; set; }
    public bool TrackerSassEnabled { get; set; } = true;
    public bool TrackerSassDisabled { get; set; }
    public bool TrackerCursingEnabled { get; set; }
    public bool TrackerCursingDisabled { get; set; } = true;
    public bool TrackerBcuEnabled { get; set; }
    public bool TrackerBcuDisabled { get; set; } = true;

    public bool DisplayPage4 => StepNumber == 4;
    public bool DisplayLinuxDesktopButton => OperatingSystem.IsLinux();


}
