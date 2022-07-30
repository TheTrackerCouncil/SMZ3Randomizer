﻿using System.Collections.Generic;
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
                vanillaItem: ItemType.CompassIP,
                memoryAddress: 0x2E,
                memoryFlag: 0x4);

            SpikeRoom = new Location(this, 256 + 162, 0x1E9E0, LocationType.Regular,
                name: "Spike Room",
                vanillaItem: ItemType.KeyIP,
                access: items => items.Hookshot || (items.KeyIP >= 1
                                            && CanNotWasteKeysBeforeAccessible(items, MapChest, BigKeyChest)),
                memoryAddress: 0x5F,
                memoryFlag: 0x4);

            MapChest = new Location(this, 256 + 163, 0x1E9DD, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapIP,
                access: items => items.Hammer && Logic.CanLiftLight(items) && (
                    items.Hookshot || (items.KeyIP >= 1
                                       && CanNotWasteKeysBeforeAccessible(items, SpikeRoom, BigKeyChest))
                ),
                memoryAddress: 0x3F,
                memoryFlag: 0x4);

            BigKeyChest = new Location(this, 256 + 164, 0x1E9A4, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyIP,
                access: items => items.Hammer && Logic.CanLiftLight(items) && (
                    items.Hookshot || (items.KeyIP >= 1
                                       && CanNotWasteKeysBeforeAccessible(items, SpikeRoom, MapChest))
                ),
                memoryAddress: 0x1F,
                memoryFlag: 0x4);

            IcedTRoom = new Location(this, 256 + 165, 0x1E9E3, LocationType.Regular,
                name: "Iced T Room",
                vanillaItem: ItemType.KeyIP,
                memoryAddress: 0xAE,
                memoryFlag: 0x4);

            FreezorChest = new Location(this, 256 + 166, 0x1E995, LocationType.Regular,
                name: "Freezor Chest",
                vanillaItem: ItemType.ThreeBombs,
                memoryAddress: 0x7E,
                memoryFlag: 0x4);

            BigChest = new Location(this, 256 + 167, 0x1E9AA, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveTunic,
                access: items => items.BigKeyIP,
                memoryAddress: 0x9E,
                memoryFlag: 0x4);

            KholdstareReward = new Location(this, 256 + 168, 0x308157, LocationType.Regular,
                name: "Kholdstare",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyIP && items.Hammer && Logic.CanLiftLight(items) &&
                    items.KeyIP >= (items.Somaria ? 1 : 2),
                memoryAddress: 0xDE,
                memoryFlag: 0xB);

            MemoryAddress = 0xDE;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 14 };
        }

        public override string Name => "Ice Palace";

        public ItemType RewardType { get; set; } = ItemType.Nothing;

        public Item RewardItem { get; set; }

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
            return items.MoonPearl && items.Flippers && Logic.CanLiftHeavy(items) && Logic.CanMeltFreezors(items);
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
