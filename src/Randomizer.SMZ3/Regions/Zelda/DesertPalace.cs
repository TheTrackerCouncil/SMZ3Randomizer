using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class DesertPalace : Z3Region, IHasReward
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D59B,
            0x02D59C,
            0x02D59D,
            0x02D59E
        };

        public DesertPalace(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyDP, ItemType.BigKeyDP, ItemType.MapDP, ItemType.CompassDP };

            BigChest = new Location(this, 256 + 109, 0x1E98F, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveGlove,
                access: items => items.BigKeyDP);

            TorchItem = new Location(this, 256 + 110, 0x308160, LocationType.Regular,
                name: "Torch",
                vanillaItem: ItemType.KeyDP,
                access: items => items.Boots);

            MapChest = new Location(this, 256 + 111, 0x1E9B6, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapDP);

            BigKeyChest = new Location(this, 256 + 112, 0x1E9C2, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyDP,
                access: items => items.KeyDP);

            CompassChest = new Location(this, 256 + 113, 0x1E9CB, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassDP,
                access: items => items.KeyDP);

            LanmolasReward = new Location(this, 256 + 114, 0x308151, LocationType.Regular,
                name: "Lanmolas",
                vanillaItem: ItemType.HeartContainer,
                access: items => (
                    World.AdvancedLogic.CanLiftLight(items) ||
                    (World.AdvancedLogic.CanAccessMiseryMirePortal(items) && items.Mirror)
                ) && items.BigKeyDP && items.KeyDP && World.AdvancedLogic.CanLightTorches(items) && CanBeatBoss(items));
        }

        public override string Name => "Desert Palace";

        public override List<string> AlsoKnownAs { get; }
            = new() { "Dessert Palace" };

        public Reward Reward { get; set; } = Reward.None;

        public Location BigChest { get; }

        public Location TorchItem { get; }

        public Location MapChest { get; }

        public Location BigKeyChest { get; }

        public Location CompassChest { get; }

        public Location LanmolasReward { get; }

        public override bool CanEnter(Progression items)
        {
            return items.Book ||
                items.Mirror && World.AdvancedLogic.CanLiftHeavy(items) && items.Flute ||
                World.AdvancedLogic.CanAccessMiseryMirePortal(items) && items.Mirror;
        }

        public bool CanComplete(Progression items)
        {
            return LanmolasReward.IsAvailable(items);
        }

        private bool CanBeatBoss(Progression items)
        {
            return items.Sword || items.Hammer || items.Bow ||
                items.FireRod || items.IceRod ||
                items.Byrna || items.Somaria;
        }
    }
}
