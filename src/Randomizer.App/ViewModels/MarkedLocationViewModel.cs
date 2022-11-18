using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Accessibility;
using Randomizer.Data.WorldData;
using Randomizer.SMZ3;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.App.ViewModels
{
    public class MarkedLocationViewModel
    {
        private readonly Location _location;
        private readonly Item _itemData;
        private readonly TrackerLocationSyncer _syncer;

        public MarkedLocationViewModel(Location location, Item itemData, string itemSprite, TrackerLocationSyncer syncer)
        {
            _location = location;
            _itemData = itemData;
            _syncer = syncer;
            ItemSprite = itemSprite != null ? new BitmapImage(new Uri(itemSprite)) : null;
        }

        public ImageSource ItemSprite { get; }

        public double Opacity => _syncer.ShowOutOfLogicLocations || _syncer.WorldService.IsAvailable(_location) ? 1.0 : 0.33;

        public string Item => _itemData.Name;

        public string Location => _location.Metadata.Name[0];

        public string Area => _location.Region.Metadata.Name[0];
    }
}
