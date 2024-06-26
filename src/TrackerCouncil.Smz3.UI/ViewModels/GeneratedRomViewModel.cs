using System;
using System.Globalization;
using ReactiveUI.Fody.Helpers;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class GeneratedRomViewModel(GeneratedRom rom) : ViewModelBase
{
    public GeneratedRom Rom { get; } = rom;

    public string Name => Rom.Label;

    public string NameLabel => string.IsNullOrEmpty(Rom.Label) ? $"Seed: {Rom.Seed}" : Rom.Label;

    public string LocationsLabel
    {
        get
        {
            if (Rom.TrackerState == null) return "";
            return $"Cleared: {Rom.TrackerState.PercentageCleared}%";
        }
    }

    public bool IsProgressionLogVisible => Rom.TrackerState?.History.Count > 0;

    [Reactive] public bool IsEditTextBoxVisible { get; set; }

    public string TimeLabel
    {
        get
        {
            return $"{Rom.Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern)} {Rom.Date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern)}";
        }
    }

    public string ElapsedLabel
    {
        get
        {
            if (Rom.TrackerState == null) return "";
            var timeSpan = TimeSpan.FromSeconds(Rom.TrackerState.SecondsElapsed);
            var duration = timeSpan.Hours > 0
                ? timeSpan.ToString("h':'mm':'ss")
                : timeSpan.ToString("mm':'ss");
            return $"Duration: {duration}";
        }
    }
}
