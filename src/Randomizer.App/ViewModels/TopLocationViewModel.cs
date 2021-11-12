using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Randomizer.SMZ3;

namespace Randomizer.App.ViewModels
{
    public class TopLocationViewModel
    {
        private readonly Region _region;
        private readonly Progression _progression;

        public TopLocationViewModel(int treasureCount, Region region, Progression progression)
        {
            TreasureCount = treasureCount;
            _region = region;
            _progression = progression;

            Sprite = new BitmapImage(new Uri(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Sprites", "Items", "chest.png")));
        }

        public ImageSource Sprite { get; }

        public int TreasureCount { get; }

        public string Region => _region.Name;

        public string Locations
        {
            get
            {
                return string.Join(", ", _region.Locations
                    .Where(x => x.IsAvailable(_progression))
                    .Select(x => x.Name));
            }
        }
    }
}
