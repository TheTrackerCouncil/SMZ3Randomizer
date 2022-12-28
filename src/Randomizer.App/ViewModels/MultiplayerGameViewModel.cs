using System;
using System.Globalization;
using System.Windows;
using Randomizer.Shared.Models;

namespace Randomizer.App.ViewModels;

/// <summary>
/// View model for an individual multiplayer game
/// </summary>
public class MultiplayerGameViewModel
{
    public MultiplayerGameViewModel(MultiplayerGameDetails details)
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

    public Visibility GeneratedRomMenuItemVisibility => Details.GeneratedRom != null ? Visibility.Visible : Visibility.Collapsed;
}
