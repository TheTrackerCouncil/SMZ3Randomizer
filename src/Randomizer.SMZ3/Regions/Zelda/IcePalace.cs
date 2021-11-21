using System.Linq;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class IcePalace : Z3Region, IHasReward
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5BF
        };
        public IcePalace(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyIP, ItemType.BigKeyIP, ItemType.MapIP, ItemType.CompassIP };

            CompassChest = new Location(this, 256 + 161, 0x1E9D4, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassIP);

            SpikeRoom = new Location(this, 256 + 162, 0x1E9E0, LocationType.Regular,
                name: "Spike Room",
                vanillaItem: ItemType.KeyIP,
                access: items => items.Hookshot || (items.KeyIP >= 1
                                            && CanNotWasteKeysBeforeAccessible(items, MapChest, BigKeyChest)));

            MapChest = new Location(this, 256 + 163, 0x1E9DD, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapIP,
                access: items => items.Hammer && items.CanLiftLight() && (
                    items.Hookshot || (items.KeyIP >= 1
                                       && CanNotWasteKeysBeforeAccessible(items, SpikeRoom, BigKeyChest))
                ));

            BigKeyChest = new Location(this, 256 + 164, 0x1E9A4, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyIP,
                access: items => items.Hammer && items.CanLiftLight() && (
                    items.Hookshot || (items.KeyIP >= 1
                                       && CanNotWasteKeysBeforeAccessible(items, SpikeRoom, MapChest))
                ));

            IcedTRoom = new Location(this, 256 + 165, 0x1E9E3, LocationType.Regular,
                name: "Iced T Room",
                vanillaItem: ItemType.KeyIP);

            FreezorChest = new Location(this, 256 + 166, 0x1E995, LocationType.Regular,
                name: "Freezor Chest",
                vanillaItem: ItemType.ThreeBombs);

            BigChest = new Location(this, 256 + 167, 0x1E9AA, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveTunic,
                access: items => items.BigKeyIP);

            KholdstareReward = new Location(this, 256 + 168, 0x308157, LocationType.Regular,
                name: "Kholdstare",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyIP && items.Hammer && items.CanLiftLight() &&
                    items.KeyIP >= (items.Somaria ? 1 : 2));
        }

        public override string Name => "Ice Palace";

        public Reward Reward { get; set; } = Reward.None;

        public Location CompassChest { get; }

        public Location SpikeRoom { get; }

        public Location MapChest { get; }

        public Location BigKeyChest { get; }

        public Location IcedTRoom { get; }

        public Location FreezorChest { get; }

        public Location BigChest { get; }

        public Location KholdstareReward { get; }

        public override bool CanEnter(Progression items)
        {
            return items.MoonPearl && items.Flippers && items.CanLiftHeavy() && items.CanMeltFreezors();
        }

        public bool CanComplete(Progression items)
        {
            return KholdstareReward.IsAvailable(items);
        }

        private bool CanNotWasteKeysBeforeAccessible(Progression items, params Location[] locations)
        {
            return !items.BigKeyIP || locations.Any(l => l.ItemIs(ItemType.BigKeyIP, World));
        }
    }
}
