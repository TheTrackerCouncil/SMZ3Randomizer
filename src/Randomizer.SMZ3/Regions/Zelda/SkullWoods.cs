﻿using System.Collections.Generic;
using Randomizer.Shared;

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
                vanillaItem: ItemType.KeySW,
                memoryAddress: 0x57,
                memoryFlag: 0x5);

            CompassChest = new Location(this, 256 + 146, 0x1E992, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassSW,
                memoryAddress: 0x67,
                memoryFlag: 0x4);

            BigChest = new Location(this, 256 + 147, 0x1E998, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Firerod,
                access: items => items.BigKeySW,
                memoryAddress: 0x58,
                memoryFlag: 0x4)
                .AlwaysAllow((item, items) => item.Is(ItemType.BigKeySW, World));

            MapChest = new Location(this, 256 + 148, 0x1E99B, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapSW,
                memoryAddress: 0x58,
                memoryFlag: 0x5);

            PinballRoom = new Location(this, 256 + 149, 0x1E9C8, LocationType.Regular,
                name: "Pinball Room",
                vanillaItem: ItemType.KeySW,
                memoryAddress: 0x68,
                memoryFlag: 0x4)
                .Allow((item, items) => item.Is(ItemType.KeySW, World));

            BigKeyChest = new Location(this, 256 + 150, 0x1E99E, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeySW,
                memoryAddress: 0x57,
                memoryFlag: 0x4);

            BridgeRoom = new Location(this, 256 + 151, 0x1E9FE, LocationType.Regular,
                name: "Bridge Room",
                vanillaItem: ItemType.KeySW,
                access: items => items.FireRod,
                memoryAddress: 0x59,
                memoryFlag: 0x4);

            MothulaReward = new Location(this, 256 + 152, 0x308155, LocationType.Regular,
                name: "Mothula",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.FireRod && items.Sword && items.KeySW >= 3,
                memoryAddress: 0x29,
                memoryFlag: 0xB);

            MemoryAddress = 0x29;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 86, 87, 88, 89, 103, 104 };
        }

        public override string Name => "Skull Woods";

        public override List<string> AlsoKnownAs { get; }
            = new List<string>() { "Skill Woods" };

        public ItemType Reward { get; set; } = ItemType.Nothing;

        public Item RewardItem { get; set; }


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
