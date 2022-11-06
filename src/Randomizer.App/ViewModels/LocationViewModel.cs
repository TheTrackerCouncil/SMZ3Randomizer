using System.Windows.Input;
using Randomizer.Data.WorldData;

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

        public string Name => _location.Metadata.Name[0];

        public string Area => _location.Region.Metadata.Name[0];

        public bool InLogic => _location.IsAvailable(_syncer.ItemService.GetProgression(false));

        public bool InLogicWithKeys => !InLogic && _location.IsAvailable(_syncer.ItemService.GetProgression(true));

        public double Opacity => (InLogic || InLogicWithKeys) ? 1.0 : 0.33;

        public ICommand Clear => new DelegateCommand(
            execute: () =>
            {
                _syncer.Tracker.Clear(_location);
            },
            canExecute: () => !_location.State.Cleared);
    }
}
