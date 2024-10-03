using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
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
        _randomizerContext.Entry(trackerState).Collection(x => x.BossStates).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.RewardStates).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.PrerequisiteStates).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.History).Load();
        _randomizerContext.Entry(trackerState).Collection(x => x.Hints).Load();

        var configs = Config.FromConfigString(rom.Settings);
        var worlds = configs.Select(config => new World(config, config.PlayerName, config.Id, config.PlayerGuid,
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
                    itemWorld.Config.MetroidKeysanity));
        }

        // Create items for saved state items not in the world
        foreach (var itemState in trackerState.ItemStates.Where(s => worlds.SelectMany(w => w.LocationItems).All(i => i.World.Id != s.WorldId || i.Type != s.Type)))
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
            foreach (var bossMetadata in _metadata.Bosses.Where(m => !world.AllBosses.Any(b => b.Is(BossType.None, m.Boss))))
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
}
