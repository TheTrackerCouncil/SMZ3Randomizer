using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using Accessibility;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.FileData;

namespace Randomizer.App.ViewModels;

public class MultiplayerGamesViewModel : INotifyPropertyChanged
{
    public List<MultiplayerGameViewModel> Games { get; set; }

    public MultiplayerGamesViewModel()
    {
        Games = new List<MultiplayerGameViewModel>();
    }

    public void UpdateList(ICollection<MultiplayerGameDetails> details)
    {
        Games = details.Select(x => new MultiplayerGameViewModel(x)).ToList();
        OnPropertyChanged();
    }

    public Visibility GamesVisibility => Games.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

    public Visibility IntroVisibility => Games.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class MultiplayerGameViewModel
{
    public MultiplayerGameViewModel(MultiplayerGameDetails details)
    {
        Details = details;
    }

    public MultiplayerGameDetails Details { get; set; }

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
