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

public class IcePalace : Z3Region, IHasReward, IHasTreasure, IHasBoss
{
    public IcePalace(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = new Dictionary<ItemType, ItemType>
        {
            { ItemType.Key, ItemType.KeyIP },
            { ItemType.BigKey, ItemType.BigKeyIP },
            { ItemType.Map, ItemType.MapIP },
            { ItemType.Compass, ItemType.CompassIP },
        };

        CompassChest = new Location(this, LocationId.IcePalaceCompassChest, 0x1E9D4, LocationType.Regular,
            name: "Compass Chest",
            vanillaItem: ItemType.CompassIP,
            memoryAddress: 0x2E,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        SpikeRoom = new Location(this, LocationId.IcePalaceSpikeRoom, 0x1E9E0, LocationType.Regular,
            name: "Spike Room",
            vanillaItem: ItemType.KeyIP,
            access: items => BigKeyChest != null && MapChest != null && (items.Hookshot || (items.KeyIP >= 1
                && CanNotWasteKeysBeforeAccessible(items, MapChest, BigKeyChest))),
            memoryAddress: 0x5F,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        MapChest = new Location(this, LocationId.IcePalaceMapChest, 0x1E9DD, LocationType.Regular,
            name: "Map Chest",
            vanillaItem: ItemType.MapIP,
            access: items => BigKeyChest != null && items.Hammer && Logic.CanLiftLight(items) && (
                items.Hookshot || (items.KeyIP >= 1
                                   && CanNotWasteKeysBeforeAccessible(items, SpikeRoom, BigKeyChest))
            ),
            memoryAddress: 0x3F,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.IcePalaceBigKeyChest, 0x1E9A4, LocationType.Regular,
            name: "Big Key Chest",
            vanillaItem: ItemType.BigKeyIP,
            access: items => items.Hammer && Logic.CanLiftLight(items) && (
                items.Hookshot || (items.KeyIP >= 1
                                   && CanNotWasteKeysBeforeAccessible(items, SpikeRoom, MapChest))
            ),
            memoryAddress: 0x1F,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        IcedTRoom = new Location(this, LocationId.IcePalaceIcedTRoom, 0x1E9E3, LocationType.Regular,
            name: "Iced T Room",
            vanillaItem: ItemType.KeyIP,
            memoryAddress: 0xAE,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        FreezorChest = new Location(this, LocationId.IcePalaceFreezorChest, 0x1E995, LocationType.Regular,
            name: "Freezor Chest",
            vanillaItem: ItemType.ThreeBombs,
            memoryAddress: 0x7E,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigChest = new Location(this, LocationId.IcePalaceBigChest, 0x1E9AA, LocationType.Regular,
            name: "Big Chest",
            vanillaItem: ItemType.ProgressiveTunic,
            access: items => items.BigKeyIP,
            memoryAddress: 0x9E,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        KholdstareReward = new Location(this, LocationId.IcePalaceKholdstare, 0x308157, LocationType.Regular,
            name: "Kholdstare",
            vanillaItem: ItemType.HeartContainer,
            access: items => items.BigKeyIP && items.Hammer && Logic.CanLiftLight(items) &&
                             items.KeyIP >= (items.Somaria ? 1 : 2) && (!Config.LogicConfig.KholdstareNeedsCaneOfSomaria || items.Somaria),
            memoryAddress: 0xDE,
            memoryFlag: 0xB,
            metadata: metadata,
            trackerState: trackerState);

        MemoryAddress = 0xDE;
        MemoryFlag = 0xB;
        StartingRooms = new List<int> { 14 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Ice Palace");
        MapName = "Dark World";

        ((IHasReward)this).ApplyState(trackerState);
        ((IHasTreasure)this).ApplyState(trackerState);
        ((IHasBoss)this).ApplyState(trackerState);
    }

    public override string Name => "Ice Palace";

    public RewardType DefaultRewardType => RewardType.CrystalRed;

    public BossType DefaultBossType => BossType.Kholdstare;

    public bool IsShuffledReward => true;

    public LocationId? BossLocationId => LocationId.IcePalaceKholdstare;

    public bool UnifiedBossAndItemLocation => true;

    public Reward Reward { get; set; } = null!;

    public TrackerTreasureState TreasureState { get; set; } = null!;

    public event EventHandler? UpdatedTreasure;

    public void OnUpdatedTreasure() => UpdatedTreasure?.Invoke(this, EventArgs.Empty);

    public TrackerRewardState RewardState { get; set; } = null!;

    public Boss Boss { get; set; } = null!;

    public Location CompassChest { get; }

    public Location SpikeRoom { get; }

    public Location MapChest { get; }

    public Location BigKeyChest { get; }

    public Location IcedTRoom { get; }

    public Location FreezorChest { get; }

    public Location BigChest { get; }

    public Location KholdstareReward { get; }

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return World.Logic.CanNavigateDarkWorld(items) && items.Flippers && Logic.CanLiftHeavy(items) && Logic.CanMeltFreezors(items);
    }

    public bool CanBeatBoss(Progression items) => KholdstareReward.IsAvailable(items);

    public bool CanRetrieveReward(Progression items) => KholdstareReward.IsAvailable(items);

    public bool CanSeeReward(Progression items) => !World.Config.ZeldaKeysanity || items.Contains(ItemType.MapIP);

    private bool CanNotWasteKeysBeforeAccessible(Progression items, params Location[] locations)
    {
        return !items.BigKeyIP || locations.Any(l => l.ItemIs(ItemType.BigKeyIP, World));
    }
}
