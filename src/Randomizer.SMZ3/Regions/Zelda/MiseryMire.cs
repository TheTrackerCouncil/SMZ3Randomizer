using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.Zelda
{
    public class MiseryMire : Z3Region, IHasReward, INeedsMedallion
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5B9
        };
        public MiseryMire(World world, Config config) : base(world, config)
        {
            RegionItems = new[] { ItemType.KeyMM, ItemType.BigKeyMM, ItemType.MapMM, ItemType.CompassMM };

            MainLobby = new Location(this, 256 + 169, 0x1EA5E, LocationType.Regular,
                name: "Main Lobby",
                vanillaItem: ItemType.KeyMM,
                access: items => items.BigKeyMM || items.KeyMM >= 1,
                memoryAddress: 0xC2,
                memoryFlag: 0x4);

            MapChest = new Location(this, 256 + 170, 0x1EA6A, LocationType.Regular,
                name: "Map Chest",
                vanillaItem: ItemType.MapMM,
                access: items => items.BigKeyMM || items.KeyMM >= 1,
                memoryAddress: 0xC3,
                memoryFlag: 0x5);

            BridgeChest = new Location(this, 256 + 171, 0x1EA61, LocationType.Regular,
                name: "Bridge Chest",
                vanillaItem: ItemType.KeyMM,
                memoryAddress: 0xA2,
                memoryFlag: 0x4);

            SpikeChest = new Location(this, 256 + 172, 0x1E9DA, LocationType.Regular,
                name: "Spike Chest",
                vanillaItem: ItemType.KeyMM,
                memoryAddress: 0xB3,
                memoryFlag: 0x4);

            CompassChest = new Location(this, 256 + 173, 0x1EA64, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassMM,
                access: items => Logic.CanLightTorches(items)
                         && items.KeyMM >= (BigKeyChest.ItemIs(ItemType.BigKeyMM, World) ? 2 : 3),
                memoryAddress: 0xC1,
                memoryFlag: 0x4);

            BigKeyChest = new Location(this, 256 + 174, 0x1EA6D, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyMM,
                access: items => Logic.CanLightTorches(items)
                         && items.KeyMM >= (CompassChest.ItemIs(ItemType.BigKeyMM, World) ? 2 : 3),
                memoryAddress: 0xD1,
                memoryFlag: 0x4);

            BigChest = new Location(this, 256 + 175, 0x1EA67, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Somaria,
                access: items => items.BigKeyMM,
                memoryAddress: 0xC3,
                memoryFlag: 0x4);

            VitreousReward = new Location(this, 256 + 176, 0x308158, LocationType.Regular,
                name: "Vitreous",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyMM && Logic.CanPassSwordOnlyDarkRooms(items) && items.Somaria,
                memoryAddress: 0x90,
                memoryFlag: 0xB);

            MemoryAddress = 0x90;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 152 };
        }

        public override string Name => "Misery Mire";

        public Reward Reward { get; set; } = Reward.None;

        public ItemType Medallion { get; set; }

        public Location MainLobby { get; }

        public Location MapChest { get; }

        public Location BridgeChest { get; }

        public Location SpikeChest { get; }

        public Location CompassChest { get; }

        public Location BigKeyChest { get; }

        public Location BigChest { get; }

        public Location VitreousReward { get; }

        // Need "CanKillManyEnemies" if implementing swordless
        public override bool CanEnter(Progression items)
        {
            return items.Contains(Medallion) && items.Sword && items.MoonPearl &&
                (items.Boots || items.Hookshot) && World.DarkWorldMire.CanEnter(items);
        }

        public bool CanComplete(Progression items)
        {
            return VitreousReward.IsAvailable(items);
        }
    }
}
