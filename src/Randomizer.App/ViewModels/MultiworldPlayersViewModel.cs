using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Randomizer.Data.Multiworld;

namespace Randomizer.App.ViewModels
{
    public class MultiworldPlayersViewModel : INotifyPropertyChanged
    {
        public MultiworldPlayersViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Players = new List<MultiworldPlayerStateViewModel>()
                {
                    new MultiworldPlayerStateViewModel(new MultiworldPlayerState
                    {
                        Guid = "Player One",
                        PlayerName = "Player One",
                    }, true),
                    new MultiworldPlayerStateViewModel(new MultiworldPlayerState
                    {
                        Guid = "Player Two",
                        PlayerName = "Player Two",
                    }, false),
                };
            }
            else
            {
                Players = new();
            }
        }

        public List<MultiworldPlayerStateViewModel> Players { get; private set; }

        public void UpdateList(List<MultiworldPlayerState> players, MultiworldPlayerState? localPlayer)
        {
            Players = players.Select(x => new MultiworldPlayerStateViewModel(x, x == localPlayer)).ToList();
            OnPropertyChanged();
        }

        public void UpdatePlayer(MultiworldPlayerState player)
        {
            var playerViewModel = Players.FirstOrDefault(x => x.PlayerGuid == player.Guid);
            if (playerViewModel != null)
                playerViewModel.Update(player);
            else
            {
                Players.Add(new MultiworldPlayerStateViewModel(player, false));
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
