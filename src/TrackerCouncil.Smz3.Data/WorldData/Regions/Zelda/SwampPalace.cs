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

public class SwampPalace : Z3Region, IHasReward, IHasTreasure, IHasBoss
{
    public SwampPalace(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = [ItemType.KeySP, ItemType.BigKeySP, ItemType.MapSP, ItemType.CompassSP];

        Entrance = new Location(this, LocationId.SwampPalaceEntrance, 0x1EA9D, LocationType.Regular,
                name: "Entrance",
                vanillaItem: ItemType.KeySP,
                memoryAddress: 0x28,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .Allow((item, items) => Config.ZeldaKeysanity || item.Is(ItemType.KeySP, World));

        MapChest = new Location(this, LocationId.SwampPalaceMapChest, 0x1E986, LocationType.Regular,
            name: "Map Chest",
            vanillaItem: ItemType.MapSP,
            access: items => items.KeySP,
            memoryAddress: 0x37,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigChest = new Location(this, LocationId.SwampPalaceBigChest, 0x1E989, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.Hookshot,
                access: items => items.BigKeySP && items.KeySP && items.Hammer,
                memoryAddress: 0x36,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .AlwaysAllow((item, items) => item.Is(ItemType.BigKeySP, World));

        CompassChest = new Location(this, LocationId.SwampPalaceCompassChest, 0x1EAA0, LocationType.Regular,
            name: "Compass Chest",
            vanillaItem: ItemType.CompassSP,
            access: items => items.KeySP && items.Hammer,
            memoryAddress: 0x46,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        WestChest = new Location(this, LocationId.SwampPalaceWestChest, 0x1EAA3, LocationType.Regular,
            name: "West Chest",
            vanillaItem: ItemType.TwentyRupees,
            access: items => items.KeySP && items.Hammer,
            memoryAddress: 0x34,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.SwampPalaceBigKeyChest, 0x1EAA6, LocationType.Regular,
            name: "Big Key Chest",
            vanillaItem: ItemType.BigKeySP,
            access: items => items.KeySP && items.Hammer,
            memoryAddress: 0x35,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        WaterfallRoom = new Location(this, LocationId.SwampPalaceWaterfallRoom, 0x1EAAF, LocationType.Regular,
            name: "Waterfall Room",
            vanillaItem: ItemType.TwentyRupees,
            access: items => items.KeySP && items.Hammer && items.Hookshot,
            memoryAddress: 0x66,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        ArrghusReward = new Location(this, LocationId.SwampPalaceArrghus, 0x308154, LocationType.Regular,
            name: "Arrghus",
            vanillaItem: ItemType.HeartContainer,
            access: items => items.KeySP && items.Hammer && items.Hookshot,
            memoryAddress: 0x6,
            memoryFlag: 0xB,
            metadata: metadata,
            trackerState: trackerState);

        FloodedRoom = new FloodedRoomRoom(this, metadata, trackerState);

        MemoryAddress = 0x6;
        MemoryFlag = 0xB;
        StartingRooms = new List<int> { 40 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Swamp Palace");
        MapName = "Dark World";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Swamp Palace";

    public RewardType DefaultRewardType => RewardType.CrystalBlue;

    public BossType DefaultBossType => BossType.Arrghus;

    public bool IsShuffledReward => true;

    public LocationId? BossLocationId => LocationId.SwampPalaceArrghus;

    public Reward Reward { get; set; } = null!;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public event EventHandler? UpdatedTreasure;

    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public TrackerRewardState RewardState { get; set; } = null!;

    public Boss Boss { get; set; } = null!;

    public Location Entrance { get; }

    public Location MapChest { get; }

    public Location BigChest { get; }

    public Location CompassChest { get; }

    public Location WestChest { get; }

    public Location BigKeyChest { get; }

    public Location WaterfallRoom { get; }

    public Location ArrghusReward { get; }

    public FloodedRoomRoom FloodedRoom { get; }

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return items.MoonPearl && items.Mirror && items.Flippers && World.DarkWorldSouth.CanEnter(items, requireRewards);
    }

    public bool CanBeatBoss(Progression items) => ArrghusReward.IsAvailable(items);

    public bool CanRetrieveReward(Progression items) => ArrghusReward.IsAvailable(items);

    public bool CanSeeReward(Progression items) => !World.Config.ZeldaKeysanity || items.Contains(ItemType.MapSP);

    public class FloodedRoomRoom : Room
    {
        public FloodedRoomRoom(Region region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Flooded Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.SwampPalaceFloodedRoomLeft, 0x1EAA9, LocationType.Regular,
                    name: "Left",
                    vanillaItem: ItemType.TwentyRupees,
                    access: items => items.KeySP && items.Hammer && items.Hookshot,
                    memoryAddress: 0x76,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),
                new Location(this, LocationId.SwampPalaceFloodedRoomRight, 0x1EAAC, LocationType.Regular,
                    name: "Right",
                    vanillaItem: ItemType.TwentyRupees,
                    access: items => items.KeySP && items.Hammer && items.Hookshot,
                    memoryAddress: 0x76,
                    memoryFlag: 0x5,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }
}
