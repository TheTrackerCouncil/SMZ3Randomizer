using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Randomizer.SMZ3;

namespace Randomizer.App.ViewModels
{
    public class LocationViewModel
    {
        private readonly Location _location;
        private readonly Action _onClear;

        public LocationViewModel(Location location, Action onClear)
        {
            _location = location;
            _onClear = onClear;
        }

        public string Name => _location.Room != null ? $"{_location.Room.Name} - {_location.Name}" : _location.Name;

        public string Area => _location.Region.ToString();

        public ICommand Clear => new DelegateCommand(
            execute: () =>
            {
                _location.Cleared = true;
                _onClear();
            },
            canExecute: () => _location.Cleared == false);
    }
}
