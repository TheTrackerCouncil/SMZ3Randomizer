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

public class EasternPalace : Z3Region, IHasReward, IHasTreasure, IHasBoss
{
    public EasternPalace(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = [ItemType.BigKeyEP, ItemType.MapEP, ItemType.CompassEP];

        CannonballChest = new Location(this, LocationId.EasternPalaceCannonballChest, 0x1E9B3, LocationType.Regular,
            name: "Cannonball Chest",
            vanillaItem: ItemType.OneHundredRupees,
            memoryAddress: 0xB9,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        MapChest = new Location(this, LocationId.EasternPalaceMapChest, 0x1E9F5, LocationType.Regular,
            name: "Map Chest",
            vanillaItem: ItemType.MapEP,
            memoryAddress: 0xAA,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        CompassChest = new Location(this, LocationId.EasternPalaceCompassChest, 0x1E977, LocationType.Regular,
            name: "Compass Chest",
            vanillaItem: ItemType.CompassEP,
            memoryAddress: 0xA8,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigChest = new Location(this, LocationId.EasternPalaceBigChest, 0x1E97D, LocationType.Regular,
            name: "Big Chest",
            vanillaItem: ItemType.Bow,
            access: items => items.BigKeyEP,
            memoryAddress: 0xA9,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.EasternPalaceBigKeyChest, 0x1E9B9, LocationType.Regular,
            name: "Big Key Chest",
            vanillaItem: ItemType.BigKeyEP,
            access: items => Logic.CanPassSwordOnlyDarkRooms(items),
            memoryAddress: 0xB8,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        ArmosKnightsRewards = new Location(this, LocationId.EasternPalaceArmosKnights, 0x308150, LocationType.Regular,
            name: "Armos Knights",
            vanillaItem: ItemType.HeartContainer,
            access: items => items.BigKeyEP && items.Bow && Logic.CanPassFireRodDarkRooms(items),
            memoryAddress: 0xC8,
            memoryFlag: 0xB,
            metadata: metadata,
            trackerState: trackerState);

        MemoryAddress = 0xC8;
        MemoryFlag = 0xB;
        StartingRooms = new List<int> { 201 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Eastern Palace");
        MapName = "Light World";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Eastern Palace";

    public RewardType DefaultRewardType => RewardType.PendantGreen;

    public BossType DefaultBossType => BossType.ArmosKnights;

    public bool IsShuffledReward => true;

    public Reward Reward { get; set; } = null!;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public event EventHandler? UpdatedTreasure;

    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public TrackerRewardState RewardState { get; set; } = null!;

    public Boss Boss { get; set; } = null!;

    public LocationId? BossLocationId => LocationId.EasternPalaceArmosKnights;

    public Location CannonballChest { get; }

    public Location MapChest { get; }

    public Location CompassChest { get; }

    public Location BigChest { get; }

    public Location BigKeyChest { get; }

    public Location ArmosKnightsRewards { get; }

    public bool CanRetrieveReward(Progression items)
        => ArmosKnightsRewards.IsAvailable(items);

    public bool CanSeeReward(Progression items) => !World.Config.ZeldaKeysanity || items.Contains(ItemType.MapEP);

    public bool CanBeatBoss(Progression items)
        => ArmosKnightsRewards.IsAvailable(items);
}
