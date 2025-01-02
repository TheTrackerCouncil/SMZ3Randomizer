using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

/// <summary>
/// Class to create worlds for a specified generated rom
/// </summary>
public class Smz3GeneratedRomLoader
{
    private readonly IWorldAccessor _worldAccessor;
    private readonly IMetadataService _metadata;
    private readonly RandomizerContext _randomizerContext;

    public Smz3GeneratedRomLoader(IWorldAccessor worldAccessor, IMetadataService metadata, RandomizerContext dbContext)
    {
        _worldAccessor = worldAccessor;
        _metadata = metadata;
        _randomizerContext = dbContext;
    }

    /// <summary>
    /// Creates a series of worls and sets up the WorldAccessor for
    /// the given GeneratedRom
    /// </summary>
    /// <param name="rom"></param>
    public List<World> LoadGeneratedRom(GeneratedRom rom)
    {
        var trackerState = rom.TrackerState;

        if (trackerState == null)
        {
            throw new InvalidOperationException("No tracker state to load");
        }

        _randomizerContext.Entry(trackerState).Collection(x => x.LocationStates).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.ItemStates).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.TreasureStates).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.BossStates).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.RewardStates).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.PrerequisiteStates).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.History).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.Hints).Load();

        UpdateGeneratedRom(rom);

        var configs = Config.FromConfigString(rom.Settings);
        var worlds = configs.Select(config => new World(config, string.IsNullOrEmpty(config.PlayerName) ? "Player" : config.PlayerName, config.Id, config.PlayerGuid,
            config.Id == trackerState.LocalWorldId, _metadata, trackerState)).ToList();

        // Load world items from state
        foreach (var location in worlds.SelectMany(x => x.Locations))
        {
            var locationState = trackerState.LocationStates.First(s =>
                s.WorldId == location.World.Id && s.LocationId == location.Id);
            var itemState = trackerState.ItemStates.First(s =>
                s.Type == locationState.Item && s.WorldId == locationState.ItemWorldId);
            var itemMetadata = _metadata.Item(locationState.Item) ?? new ItemData(locationState.Item);
            var itemWorld = worlds.First(w => w.Id == locationState.ItemWorldId);
            location.Item = new Item(locationState.Item, itemWorld,
                itemState.ItemName, itemMetadata, itemState,
                locationState.Item.IsPossibleProgression(itemWorld.Config.ZeldaKeysanity,
                    itemWorld.Config.MetroidKeysanity, itemWorld == location.World),
                locationState.ItemOwnerName, locationState.ItemName
            );
        }

        // Create items for saved state items not in the world
        foreach (var itemState in trackerState.ItemStates.Where(s => worlds.SelectMany(w => w.LocationItems).All(i => !(i.Type == s.Type && s.WorldId == i.World.Id && i.IsLocalPlayerItem))))
        {
            var itemMetadata = (itemState.Type != null && itemState.Type != ItemType.Nothing ? _metadata.Item(itemState.Type ?? ItemType.Nothing) : _metadata.Item(itemState.ItemName)) ??
                               new ItemData(new SchrodingersString(itemState.ItemName),
                                   itemState.Type ?? ItemType.Nothing, new SchrodingersString());
            var world = worlds.First(w => w.Id == itemState.WorldId);
            world.CustomItems.Add(new Item(itemState.Type ?? ItemType.Nothing, world, itemState.ItemName, itemMetadata, itemState));
        }

        // Create items for metadata items not in the world
        var allItems = worlds.SelectMany(x => x.AllItems).ToList();
        foreach (var world in worlds)
        {
            foreach (var itemMetadata in _metadata.Items.Where(m => !allItems.Any(i => i.World == world && i.Is(m.InternalItemType, m.Item))))
            {
                var itemState = new TrackerItemState
                {
                    ItemName = itemMetadata.InternalItemType == ItemType.Nothing ? itemMetadata.Item : itemMetadata.InternalItemType.GetDescription(),
                    Type = itemMetadata.InternalItemType,
                    TrackerState = trackerState,
                    WorldId = world.Id
                };

                world.CustomItems.Add(new Item(itemMetadata.InternalItemType, world, itemState.ItemName, itemMetadata, itemState));
                trackerState.ItemStates.Add(itemState);
            }
        }

        // Create custom bosses from the tracker states
        foreach (var bossState in trackerState.BossStates.Where(x => x.Type == BossType.None))
        {
            var bossMetadata = _metadata.Boss(bossState.BossName) ?? new BossInfo(bossState.BossName);
            var world = worlds.First(w => w.Id == bossState.WorldId);
            world.CustomBosses.Add(new Boss(BossType.None, world, bossMetadata, bossState));
        }

        // Create custom bosses for metadata items not in the world
        foreach (var world in worlds)
        {
            foreach (var bossMetadata in _metadata.Bosses.Where(m => m.Type == BossType.None && !world.AllBosses.Any(b => b.Is(BossType.None, m.Boss))))
            {
                var bossState = new TrackerBossState()
                {
                    BossName = bossMetadata.Boss,
                    Type = bossMetadata.Type,
                    TrackerState = trackerState,
                    WorldId = world.Id
                };

                world.CustomBosses.Add(new Boss(BossType.None, world, bossMetadata, bossState));
                trackerState.BossStates.Add(bossState);
            }
        }

        _worldAccessor.Worlds = worlds;
        _worldAccessor.World = worlds.First(x => x.IsLocalWorld);
        return worlds;
    }

