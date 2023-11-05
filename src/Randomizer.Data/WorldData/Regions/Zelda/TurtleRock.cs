using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.Zelda
{
    public class TurtleRock : Z3Region, IHasReward, INeedsMedallion, IDungeon
    {
        public static readonly int[] MusicAddresses = new[] {
            0x02D5C7,
            0x02D5A7,
            0x02D5AA,
            0x02D5AB
        };
        public TurtleRock(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            RegionItems = new[] { ItemType.KeyTR, ItemType.BigKeyTR, ItemType.MapTR, ItemType.CompassTR };

            CompassChest = new Location(this, LocationId.TurtleRockCompassChest, 0x1EA22, LocationType.Regular,
                name: "Compass Chest",
                vanillaItem: ItemType.CompassTR,
                memoryAddress: 0xD6,
                memoryFlag: 0x4,
                trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
                metadata: metadata,
                trackerState: trackerState);

            ChainChomps = new Location(this, LocationId.TurtleRockChainChomps, 0x1EA16, LocationType.Regular,
                name: "Chain Chomps",
                vanillaItem: ItemType.KeyTR,
                access: items => items.KeyTR >= 1,
                memoryAddress: 0xB6,
                memoryFlag: 0x4,
                trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
                metadata: metadata,
                trackerState: trackerState);

            BigKeyChest = new Location(this, LocationId.TurtleRockBigKeyChest, 0x1EA25, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyTR,
                access: items => BigKeyChest != null && items.KeyTR >=
                    (!Config.GameModeConfigs.KeysanityConfig.ZeldaKeysanity || BigKeyChest.ItemIs(ItemType.BigKeyTR, World) ? 2 :
                        BigKeyChest.ItemIs(ItemType.KeyTR, World) ? 3 : 4),
                memoryAddress: 0x14,
                memoryFlag: 0x4,
                trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
                metadata: metadata,
                trackerState: trackerState)
                .AlwaysAllow((item, items) => item.Is(ItemType.KeyTR, World) && items.KeyTR >= 3);

            BigChest = new Location(this, LocationId.TurtleRockBigChest, 0x1EA19, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveShield,
                access: items => items.BigKeyTR && items.KeyTR >= 2,
                memoryAddress: 0x24,
                memoryFlag: 0x4,
                trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
                metadata: metadata,
                trackerState: trackerState)
                .Allow((item, items) => item.IsNot(ItemType.BigKeyTR, World));

            CrystarollerRoom = new Location(this, LocationId.TurtleRockCrystarollerRoom, 0x1EA34, LocationType.Regular,
                name: "Crystaroller Room",
                vanillaItem: ItemType.KeyTR,
                access: items => items.BigKeyTR && items.KeyTR >= 2,
                memoryAddress: 0x4,
                memoryFlag: 0x4,
                trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
                metadata: metadata,
                trackerState: trackerState);

            TrinexxReward = new Location(this, LocationId.TurtleRockTrinexx, 0x308159, LocationType.Regular,
                name: "Trinexx",
                vanillaItem: ItemType.HeartContainer,
                access: items => items.BigKeyTR && items.KeyTR >= 4 && Logic.CanPassSwordOnlyDarkRooms(items) && CanBeatBoss(items),
                memoryAddress: 0xA4,
                memoryFlag: 0xB,
                trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
                metadata: metadata,
                trackerState: trackerState);

            RollerRoom = new RollerRoomRoom(this, metadata, trackerState);
            LaserBridge = new LaserBridgeRoom(this, metadata, trackerState);

            MemoryAddress = 0xA4;
            MemoryFlag = 0xB;
            StartingRooms = new List<int> { 35, 36, 213, 214 };
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Turtle Rock");
            DungeonMetadata = metadata?.Dungeon(GetType()) ?? new DungeonInfo("Turtle Rock", "TR", "Trinexx");
            DungeonState = trackerState?.DungeonStates.First(x => x.WorldId == world.Id && x.Name == GetType().Name) ?? new TrackerDungeonState();
            Reward = new Reward(DungeonState.Reward ?? RewardType.None, world, this, metadata, DungeonState);
            Medallion = DungeonState.RequiredMedallion ?? ItemType.Nothing;
        }

        public override string Name => "Turtle Rock";

        public RewardType DefaultRewardType => RewardType.CrystalBlue;

        public ItemType DefaultMedallion => ItemType.Quake;

        public int SongIndex { get; init; } = 10;

        public Reward Reward { get; set; }

        public DungeonInfo DungeonMetadata { get; set; }

        public TrackerDungeonState DungeonState { get; set; }

        public Region ParentRegion => World.DarkWorldDeathMountainEast;

        public ItemType Medallion { get; set; }

        public Location CompassChest { get; }

        public Location ChainChomps { get; }

        public Location BigKeyChest { get; }

        public Location BigChest { get; }

        public Location CrystarollerRoom { get; }

        public Location TrinexxReward { get; }

        public RollerRoomRoom RollerRoom { get; }

        public LaserBridgeRoom LaserBridge { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.Contains(Medallion) && items.Sword && items.MoonPearl &&
                Logic.CanLiftHeavy(items) && items.Hammer && items.Somaria &&
                World.LightWorldDeathMountainEast.CanEnter(items, requireRewards);
        }

        public bool CanComplete(Progression items)
        {
            return TrinexxReward.IsAvailable(items);
        }

        private bool CanBeatBoss(Progression items)
        {
            return items.FireRod && items.IceRod;
        }

        public class RollerRoomRoom : Room
        {
            public RollerRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Roller Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.TurtleRockRollerRoomLeft, 0x1EA1C, LocationType.Regular,
                        name: "Left",
                        vanillaItem: ItemType.MapTR,
                        access: items => items.FireRod,
                        memoryAddress: 0xB7,
                        memoryFlag: 0x4,
                        trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.DungeonState.MarkedMedallion),
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.TurtleRockRollerRoomRight, 0x1EA1F, LocationType.Regular,
                        name: "Right",
                        vanillaItem: ItemType.KeyTR,
                        access: items => items.FireRod,
                        memoryAddress: 0xB7,
                        memoryFlag: 0x5,
                        trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.DungeonState.MarkedMedallion),
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class LaserBridgeRoom : Room
        {
            public LaserBridgeRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Eye Bridge", metadata, "Laser Bridge")
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.TurtleRockEyeBridgeTopRight, 0x1EA28, LocationType.Regular,
                        name: "Top Right",
                        vanillaItem: ItemType.FiveRupees,
                        access: CanAccess,
                        memoryAddress: 0xD5,
                        memoryFlag: 0x4,
                        trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.DungeonState.MarkedMedallion),
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.TurtleRockEyeBridgeTopLeft, 0x1EA2B, LocationType.Regular,
                        name: "Top Left",
                        vanillaItem: ItemType.FiveRupees,
                        access: CanAccess,
                        memoryAddress: 0xD5,
                        memoryFlag: 0x5,
                        trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.DungeonState.MarkedMedallion),
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.TurtleRockEyeBridgeBottomRight, 0x1EA2E, LocationType.Regular,
                        name: "Bottom Right",
                        vanillaItem: ItemType.TwentyRupees,
                        access: CanAccess,
                        memoryAddress: 0xD5,
                        memoryFlag: 0x6,
                        trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.DungeonState.MarkedMedallion),
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.TurtleRockEyeBridgeBottomLeft, 0x1EA31, LocationType.Regular,
                        name: "Bottom Left",
                        vanillaItem: ItemType.KeyTR,
                        access: CanAccess,
                        memoryAddress: 0xD5,
                        memoryFlag: 0x7,
                        trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.DungeonState.MarkedMedallion),
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }

            private bool CanAccess(Progression items)
            {
                return items.BigKeyTR && items.KeyTR >= 3 && Logic.CanPassSwordOnlyDarkRooms(items) && (items.Cape || items.Byrna || items.CanBlockLasers);
            }
        }
    }
}
