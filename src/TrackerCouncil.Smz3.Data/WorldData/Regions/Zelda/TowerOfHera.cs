﻿using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;

public class TowerOfHera : Z3Region, IHasReward, IDungeon
{
    public static readonly int[] MusicAddresses = new[] {
        0x02D5C5,
        0x02907A,
        0x028B8C
    };

    public TowerOfHera(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
    {
        RegionItems = new[] { ItemType.KeyTH, ItemType.BigKeyTH, ItemType.MapTH, ItemType.CompassTH };

        BasementCage = new Location(this, LocationId.TowerOfHeraBasementCage, 0x308162, LocationType.HeraStandingKey,
            name: "Basement Cage",
            vanillaItem: ItemType.KeyTH,
            memoryAddress: 0x87,
            memoryFlag: 0xA,
            metadata: metadata,
            trackerState: trackerState);

        MapChest = new Location(this, LocationId.TowerOfHeraMapChest, 0x1E9AD, LocationType.Regular,
            name: "Map Chest",
            vanillaItem: ItemType.MapTH,
            memoryAddress: 0x77,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        BigKeyChest = new Location(this, LocationId.TowerOfHeraBigKeyChest, 0x1E9E6, LocationType.Regular,
                name: "Big Key Chest",
                vanillaItem: ItemType.BigKeyTH,
                access: items => items.KeyTH && Logic.CanLightTorches(items),
                memoryAddress: 0x87,
                memoryFlag: 0x4,
                metadata: metadata,
                trackerState: trackerState)
            .AlwaysAllow((item, items) => item.Is(ItemType.KeyTH, World));

        CompassChest = new Location(this, LocationId.TowerOfHeraCompassChest, 0x1E9FB, LocationType.Regular,
            name: "Compass Chest",
            vanillaItem: ItemType.CompassTH,
            access: items => items.BigKeyTH,
            memoryAddress: 0x27,
            memoryFlag: 0x5,
            metadata: metadata,
            trackerState: trackerState);

        BigChest = new Location(this, LocationId.TowerOfHeraBigChest, 0x1E9F8, LocationType.Regular,
            name: "Big Chest",
            vanillaItem: ItemType.MoonPearl,
            access: items => items.BigKeyTH,
            memoryAddress: 0x27,
            memoryFlag: 0x4,
            metadata: metadata,
            trackerState: trackerState);

        MoldormReward = new Location(this, LocationId.TowerOfHeraMoldorm, 0x308152, LocationType.Regular,
            name: "Moldorm",
            vanillaItem: ItemType.HeartContainer,
            access: items => items.BigKeyTH && CanBeatBoss(items),
            memoryAddress: 0x7,
            memoryFlag: 0xB,
            metadata: metadata,
            trackerState: trackerState);

        MemoryAddress = 0x7;
        MemoryFlag = 0xB;
        StartingRooms = new List<int> { 119 };
        Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Tower of Hera");
        DungeonMetadata = metadata?.Dungeon(GetType()) ?? new DungeonInfo("Tower of Hera", "Moldorm");
        DungeonState = trackerState?.DungeonStates.First(x => x.WorldId == world.Id && x.Name == GetType().Name) ?? new TrackerDungeonState();
        Reward = new Reward(DungeonState.Reward ?? RewardType.None, world, this, metadata, DungeonState);
        MapName = "Light World";
    }

    public override string Name => "Tower of Hera";

    public RewardType DefaultRewardType => RewardType.PendantRed;

    public int SongIndex { get; init; } = 8;
    public string Abbreviation => "TH";
    public LocationId? BossLocationId => LocationId.TowerOfHeraMoldorm;

    public Reward Reward { get; set; }

    public DungeonInfo DungeonMetadata { get; set; }

    public TrackerDungeonState DungeonState { get; set; }

    public Region ParentRegion => World.LightWorldNorthWest;

    public Location BasementCage { get; }

    public Location MapChest { get; }

    public Location BigKeyChest { get; }

    public Location CompassChest { get; }

    public Location BigChest { get; }

    public Location MoldormReward { get; }

    public override bool CanEnter(Progression items, bool requireRewards)
    {
        return (items.Mirror || (items.Hookshot && items.Hammer))
               && World.LightWorldDeathMountainWest.CanEnter(items, requireRewards);
    }

    public bool CanComplete(Progression items)
    {
        return MoldormReward.IsAvailable(items);
    }

    private bool CanBeatBoss(Progression items)
    {
        return items.Sword || items.Hammer;
    }
}
