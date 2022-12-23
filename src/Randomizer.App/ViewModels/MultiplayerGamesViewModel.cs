using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Randomizer.Shared.Models;

namespace Randomizer.App.ViewModels;

/// <summary>
/// View model for the list of multiplayer games
/// </summary>
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