#pragma warning disable CS0618 // Type or member is obsolete
    private void UpdateGeneratedRom(GeneratedRom rom)
    {
        if (!RandomizerVersion.IsVersionPreReplaceDungeonState(rom.GeneratorVersion) || rom.TrackerState?.PrerequisiteStates.Count > 0)
        {
            return;
        }

        var trackerState = rom.TrackerState!;

        _randomizerContext.Entry(trackerState).Collection(x => x.DungeonStates).Load();

        foreach (var boss in trackerState.BossStates.Where(x => x.Type != BossType.None))
        {
            boss.RegionName = MetroidBossRegions[boss.Type];

            trackerState.RewardStates.Add(new TrackerRewardState()
            {
                TrackerState = trackerState,
                RewardType = MetroidBossRewards[boss.Type],
                MarkedReward = MetroidBossRewards[boss.Type],
                HasReceivedReward = boss.Defeated,
                RegionName = MetroidBossRegions[boss.Type],
                AutoTracked = boss.AutoTracked,
                WorldId = boss.WorldId
            });
        }

        var motherBrain =
            trackerState.BossStates.FirstOrDefault(x => x is { BossName: "Mother Brain", Type: BossType.None });
        if (motherBrain != null)
        {
            motherBrain.Type = BossType.MotherBrain;
        }

        foreach (var dungeon in trackerState.DungeonStates)
        {
            var bossType = DungeonBosses[dungeon.Name];

            trackerState.BossStates.Add(new TrackerBossState()
            {
                TrackerState = trackerState,
                RegionName = dungeon.Name,
                BossName = bossType.GetDescription(),
                Defeated = dungeon.Cleared,
                AutoTracked = dungeon.AutoTracked,
                Type = bossType,
                WorldId = dungeon.WorldId
            });

            trackerState.TreasureStates.Add(new TrackerTreasureState()
            {
                TrackerState = trackerState,
                RegionName = dungeon.Name,
                RemainingTreasure = dungeon.RemainingTreasure,
                TotalTreasure = dungeon.RemainingTreasure,
                HasManuallyClearedTreasure = dungeon.HasManuallyClearedTreasure,
                WorldId = dungeon.WorldId
            });

            if (dungeon.RequiredMedallion != null)
            {
                trackerState.PrerequisiteStates.Add(new TrackerPrerequisiteState()
                {
                    TrackerState = trackerState,
                    RegionName = dungeon.Name,
                    WorldId = dungeon.WorldId,
                    AutoTracked = dungeon.AutoTracked,
                    RequiredItem = dungeon.RequiredMedallion!.Value,
                    MarkedItem = dungeon.MarkedMedallion
                });
            }

            if (dungeon.Reward != null)
            {
                trackerState.RewardStates.Add(new TrackerRewardState()
                {
                    TrackerState = trackerState,
                    RewardType = dungeon.Reward!.Value,
                    MarkedReward = dungeon.MarkedReward,
                    HasReceivedReward = dungeon.Cleared,
                    RegionName = dungeon.Name,
                    AutoTracked = dungeon.AutoTracked,
                    WorldId = dungeon.WorldId
                });
            }
        }
    }

    private Dictionary<string, BossType> DungeonBosses = new()
    {
        { "CastleTower", BossType.Agahnim },
        { "EasternPalace", BossType.ArmosKnights },
        { "DesertPalace", BossType.Lanmolas },
        { "TowerOfHera", BossType.Moldorm },
        { "PalaceOfDarkness", BossType.HelmasaurKing },
        { "SwampPalace", BossType.Arrghus },
        { "SkullWoods", BossType.Mothula },
        { "ThievesTown", BossType.Blind },
        { "IcePalace", BossType.Kholdstare },
        { "MiseryMire", BossType.Vitreous },
        { "TurtleRock", BossType.Trinexx },
        { "GanonsTower", BossType.Ganon },
        { "HyruleCastle", BossType.CastleGuard },
    };

    private Dictionary<BossType, string> MetroidBossRegions = new()
    {
        { BossType.Kraid, "KraidsLair" },
        { BossType.Phantoon, "WreckedShip" },
        { BossType.Draygon, "InnerMaridia" },
        { BossType.Ridley, "LowerNorfairEast" },
    };

    private Dictionary<BossType, RewardType> MetroidBossRewards = new()
    {
        { BossType.Kraid, RewardType.KraidToken },
        { BossType.Phantoon, RewardType.PhantoonToken },
        { BossType.Draygon, RewardType.DraygonToken },
        { BossType.Ridley, RewardType.RidleyToken },
    };

#pragma warning restore CS0618 // Type or member is obsolete
}
