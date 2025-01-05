using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;

public class TurtleRock : Z3Region, IHasReward, IHasPrerequisite, IHasTreasure, IHasBoss
{
    public TurtleRock(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = new Dictionary<ItemType, ItemType>
        {
            { ItemType.Key, ItemType.KeyTR },
            { ItemType.BigKey, ItemType.BigKeyTR },
            { ItemType.Map, ItemType.MapTR },
            { ItemType.Compass, ItemType.CompassTR },
        };

        CompassChest = new Location(this, LocationId.TurtleRockCompassChest, 0x1EA22, LocationType.Regular,
            name: "Compass Chest",
            vanillaItem: ItemType.CompassTR,
            memoryAddress: 0xD6,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState?.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        ChainChomps = new Location(this, LocationId.TurtleRockChainChomps, 0x1EA16, LocationType.Regular,
            name: "Chain Chomps",
            vanillaItem: ItemType.KeyTR,
            access: items => items.KeyTR >= 1,
            memoryAddress: 0xB6,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState?.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.TurtleRockBigKeyChest, 0x1EA25, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyTR,
                access: items => BigKeyChest != null && items.KeyTR >=
                    (!Config.ZeldaKeysanity || BigKeyChest.ItemIs(ItemType.BigKeyTR, World) ? 2 :
                        BigKeyChest.ItemIs(ItemType.KeyTR, World) ? 3 : 4),
                memoryAddress: 0x14,
                memoryFlag: 0x4,
                trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState?.MarkedItem),
                metadata: metadata,
                trackerState: trackerState)
            .AlwaysAllow((item, items) => item.Is(ItemType.KeyTR, World) && items.KeyTR >= 3);

        BigChest = new Location(this, LocationId.TurtleRockBigChest, 0x1EA19, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveShield,
                access: items => items.BigKeyTR && items.KeyTR >= 2,
                memoryAddress: 0x24,
                memoryFlag: 0x4,
                trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState?.MarkedItem),
                metadata: metadata,
                trackerState: trackerState)
            .Allow((item, items) => item.IsNot(ItemType.BigKeyTR, World));

        CrystarollerRoom = new Location(this, LocationId.TurtleRockCrystarollerRoom, 0x1EA34, LocationType.Regular,
            name: "Crystaroller Room",
            vanillaItem: ItemType.KeyTR,
            access: items => items.BigKeyTR && items.KeyTR >= 2,
            memoryAddress: 0x4,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState?.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        TrinexxReward = new Location(this, LocationId.TurtleRockTrinexx, 0x308159, LocationType.Regular,
            name: "Trinexx",
            vanillaItem: ItemType.HeartContainer,
            access: items => items.BigKeyTR && items.KeyTR >= 4 && Logic.CanPassSwordOnlyDarkRooms(items) && CanBeatTrinexx(items),
            memoryAddress: 0xA4,
            memoryFlag: 0xB,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState?.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        RollerRoom = new RollerRoomRoom(this, metadata, trackerState);
        LaserBridge = new LaserBridgeRoom(this, metadata, trackerState);

        MemoryAddress = 0xA4;
        MemoryFlag = 0xB;
        StartingRooms = new List<int> { 35, 36, 213, 214 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Turtle Rock");
        MapName = "Dark World";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasPrerequisite)this).ApplyState(trackerState);
        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Turtle Rock";

    public RewardType DefaultRewardType => RewardType.CrystalBlue;

    public bool IsShuffledReward => true;

    public ItemType DefaultRequiredItem => ItemType.Quake;

    public BossType DefaultBossType => BossType.Trinexx;

    public LocationId? BossLocationId => LocationId.TurtleRockTrinexx;

    public bool UnifiedBossAndItemLocation => true;

    public Reward Reward { get; set; } = null!;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public event EventHandler? UpdatedTreasure;

    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public TrackerRewardState RewardState { get; set; } = null!;

    public TrackerPrerequisiteState PrerequisiteState { get; set; } = null!;

    public event EventHandler? UpdatedPrerequisite;

    public void OnUpdatedPrerequisite() => UpdatedPrerequisite?.Invoke(this, EventArgs.Empty);

    public Boss Boss { get; set; } = null!;

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
        return items.Contains(PrerequisiteState.RequiredItem) && items.Sword && World.Logic.CanNavigateDarkWorld(items) &&
               Logic.CanLiftHeavy(items) && items.Hammer && items.Somaria &&
               World.LightWorldDeathMountainEast.CanEnter(items, requireRewards);
    }

    public bool CanBeatBoss(Progression items) => TrinexxReward.IsAvailable(items);

    public bool CanRetrieveReward(Progression items) => TrinexxReward.IsAvailable(items);

    public bool CanSeeReward(Progression items) => !World.Config.ZeldaKeysanity || items.Contains(ItemType.MapTR);

    private bool CanBeatTrinexx(Progression items)
    {
        return items is { FireRod: true, IceRod: true };
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
                    trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.PrerequisiteState.MarkedItem),
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.TurtleRockRollerRoomRight, 0x1EA1F, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.KeyTR,
                    access: items => items.FireRod,
                    memoryAddress: 0xB7,
                    memoryFlag: 0x5,
                    trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.PrerequisiteState.MarkedItem),
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
                    trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.PrerequisiteState.MarkedItem),
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.TurtleRockEyeBridgeTopLeft, 0x1EA2B, LocationType.Regular,
                    name: "Top Left",
                    vanillaItem: ItemType.FiveRupees,
                    access: CanAccess,
                    memoryAddress: 0xD5,
                    memoryFlag: 0x5,
                    trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.PrerequisiteState.MarkedItem),
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.TurtleRockEyeBridgeBottomRight, 0x1EA2E, LocationType.Regular,
                    name: "Bottom Right",
                    vanillaItem: ItemType.TwentyRupees,
                    access: CanAccess,
                    memoryAddress: 0xD5,
                    memoryFlag: 0x6,
                    trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.PrerequisiteState.MarkedItem),
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.TurtleRockEyeBridgeBottomLeft, 0x1EA31, LocationType.Regular,
                    name: "Bottom Left",
                    vanillaItem: ItemType.KeyTR,
                    access: CanAccess,
                    memoryAddress: 0xD5,
                    memoryFlag: 0x7,
                    trackerLogic: items => items.HasMarkedMedallion(World.TurtleRock.PrerequisiteState.MarkedItem),
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
