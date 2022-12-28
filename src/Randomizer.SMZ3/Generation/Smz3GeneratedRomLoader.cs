using System;
using System.Linq;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Generation
{
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
        public void LoadGeneratedRom(GeneratedRom rom)
        {
            var trackerState = rom.TrackerState;

            if (trackerState == null)
            {
                throw new InvalidOperationException("No tracker state to load");
            }

            _randomizerContext.Entry(trackerState).Collection(x => x.LocationStates).Load();
            _randomizerContext.Entry(trackerState).Collection(x => x.ItemStates).Load();
            _randomizerContext.Entry(trackerState).Collection(x => x.DungeonStates).Load();
            _randomizerContext.Entry(trackerState).Collection(x => x.BossStates).Load();
            _randomizerContext.Entry(trackerState).Collection(x => x.History).Load();

            var configs = Config.FromConfigString(rom.Settings);
            var worlds = configs.Select(config => new World(config, config.PlayerName, config.Id, config.PlayerGuid, config.Id == trackerState.LocalWorldId, _metadata, trackerState)).ToList();

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
            foreach (var itemState in trackerState.ItemStates.Where(s => worlds.First(w => w.Id == s.WorldId).LocationItems.All(i => i.Type != s.Type)))
            {
                var itemMetadata = _metadata.Item(itemState.ItemName) ??
                                   new ItemData(new SchrodingersString(itemState.ItemName),
                                       itemState.Type ?? ItemType.Nothing, new SchrodingersString());
                var world = worlds.First(w => w.Id == itemState.WorldId);
                world.TrackerItems.Add(new Item(itemState.Type ?? ItemType.Nothing, world, itemState.ItemName, itemMetadata, itemState));
            }

            // Create items for metadata items not in the world
            foreach (var world in worlds)
            {
                foreach (var itemMetadata in _metadata.Items.Where(m => !world.AllItems.Any(i => i.Is(m.InternalItemType, m.Item))))
                {
                    var itemState = new TrackerItemState
                    {
                        ItemName = itemMetadata.Item,
                        Type = itemMetadata.InternalItemType,
                        TrackerState = trackerState,
                        WorldId = world.Id
                    };

                    world.TrackerItems.Add(new Item(itemMetadata.InternalItemType, world, itemMetadata.Item, itemMetadata, itemState));
                    trackerState.ItemStates.Add(itemState);
                }
            }

            // Create bosses for saved state bosses not in the world
            foreach (var bossState in trackerState.BossStates.Where(s => worlds.First(w => w.Id == s.WorldId).GoldenBosses.All(b => b.Type != s.Type)))
            {
                var bossMetadata = _metadata.Boss(bossState.BossName) ?? new BossInfo(bossState.BossName);
                var world = worlds.First(w => w.Id == bossState.WorldId);
                world.TrackerBosses.Add(new Boss(bossMetadata.Type, world, bossMetadata.Boss, bossMetadata, bossState));
            }

            // Create bosses for metadata items not in the world
            foreach (var world in worlds)
            {
                foreach (var bossMetadata in _metadata.Bosses.Where(m => !world.AllBosses.Any(b => b.Is(m.Type, m.Boss))))
                {
                    var bossState = new TrackerBossState()
                    {
                        BossName = bossMetadata.Boss,
                        Type = bossMetadata.Type,
                        TrackerState = trackerState,
                        WorldId = world.Id
                    };

                    world.TrackerBosses.Add(new Boss(bossMetadata.Type, world, bossMetadata.Boss, bossMetadata, bossState));
                    trackerState.BossStates.Add(bossState);
                }
            }


            _worldAccessor.Worlds = worlds;
            _worldAccessor.World = worlds.First(x => x.IsLocalWorld);
        }
    }
}
