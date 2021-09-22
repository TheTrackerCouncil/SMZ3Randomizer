using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Randomizer.SMZ3
{
    public abstract class Region
    {
        public abstract string Name { get; }
        public virtual string Area => Name;

        public virtual List<string> AlsoKnownAs { get; } = new();

        public List<Location> Locations { get; set; }
        public World World { get; set; }
        public int Weight { get; set; } = 0;

        protected Config Config { get; set; }
        protected IList<ItemType> RegionItems { get; set; } = new List<ItemType>();

        private Dictionary<string, Location> locationLookup { get; set; }
        public Location GetLocation(string name) => locationLookup[name];

        protected Region(World world, Config config)
        {
            Config = config;
            World = world;
            locationLookup = new Dictionary<string, Location>();
        }

        public void GenerateLocationLookup()
        {
            locationLookup = Locations.ToDictionary(l => l.Name, l => l);
        }

        public bool IsRegionItem(Item item)
        {
            return RegionItems.Contains(item.Type);
        }

        public virtual bool CanFill(Item item, Progression items)
        {
            return Config.Keysanity || !item.IsDungeonItem || IsRegionItem(item);
        }

        public virtual bool CanEnter(Progression items)
        {
            return true;
        }
    }
}
