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

public class MiseryMire : Z3Region, IHasReward, IHasPrerequisite, IHasTreasure, IHasBoss
{
    public MiseryMire(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = [ItemType.KeyMM, ItemType.BigKeyMM, ItemType.MapMM, ItemType.CompassMM];

        MainLobby = new Location(this, LocationId.MiseryMireMainLobby, 0x1EA5E, LocationType.Regular,
            name: "Main Lobby",
            vanillaItem: ItemType.KeyMM,
            access: items => items.BigKeyMM || items.KeyMM >= 1,
            memoryAddress: 0xC2,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        MapChest = new Location(this, LocationId.MiseryMireMapChest, 0x1EA6A, LocationType.Regular,
            name: "Map Chest",
            vanillaItem: ItemType.MapMM,
            access: items => items.BigKeyMM || items.KeyMM >= 1,
            memoryAddress: 0xC3,
            memoryFlag: 0x5,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        BridgeChest = new Location(this, LocationId.MiseryMireBridgeChest, 0x1EA61, LocationType.Regular,
            name: "Bridge Chest",
            vanillaItem: ItemType.KeyMM,
            memoryAddress: 0xA2,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        SpikeChest = new Location(this, LocationId.MiseryMireSpikeChest, 0x1E9DA, LocationType.Regular,
            name: "Spike Chest",
            vanillaItem: ItemType.KeyMM,
            memoryAddress: 0xB3,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        CompassChest = new Location(this, LocationId.MiseryMireCompassChest, 0x1EA64, LocationType.Regular,
            name: "Compass Chest",
            vanillaItem: ItemType.CompassMM,
            access: items => BigKeyChest != null
                             && Logic.CanLightTorches(items)
                             && items.KeyMM >= (BigKeyChest.ItemIs(ItemType.BigKeyMM, World) ? 2 : 3),
            memoryAddress: 0xC1,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.MiseryMireBigKeyChest, 0x1EA6D, LocationType.Regular,
            name: "Big Key Chest",
            vanillaItem: ItemType.BigKeyMM,
            access: items => Logic.CanLightTorches(items)
                             && items.KeyMM >= (CompassChest.ItemIs(ItemType.BigKeyMM, World) ? 2 : 3),
            memoryAddress: 0xD1,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        BigChest = new Location(this, LocationId.MiseryMireBigChest, 0x1EA67, LocationType.Regular,
            name: "Big Chest",
            vanillaItem: ItemType.Somaria,
            access: items => items.BigKeyMM,
            memoryAddress: 0xC3,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        VitreousReward = new Location(this, LocationId.MiseryMireVitreous, 0x308158, LocationType.Regular,
            name: "Vitreous",
            vanillaItem: ItemType.HeartContainer,
            access: items => items.BigKeyMM && Logic.CanPassSwordOnlyDarkRooms(items) && items.Somaria,
            memoryAddress: 0x90,
            memoryFlag: 0xB,
            trackerLogic: items => items.HasMarkedMedallion(PrerequisiteState.MarkedItem),
            metadata: metadata,
            trackerState: trackerState);

        MemoryAddress = 0x90;
        MemoryFlag = 0xB;
        StartingRooms = new List<int> { 152 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Misery Mire");
        MapName = "Dark World";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasPrerequisite)this).ApplyState(trackerState);
        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Misery Mire";

    public RewardType DefaultRewardType => RewardType.CrystalRed;

    public BossType DefaultBossType => BossType.Vitreous;

    public bool IsShuffledReward => true;

    public ItemType DefaultRequiredItem => ItemType.Ether;

    public LocationId? BossLocationId => LocationId.MiseryMireVitreous;

    public Reward Reward { get; set; } = null!;

    public TrackerRewardState RewardState { get; set; } = null!;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public event EventHandler? UpdatedTreasure;

    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public TrackerPrerequisiteState PrerequisiteState { get; set; } = null!;

    public event EventHandler? UpdatedPrerequisite;

    public void OnUpdatedPrerequisite() => UpdatedPrerequisite?.Invoke(this, EventArgs.Empty);

    public Boss Boss { get; set; } = null!;

    public Location MainLobby { get; }

    public Location MapChest { get; }

    public Location BridgeChest { get; }

    public Location SpikeChest { get; }

    public Location CompassChest { get; }

    public Location BigKeyChest { get; }

    public Location BigChest { get; }

    public Location VitreousReward { get; }

    // Need "CanKillManyEnemies" if implementing swordless
    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return items.Contains(PrerequisiteState.RequiredItem) && items is { Sword: true, MoonPearl: true } &&
               (items.Boots || items.Hookshot) && World.DarkWorldMire.CanEnter(items, requireRewards);
    }

    public bool CanBeatBoss(Progression items) => VitreousReward.IsAvailable(items);

    public bool CanRetrieveReward(Progression items) => VitreousReward.IsAvailable(items);

    public bool CanSeeReward(Progression items) => !World.Config.ZeldaKeysanity || items.Contains(ItemType.MapMM);
}
