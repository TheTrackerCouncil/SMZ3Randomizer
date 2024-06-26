using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;

public class MiseryMire : Z3Region, IHasReward, INeedsMedallion, IDungeon
{
    public static readonly int[] MusicAddresses = new[] {
        0x02D5B9
    };
    public MiseryMire(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = new[] { ItemType.KeyMM, ItemType.BigKeyMM, ItemType.MapMM, ItemType.CompassMM };

        MainLobby = new Location(this, LocationId.MiseryMireMainLobby, 0x1EA5E, LocationType.Regular,
            name: "Main Lobby",
            vanillaItem: ItemType.KeyMM,
            access: items => items.BigKeyMM || items.KeyMM >= 1,
            memoryAddress: 0xC2,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
            metadata: metadata,
            trackerState: trackerState);

        MapChest = new Location(this, LocationId.MiseryMireMapChest, 0x1EA6A, LocationType.Regular,
            name: "Map Chest",
            vanillaItem: ItemType.MapMM,
            access: items => items.BigKeyMM || items.KeyMM >= 1,
            memoryAddress: 0xC3,
            memoryFlag: 0x5,
            trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
            metadata: metadata,
            trackerState: trackerState);

        BridgeChest = new Location(this, LocationId.MiseryMireBridgeChest, 0x1EA61, LocationType.Regular,
            name: "Bridge Chest",
            vanillaItem: ItemType.KeyMM,
            memoryAddress: 0xA2,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
            metadata: metadata,
            trackerState: trackerState);

        SpikeChest = new Location(this, LocationId.MiseryMireSpikeChest, 0x1E9DA, LocationType.Regular,
            name: "Spike Chest",
            vanillaItem: ItemType.KeyMM,
            memoryAddress: 0xB3,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
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
            trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.MiseryMireBigKeyChest, 0x1EA6D, LocationType.Regular,
            name: "Big Key Chest",
            vanillaItem: ItemType.BigKeyMM,
            access: items => Logic.CanLightTorches(items)
                             && items.KeyMM >= (CompassChest.ItemIs(ItemType.BigKeyMM, World) ? 2 : 3),
            memoryAddress: 0xD1,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
            metadata: metadata,
            trackerState: trackerState);

        BigChest = new Location(this, LocationId.MiseryMireBigChest, 0x1EA67, LocationType.Regular,
            name: "Big Chest",
            vanillaItem: ItemType.Somaria,
            access: items => items.BigKeyMM,
            memoryAddress: 0xC3,
            memoryFlag: 0x4,
            trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
            metadata: metadata,
            trackerState: trackerState);

        VitreousReward = new Location(this, LocationId.MiseryMireVitreous, 0x308158, LocationType.Regular,
            name: "Vitreous",
            vanillaItem: ItemType.HeartContainer,
            access: items => items.BigKeyMM && Logic.CanPassSwordOnlyDarkRooms(items) && items.Somaria,
            memoryAddress: 0x90,
            memoryFlag: 0xB,
            trackerLogic: items => items.HasMarkedMedallion(DungeonState!.MarkedMedallion),
            metadata: metadata,
            trackerState: trackerState);

        MemoryAddress = 0x90;
        MemoryFlag = 0xB;
        StartingRooms = new List<int> { 152 };

        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Misery Mire");
        DungeonMetadata = metadata?.Dungeon(GetType()) ?? new DungeonInfo("Misery Mire", "Vitreous");
        DungeonState = trackerState?.DungeonStates.First(x => x.WorldId == world.Id && x.Name == GetType().Name) ?? new TrackerDungeonState();
        Reward = new Reward(DungeonState.Reward ?? RewardType.None, world, this, metadata, DungeonState);
        Medallion = DungeonState.RequiredMedallion ?? ItemType.Nothing;
        MapName = "Dark World";
    }

    public override string Name => "Misery Mire";

    public RewardType DefaultRewardType => RewardType.CrystalRed;

    public ItemType DefaultMedallion => ItemType.Ether;

    public int SongIndex { get; init; } = 5;
    public string Abbreviation => "MM";
    public LocationId? BossLocationId => LocationId.MiseryMireVitreous;

    public Reward Reward { get; set; }

    public DungeonInfo DungeonMetadata { get; set; }

    public TrackerDungeonState DungeonState { get; set; }

    public Region ParentRegion => World.DarkWorldMire;

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
    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return items.Contains(Medallion) && items.Sword && items.MoonPearl &&
               (items.Boots || items.Hookshot) && World.DarkWorldMire.CanEnter(items, requireRewards);
    }

    public bool CanComplete(Progression items)
    {
        return VitreousReward.IsAvailable(items);
    }
}
