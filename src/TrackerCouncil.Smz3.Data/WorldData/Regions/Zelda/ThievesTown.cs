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

public class ThievesTown : Z3Region, IHasReward, IHasTreasure, IHasBoss
{
    public ThievesTown(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = [ItemType.KeyTT, ItemType.BigKeyTT, ItemType.MapTT, ItemType.CompassTT];

        MapChest = new Location(this, LocationId.ThievesTownMapChest, 0x1EA01, LocationType.Regular,
            name: "Map Chest",
            vanillaItem: ItemType.MapTT,
            memoryAddress: 0xDB,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        AmbushChest = new Location(this, LocationId.ThievesTownAmbushChest, 0x1EA0A, LocationType.Regular,
            name: "Ambush Chest",
            vanillaItem: ItemType.TwentyRupees,
            memoryAddress: 0xCB,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        CompassChest = new Location(this, LocationId.ThievesTownCompassChest, 0x1EA07, LocationType.Regular,
            name: "Compass Chest",
            vanillaItem: ItemType.CompassTT,
            memoryAddress: 0xDC,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.ThievesTownBigKeyChest, 0x1EA04, LocationType.Regular,
            name: "Big Key Chest",
            vanillaItem: ItemType.BigKeyTT,
            memoryAddress: 0xDB,
            memoryFlag: 0x5,
            metadata: metadata,
            trackerState: trackerState);

        Attic = new Location(this, LocationId.ThievesTownAttic, 0x1EA0D, LocationType.Regular,
            name: "Attic", // ??? Vanilla item ???
            access: items => items.BigKeyTT && items.KeyTT,
            memoryAddress: 0x65,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BlindsCell = new Location(this, LocationId.ThievesTownBlindsCell, 0x1EA13, LocationType.Regular,
            name: "Blind's Cell",
            vanillaItem: ItemType.KeyTT,
            access: items => items.BigKeyTT,
            memoryAddress: 0x45,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigChest = new Location(this, LocationId.ThievesTownBigChest, 0x1EA10, LocationType.Regular,
                name: "Big Chest",
                vanillaItem: ItemType.ProgressiveGlove,
                access: items => BigChest != null && items.BigKeyTT && items.Hammer &&
                                 (BigChest.ItemIs(ItemType.KeyTT, World) || items.KeyTT),
                memoryAddress: 0x44,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .AlwaysAllow((item, items) => item.Is(ItemType.KeyTT, World) && items.Hammer);

        BlindReward = new Location(this, LocationId.ThievesTownBlind, 0x308156, LocationType.Regular,
            name: "Blind",
            vanillaItem: ItemType.HeartContainer,
            access: items => items.BigKeyTT && items.KeyTT && CanBeatBlind(items),
            memoryAddress: 0xAC,
            memoryFlag: 0xB,
            metadata: metadata,
            trackerState: trackerState);

        MemoryAddress = 0xAC;
        MemoryFlag = 0xB;
        StartingRooms = new List<int> { 219 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Thieves' Town");
        MapName = "Dark World";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Thieves' Town";

    public RewardType DefaultRewardType => RewardType.CrystalBlue;

    public BossType DefaultBossType => BossType.Blind;

    public bool IsShuffledReward => true;

    public LocationId? BossLocationId => LocationId.ThievesTownBlind;

    public Reward Reward { get; set; } = null!;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public event EventHandler? UpdatedTreasure;

    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public TrackerRewardState RewardState { get; set; } = null!;

    public Boss Boss { get; set; } = null!;

    public Location MapChest { get; }

    public Location AmbushChest { get; }

    public Location CompassChest { get; }

    public Location BigKeyChest { get; }

    public Location Attic { get; }

    public Location BlindsCell { get; }

    public Location BigChest { get; }

    public Location BlindReward { get; }

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return items.MoonPearl && World.DarkWorldNorthWest.CanEnter(items, requireRewards);
    }

    public bool CanBeatBoss(Progression items) =>BlindReward.IsAvailable(items);

    public bool CanRetrieveReward(Progression items) =>BlindReward.IsAvailable(items);

    public bool CanSeeReward(Progression items) => !World.Config.ZeldaKeysanity || items.Contains(ItemType.MapTT);

    private bool CanBeatBlind(Progression items)
    {
        return items.Sword || items.Hammer ||
               items.Somaria || items.Byrna;
    }
}
