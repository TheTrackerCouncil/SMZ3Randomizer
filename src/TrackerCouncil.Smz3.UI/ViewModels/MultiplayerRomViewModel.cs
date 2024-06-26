using System;
using System.Globalization;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class MultiplayerRomViewModel(MultiplayerGameDetails details)
{
    public MultiplayerGameDetails Details { get; } = details;

    public string TypeLabel => $"Type: {Details.Type}";

    public string StatusLabel => $"Status: {Details.Status}";

    public string TimeLabel
    {
        get
        {
            return $"{Details.JoinedDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern)} {Details.JoinedDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern)}";
        }
    }

    public string ElapsedLabel
    {
        get
        {
            if (Details.GeneratedRom?.TrackerState == null) return "";
            var timeSpan = TimeSpan.FromSeconds(Details.GeneratedRom.TrackerState.SecondsElapsed);
            var duration = timeSpan.Hours > 0
                ? timeSpan.ToString("h':'mm':'ss")
                : timeSpan.ToString("mm':'ss");
            return $"Duration: {duration}";
        }
    }

    public bool IsGeneratedRomMenuItemVisibile => Details.GeneratedRom != null;
}
