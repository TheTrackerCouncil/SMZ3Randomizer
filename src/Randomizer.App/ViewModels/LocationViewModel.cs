using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Randomizer.Data.WorldData;
using Randomizer.SMZ3;

namespace Randomizer.App.ViewModels
{
    public class LocationViewModel
    {
        private readonly Location _location;
        private readonly TrackerLocationSyncer _syncer;

        public LocationViewModel(Location location, TrackerLocationSyncer syncer)
        {
            _location = location;
            _syncer = syncer;
        }

        public string Name => _syncer.GetName(_location);

        public string Area => _syncer.GetName(_location.Region);

        public bool InLogic => _location.IsAvailable(_syncer.Progression);

        public bool InLogicWithKeys => !InLogic && _location.IsAvailable(_syncer.ProgressionWithKeys);

        public double Opacity => (InLogic || InLogicWithKeys) ? 1.0 : 0.33;

        public ICommand Clear => new DelegateCommand(
            execute: () =>
            {
                _syncer.ClearLocation(_location);
            },
            canExecute: () => _location.State.Cleared == false);
    }
}
