using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Shared;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Services
{
    public class TrackerStateService : ITrackerStateService
    {
        private readonly RandomizerContext _randomizerContext;
        private readonly ILogger<TrackerStateService> _logger;

        public TrackerStateService(RandomizerContext dbContext, ILogger<TrackerStateService> logger)
        {
            _randomizerContext = dbContext;
            _logger = logger;
        }

        public async Task CreateStateAsync(IEnumerable<World> worlds, GeneratedRom generatedRom)
        {
            var locationStates = worlds
                .SelectMany(x => x.Locations)
                .Select(x => new TrackerLocationState
                {
                    LocationId = x.Id,
                    Item = x.Item.Type,
                    Cleared = false,
                    Autotracked = false,
                    WorldId = x.World.Id,
                    ItemWorldId = x.Item.World.Id
                })
                .ToList();

            var addedItems = new List<(World, ItemType)>();
            var itemStates = new List<TrackerItemState>();
            foreach (var item in worlds.SelectMany(x => x.AllItems))
            {
                if (addedItems.Contains((item.World, item.Type))) continue;
                itemStates.Add(new TrackerItemState
                {
                    ItemName = item.Name,
                    Type = item.Type,
                    WorldId = item.World.Id
                });
                addedItems.Add((item.World, item.Type));
            }

            var dungeonStates = worlds
                .SelectMany(x => x.Dungeons)
                .Select(x => new TrackerDungeonState
                {
                    Name = x.GetType().Name,
                    RemainingTreasure = x.GetTreasureCount(),
                    Reward = x is IHasReward rewardRegion ? rewardRegion.RewardType : null,
                    RequiredMedallion = x is INeedsMedallion medallionRegion ? medallionRegion.Medallion : null,
                    MarkedReward = x is CastleTower ? RewardType.Agahnim : null,
                    WorldId = ((Region)x).World.Id
                })
                .ToList();

            var bossStates = worlds
                .SelectMany(x => x.AllBosses)
                .Select(boss => new TrackerBossState()
                {
                    BossName = boss.Name,
                    Type = boss.Type,
                    WorldId = boss.World.Id,
                })
                .ToList();

            var state = new TrackerState()
            {
                LocationStates = locationStates,
                ItemStates = itemStates,
                DungeonStates = dungeonStates,
                BossStates = bossStates,
                LocalWorldId = worlds.First(x => x.IsLocalWorld).Id,
                StartDateTime = DateTimeOffset.Now,
                UpdatedDateTime = DateTimeOffset.Now
            };

            foreach (var world in worlds)
            {
                world.State = state;
            }

            generatedRom.TrackerState = state;
            await _randomizerContext.SaveChangesAsync();

        }

        public TrackerState? LoadState(IEnumerable<World> worlds, GeneratedRom generatedRom)
        {
            var trackerState = generatedRom.TrackerState;

            if (trackerState == null)
            {
                return null;
            }

            foreach (var world in worlds)
            {
                world.State = trackerState;
            }

            LoadLocationStates(worlds, trackerState);
            LoadItemStates(worlds, trackerState);
            LoadDungeonStates(worlds, trackerState);
            LoadBossStates(worlds, trackerState);
            LoadHistory(trackerState);

            return trackerState;
        }


        public async Task SaveStateAsync(IEnumerable<World> worlds, GeneratedRom generatedRom, double secondsElapsed)
        {
            var trackerState = generatedRom.TrackerState;

            if (trackerState == null)
            {
                return;
            }

            foreach (var world in worlds)
            {
                world.State = trackerState;
            }

            SaveLocationStates(worlds, trackerState);
            SaveItemStates(worlds, trackerState);
            SaveBossStates(worlds, trackerState);

            trackerState.UpdatedDateTime = DateTimeOffset.Now;
            trackerState.SecondsElapsed = secondsElapsed;


            await _randomizerContext.SaveChangesAsync();
        }

        private void LoadLocationStates(IEnumerable<World> worlds, TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.LocationStates).Load();

            foreach (var locationState in trackerState.LocationStates)
            {
                var location = worlds
                    .First(x => x.Id == locationState.WorldId)
                    .Locations
                    .First(x => x.Id == locationState.LocationId);
                location.State = locationState;
                location.Item = new Item(locationState.Item, worlds.First(x => x.Id == locationState.ItemWorldId));
            }
        }

        private void SaveLocationStates(IEnumerable<World> worlds, TrackerState trackerState)
        {
            var totalLocations = worlds.SelectMany(x => x.Locations).Count();
            var clearedLocations = worlds.SelectMany(x => x.Locations).Count(x => x.State?.Cleared == true);
            var percCleared = (int)Math.Floor((double)clearedLocations / totalLocations * 100);
            trackerState.PercentageCleared = percCleared;
        }

        private void LoadItemStates(IEnumerable<World> worlds, TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.ItemStates).Load();

            // Load previously saved items
            foreach (var world in worlds)
            {
                foreach (var (state, worldItems) in trackerState.ItemStates.Select(x => (state: x, items: world.AllItems.Where(w => w.Is(x.Type ?? ItemType.Nothing, x.ItemName)))))
                {
                    if (worldItems.Any())
                    {
                        worldItems.ToList().ForEach(x => x.State = state);
                    }
                    else
                    {
                        _logger.LogInformation($"{state.ItemName} not found in world");
                        var item = new Item(state.Type ?? ItemType.Nothing, world, state.ItemName)
                        {
                            State = state
                        };
                        world.TrackerItems.Add(item);
                    }
                }
            }

            // Create item states for items
            foreach (var item in worlds.SelectMany(x => x.AllItems).Where(x => x.State == null))
            {
                var otherItem = item.World.AllItems
                    .FirstOrDefault(x => x != item && x.State != null && x.Name == item.Name);

                if (otherItem != null)
                {
                    item.State = otherItem.State;
                }
                else
                {
                    var state = new TrackerItemState()
                    {
                        TrackerState = trackerState,
                        ItemName = item.Name,
                        Type = item.Type,
                        WorldId = item.World.Id
                    };
                    item.State = state;
                    trackerState.ItemStates.Add(state);
                }
            }
        }

        private void SaveItemStates(IEnumerable<World> worlds, TrackerState trackerState)
        {
            // Add any new item states
            var itemStates = worlds
                .SelectMany(x => x.AllItems)
                .Select(x => x.State).Distinct()
                .Where(x => x != null && !trackerState.ItemStates.Contains(x))
                .NonNull()
                .ToList();
            itemStates.ForEach(x => trackerState.ItemStates.Add(x) );
        }

        private void LoadDungeonStates(IEnumerable<World> worlds, TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.DungeonStates).Load();

            foreach (var dungeonState in trackerState.DungeonStates)
            {
                var world = worlds.First(x => x.Id == dungeonState.WorldId);
                var dungeon = world.Dungeons.First(x => x.GetType().Name == dungeonState.Name);
                dungeon.DungeonState = dungeonState;

                if (dungeon is IHasReward rewardRegion && dungeonState.Reward != null)
                {
                    rewardRegion.RewardType = dungeonState.Reward.Value;
                    rewardRegion.Reward = new Reward(dungeonState.Reward.Value, world, rewardRegion)
                    {
                        State = dungeonState
                    };
                }
            }
        }

        private void LoadBossStates(IEnumerable<World> worlds, TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.BossStates).Load();

            // Load previously saved bosses
            foreach (var world in worlds)
            {
                foreach (var (state, worldBoss) in trackerState.BossStates.Select(x => (state: x, worldBoss: world.AllBosses.FirstOrDefault(w => w.Is(x.Type, x.BossName)))))
                {
                    if (worldBoss != null)
                    {
                        worldBoss.State = state;
                    }
                    else
                    {
                        _logger.LogInformation($"{state.BossName} not found in world");
                        var boss = new Boss(state.Type, world, state.BossName)
                        {
                            State = state
                        };
                        world.TrackerBosses.Add(boss);
                    }
                }
            }

            // Create boss states for bosses not already set
            foreach (var boss in worlds.SelectMany(x => x.AllBosses).Where(x => x.State == null))
            {
                var state = new TrackerBossState()
                {
                    TrackerState = trackerState,
                    BossName = boss.Name,
                    Type = boss.Type,
                    WorldId = boss.World.Id,
                };
                boss.State = state;
                trackerState.BossStates.Add(state);
            }
        }

        private void SaveBossStates(IEnumerable<World> worlds, TrackerState trackerState)
        {
            // Add any new item states
            var bossStates = worlds
                .SelectMany(x => x.AllBosses)
                .Select(x => x.State).Distinct()
                .Where(x => x != null && !trackerState.BossStates.Contains(x))
                .NonNull()
                .ToList();
            bossStates.ForEach(x => trackerState.BossStates.Add(x));
        }

        private void LoadHistory(TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.History).Load();
        }
    }
}
