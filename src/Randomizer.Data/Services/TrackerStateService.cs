using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
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

        public async Task CreateStateAsync(World world, GeneratedRom generatedRom)
        {
            var locationStates = world
                .Locations
                .Select(x => new TrackerLocationState
                {
                    LocationId = x.Id,
                    Item = x.Item?.Type,
                    Cleared = false
                })
                .ToList();

            var dungeonStates = world
                .Dungeons
                .Select(x => new TrackerDungeonState
                {
                    Name = x.GetType().Name,
                    RemainingTreasure = x.GetTreasureCount(),
                    Reward = x is IHasReward rewardRegion ? rewardRegion.RewardType : null,
                    RequiredMedallion = x is INeedsMedallion medallionRegion ? medallionRegion.Medallion : null,
                    MarkedReward = x is CastleTower ? Shared.RewardType.Agahnim : null,
                })
                .ToList();

            var state = new TrackerState()
            {
                LocationStates = locationStates,
                DungeonStates = dungeonStates,
                StartDateTime = DateTimeOffset.Now,
                UpdatedDateTime = DateTimeOffset.Now
            };

            generatedRom.TrackerState = state;
            world.State = state;
            await _randomizerContext.SaveChangesAsync();

        }

        public TrackerState? LoadState(World world, GeneratedRom generatedRom)
        {
            var trackerState = generatedRom.TrackerState;

            if (trackerState == null)
            {
                return null;
            }

            world.State = trackerState;

            LoadLocationStates(world, trackerState);
            LoadItemStates(world, trackerState);
            LoadDungeonStates(world, trackerState);
            LoadBossStates(world, trackerState);
            LoadHistory(trackerState);

            var locationCount = world.Locations.Count();
            var emptyStateLocations = world.Locations.Where(x => x.State == null).ToList();

            return trackerState;
        }


        public async Task SaveStateAsync(World world, GeneratedRom generatedRom, double secondsElapsed)
        {
            var trackerState = generatedRom.TrackerState;

            if (trackerState == null)
            {
                return;
            }

            world.State = trackerState;

            SaveLocationStates(world, trackerState);
            SaveItemStates(world, trackerState);
            SaveBossStates(world, trackerState);

            trackerState.UpdatedDateTime = DateTimeOffset.Now;
            trackerState.SecondsElapsed = secondsElapsed;
            

            await _randomizerContext.SaveChangesAsync();
        }

        private void LoadLocationStates(World world, TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.LocationStates).Load();

            foreach (var locationState in trackerState.LocationStates)
            {
                var location = world.Locations.First(x => x.Id == locationState.LocationId);
                location.State = locationState;
                location.Item = locationState.Item != null ? new Item(locationState.Item.Value, world) : null;
            }
        }

        private void SaveLocationStates(World world, TrackerState trackerState)
        {
            var totalLocations = world.Locations.Count();
            var clearedLocations = world.Locations.Where(x => x.State.Cleared).Count();
            var percCleared = (int)Math.Floor((double)clearedLocations / totalLocations * 100);
            trackerState.PercentageCleared = percCleared;
        }

        private void LoadItemStates(World world, TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.ItemStates).Load();

            // Load previously saved items
            foreach (var (state, worldItems) in trackerState.ItemStates.Select(x => (state: x, items: world.AllItems.Where(w => w.Is(x.Type ?? Shared.ItemType.Nothing, x.ItemName)))))
            {
                if (worldItems.Any())
                {
                    worldItems.ToList().ForEach(x => x.State = state);
                }
                else
                {
                    _logger.LogInformation($"{state.ItemName} not found in world");
                    var item = new Item(state.Type ?? Shared.ItemType.Nothing, world, state.ItemName)
                    {
                        State = state
                    };
                    world.TrackerItems.Add(item);
                }
            }

            // Create item states for items 
            foreach (var item in world.AllItems.Where(x => x.State == null))
            {
                var otherItem = world.AllItems
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
                    };
                    item.State = state;
                    trackerState.ItemStates.Add(state);
                }
            }
        }

        private void SaveItemStates(World world, TrackerState trackerState)
        {
            // Add any new item states
            var itemStates = world.AllItems
                .Select(x => x.State).Distinct()
                .Where(x => !trackerState.ItemStates.Contains(x))
                .ToList();
            itemStates.ForEach(x => trackerState.ItemStates.Add(x));
        }

        private void LoadDungeonStates(World world, TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.DungeonStates).Load();

            foreach (var dungeonState in trackerState.DungeonStates)
            {
                var dungeon = world.Dungeons.First(x => x.GetType().Name == dungeonState.Name);
                dungeon.DungeonState = dungeonState;

                if (dungeon is IHasReward rewardRegion && dungeonState.Reward != null)
                {
                    rewardRegion.Reward = new Reward(dungeonState.Reward.Value, world, rewardRegion)
                    {
                        State = dungeonState
                    };
                }
            }
        }

        private void LoadBossStates(World world, TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.BossStates).Load();

            // Load previously saved bosses
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

            // Create boss states for bosses not already set
            foreach (var boss in world.AllBosses.Where(x => x.State == null))
            {
                var state = new TrackerBossState()
                {
                    TrackerState = trackerState,
                    BossName = boss.Name,
                    Type = boss.Type,
                };
                boss.State = state;
                trackerState.BossStates.Add(state);
            }
        }

        private void SaveBossStates(World world, TrackerState trackerState)
        {
            // Add any new item states
            var bossStates = world.AllBosses
                .Select(x => x.State).Distinct()
                .Where(x => !trackerState.BossStates.Contains(x))
                .ToList();
            bossStates.ForEach(x => trackerState.BossStates.Add(x));
        }

        private void LoadHistory(TrackerState trackerState)
        {
            _randomizerContext.Entry(trackerState).Collection(x => x.History).Load();
        }
    }
}
