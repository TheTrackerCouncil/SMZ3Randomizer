using System.Collections.Generic;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class SkullWoods : Z3Region, IHasReward
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5BA,
            0x02D5BB,
            0x02D5BC,
            0x02D5BD,
            0x02D608,
            0x02D609,
            0x02D60A,
            0x02D60B
        };

        public SkullWoods(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeySW, ItemType.BigKeySW, ItemType.MapSW, ItemType.CompassSW };

            PotPrison = new Location(this, 256 + 145, 0x1E9A1, LocationType.Regular,
                name: "Pot Prison",
                vanillaItem: ItemType.KeySW);

            CompassChest = new Location(this, 256 + 146, 0x1E992, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassSW);

            BigChest = new Location(this, 256 + 147, 0x1E998, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Firerod,
                access: items => items.BigKeySW)
                .AlwaysAllow((item, items) => item.Is(ItemType.BigKeySW, World));

            MapChest = new Location(this, 256 + 148, 0x1E99B, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapSW);

            PinballRoom = new Location(this, 256 + 149, 0x1E9C8, LocationType.Regular,
                name: "Pinball Room",
                vanillaItem: ItemType.KeySW)
                .Allow((item, items) => item.Is(ItemType.KeySW, World));

            BigKeyChest = new Location(this, 256 + 150, 0x1E99E, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeySW);

            BridgeRoom = new Location(this, 256 + 151, 0x1E9FE, LocationType.Regular,
                name: "Bridge Room",
                vanillaItem: ItemType.KeySW,
                access: items => items.Firerod);

            MothulaReward = new Location(this, 256 + 152, 0x308155, LocationType.Regular,
                name: "Mothula",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.Firerod && items.Sword && items.KeySW >= 3);
        }

        public override string Name => "Skull Woods";

        public override List<string> AlsoKnownAs { get; }
            = new() { "Skill Woods" };

        public Reward Reward { get; set; } = Reward.None;

        public Location PotPrison { get; }

        public Location CompassChest { get; }

        public Location BigChest { get; }

        public Location MapChest { get; }

        public Location PinballRoom { get; }

        public Location BigKeyChest { get; }

        public Location BridgeRoom { get; }

        public Location MothulaReward { get; }

        public override bool CanEnter(Progression items)
        {
            return items.MoonPearl && World.DarkWorldNorthWest.CanEnter(items);
        }

        public bool CanComplete(Progression items)
        {
            return MothulaReward.IsAvailable(items);
        }
    }
}
