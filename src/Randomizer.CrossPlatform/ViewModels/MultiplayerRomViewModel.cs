using System;
using System.Globalization;
using Randomizer.Shared.Models;

namespace Randomizer.CrossPlatform.ViewModels;

public class MultiplayerRomViewModel
{
    public MultiplayerRomViewModel(MultiplayerGameDetails details)
    {
        Details = details;
    }

    public MultiplayerGameDetails Details { get; }

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
