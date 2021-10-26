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
        private readonly Progression _progression;
        private readonly Progression _progressionWithKeys;
        private readonly Action _onClear;

        public LocationViewModel(Location location, Progression progression, Progression progressionWithKeys, Action onClear)
        {
            _location = location;
            _progression = progression;
            _progressionWithKeys = progressionWithKeys;
            _onClear = onClear;
        }

        public string Name => _location.Room != null ? $"{_location.Room.Name} - {_location.Name}" : _location.Name;

        public string Area => _location.Region.ToString();

        public bool InLogic => _location.IsAvailable(_progression);

        public bool InLogicWithKeys => !InLogic && _location.IsAvailable(_progressionWithKeys);

        public double Opacity => (InLogic || InLogicWithKeys) ? 1.0 : 0.33;

        public ICommand Clear => new DelegateCommand(
            execute: () =>
            {
                _location.Cleared = true;
                _onClear();
            },
            canExecute: () => _location.Cleared == false);
    }
}
