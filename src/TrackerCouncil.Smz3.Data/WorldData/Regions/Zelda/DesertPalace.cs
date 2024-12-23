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

public class DesertPalace : Z3Region, IHasReward, IHasTreasure, IHasBoss
{
    public DesertPalace(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = [ItemType.KeyDP, ItemType.BigKeyDP, ItemType.MapDP, ItemType.CompassDP];

        BigChest = new Location(this, LocationId.DesertPalaceBigChest, 0x1E98F, LocationType.Regular,
            name: "Big Chest",
            vanillaItem: ItemType.ProgressiveGlove,
            access: items => items.BigKeyDP,
            memoryAddress: 0x73,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        TorchItem = new Location(this, LocationId.DesertPalaceTorch, 0x308160, LocationType.Regular,
            name: "Torch",
            vanillaItem: ItemType.KeyDP,
            access: items => items.Boots,
            memoryAddress: 0x73,
            memoryFlag: 0xA,
            metadata: metadata,
            trackerState: trackerState);

        MapChest = new Location(this, LocationId.DesertPalaceMapChest, 0x1E9B6, LocationType.Regular,
            name: "Map Chest",
            vanillaItem: ItemType.MapDP,
            memoryAddress: 0x74,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.DesertPalaceBigKeyChest, 0x1E9C2, LocationType.Regular,
            name: "Big Key Chest",
            vanillaItem: ItemType.BigKeyDP,
            access: items => items.KeyDP,
            memoryAddress: 0x75,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        CompassChest = new Location(this, LocationId.DesertPalaceCompassChest, 0x1E9CB, LocationType.Regular,
            name: "Compass Chest",
            vanillaItem: ItemType.CompassDP,
            access: items => items.KeyDP,
            memoryAddress: 0x85,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        LanmolasReward = new Location(this, LocationId.DesertPalaceLanmolas, 0x308151, LocationType.Regular,
            name: "Lanmolas",
            vanillaItem: ItemType.HeartContainer,
            access: items => (
                Logic.CanLiftLight(items) ||
                (Logic.CanAccessMiseryMirePortal(items) && items.Mirror)
            ) && items.BigKeyDP && items.KeyDP && Logic.CanLightTorches(items) && CanDefeatLanmolas(items),
            memoryAddress: 0x33,
            memoryFlag: 0xB,
            metadata: metadata,
            trackerState: trackerState);

        MemoryAddress = 0x33;
        MemoryFlag = 0xB;
        StartingRooms = new List<int>() { 99, 131, 132, 133 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Desert Palace");
        MapName = "Light World";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Desert Palace";

    public RewardType DefaultRewardType => RewardType.PendantBlue;

    public BossType DefaultBossType => BossType.Lanmolas;

    public bool IsShuffledReward => true;

    public bool UnifiedBossAndItemLocation => true;

    public override List<string> AlsoKnownAs { get; } = ["Dessert Palace"];

    public LocationId? BossLocationId => LocationId.DesertPalaceLanmolas;

    public Reward Reward { get; set; } = null!;

    public TrackerRewardState RewardState { get; set; } = null!;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public event EventHandler? UpdatedTreasure;

    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public Boss Boss { get; set; } = null!;

    public Location BigChest { get; }

    public Location TorchItem { get; }

    public Location MapChest { get; }

    public Location BigKeyChest { get; }

    public Location CompassChest { get; }

    public Location LanmolasReward { get; }

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return items.Book ||
               items.Mirror && Logic.CanLiftHeavy(items) && items.Flute ||
               Logic.CanAccessMiseryMirePortal(items) && items.Mirror;
    }

    public bool CanBeatBoss(Progression items) => LanmolasReward.IsAvailable(items);

    public bool CanRetrieveReward(Progression items) => LanmolasReward.IsAvailable(items);

    public bool CanSeeReward(Progression items) => !World.Config.ZeldaKeysanity || items.Contains(ItemType.MapDP);

    private bool CanDefeatLanmolas(Progression items)
    {
        return items.Sword || items.Hammer || items.Bow ||
               items.FireRod || items.IceRod ||
               items.Byrna || items.Somaria;
    }
}
