using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.Services;

public class TrackerStateService : ITrackerStateService
{
    private readonly RandomizerContext _randomizerContext;
    private readonly ILogger<TrackerStateService> _logger;
    private readonly Configs _configs;

    public TrackerStateService(RandomizerContext dbContext, ILogger<TrackerStateService> logger, Configs configs)
    {
        _randomizerContext = dbContext;
        _logger = logger;
        _configs = configs;
    }

    public async Task CreateStateAsync(IEnumerable<World> worlds, GeneratedRom generatedRom)
    {
        var worldList = worlds.ToList();

        var state = CreateTrackerState(worldList);

        foreach (var world in worldList)
        {
            world.State = state;
        }

        generatedRom.TrackerState = state;
        await _randomizerContext.SaveChangesAsync();
    }

    public TrackerState CreateTrackerState(List<World> worlds)
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

        var rewardStates = worlds
            .SelectMany(x => x.RewardRegions)
            .Select(x => new TrackerRewardState
            {
                RegionName = x.GetType().Name,
                RewardType = x.RewardType,
                MarkedReward = x.MarkedReward
            })
            .ToList();

        var treasureStates = worlds
            .SelectMany(x => x.TreasureRegions)
            .Select(region => new TrackerTreasureState
            {
                RegionName = region.GetType().Name,
                RemainingTreasure = region.GetTreasureCount(),
                TotalTreasure = region.GetTreasureCount(),
                WorldId = ((Region)region).World.Id
            })
            .ToList();

        var bossStates = worlds
            .SelectMany(x => x.AllBosses)
            .Select(boss => new TrackerBossState
            {
                BossName = boss.Name,
                RegionName = boss.Region?.GetType().Name ?? string.Empty,
                Type = boss.Type,
                WorldId = boss.World.Id,
            })
            .ToList();

        var prereqStates = worlds
            .SelectMany(x => x.PrerequisiteRegions)
            .Select(region => new TrackerPrerequisiteState
            {
                RequiredItem = region.RequiredItem,
                RegionName = region.GetType().Name,
                WorldId = region.World.Id
            })
            .ToList();


        var hintStates = new List<TrackerHintState>();
        foreach (var hint in worlds.SelectMany(x => x.HintTiles))
        {
            var hintState = new TrackerHintState()
            {
                Type = hint.Type,
                WorldId = hint.WorldId,
                LocationKey = hint.LocationKey,
                LocationWorldId = hint.LocationWorldId,
                LocationString =
                    hint.Locations == null ? null : string.Join(",", hint.Locations.Select(x => (int)x)),
                Usefulness = hint.Usefulness,
                MedallionType = hint.MedallionType,
                HintTileCode = hint.HintTileCode
            };
            hint.State = hintState;
            hintStates.Add(hintState);
        }

        // Add starting equipment, including items that may not be in the world anymore
        foreach (var world in worlds)
        {
            var startingInventory = ItemSettingOptions.GetStartingItemTypes(world.Config).ToList();
            foreach (var itemType in startingInventory.Distinct())
            {
                _logger.LogInformation("Adding Starting Inventory Item {ItemName} to Tracker State", itemType.GetDescription());
                var addedItem = itemStates.FirstOrDefault(x => x.WorldId == world.Id && x.Type == itemType);
                if (addedItem != null)
                {
                    addedItem.TrackingState = startingInventory.Count(x => x == itemType);
                }
                else
                {
                    itemStates.Add(new TrackerItemState
                    {
                        ItemName = itemType.GetDescription(),
                        Type = itemType,
                        WorldId = world.Id,
                        TrackingState = startingInventory.Count(x => x == itemType)
                    });
                }
            }
        }

        // Add items from metadata that may be missing
        foreach (var world in worlds)
        {
            foreach (var itemMetadata in _configs.Items.Where(m => !itemStates.Any(s => m.Is(s) && s.WorldId == world.Id)))
            {
                _logger.LogInformation("Adding Metadata Item {ItemName} to Tracker State", itemMetadata.Item);
                itemStates.Add(new TrackerItemState
                {
                    ItemName = itemMetadata.InternalItemType == ItemType.Nothing ? itemMetadata.Item : itemMetadata.InternalItemType.GetDescription(),
                    Type = itemMetadata.InternalItemType,
                    WorldId = world.Id,
                    TrackingState = 0
                });
            }
        }

        return new TrackerState()
        {
            LocationStates = locationStates,
            ItemStates = itemStates,
            TreasureStates = treasureStates,
            BossStates = bossStates,
            RewardStates = rewardStates,
            PrerequisiteStates = prereqStates,
            LocalWorldId = worlds.First(x => x.IsLocalWorld).Id,
            Hints = hintStates,
            StartDateTime = DateTimeOffset.Now,
            UpdatedDateTime = DateTimeOffset.Now
        };
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
            world.HintTiles = trackerState.Hints.Where(x => x.WorldId == world.Id)
                .Select(hint => new PlayerHintTile()
                {
                    Type = hint.Type,
                    WorldId = hint.WorldId,
                    LocationKey = hint.LocationKey,
                    LocationWorldId = hint.LocationWorldId,
                    Locations = hint.LocationString?.Split(",").Select(x => (LocationId)int.Parse(x)),
                    Usefulness = hint.Usefulness,
                    MedallionType = hint.MedallionType,
                    HintTileCode = hint.HintTileCode,
                    State = hint
                }).ToImmutableList();
            world.State = trackerState;
        }

        return trackerState;
    }


    public async Task SaveStateAsync(IEnumerable<World> worlds, GeneratedRom generatedRom, double secondsElapsed)
    {
        var worldList = worlds.ToList();
        var trackerState = generatedRom.TrackerState;

        if (trackerState == null)
        {
            return;
        }

        foreach (var world in worldList)
        {
            world.State = trackerState;
        }

        SaveLocationStates(worldList, trackerState);
        SaveItemStates(worldList, trackerState);
        SaveBossStates(worldList, trackerState);

        trackerState.UpdatedDateTime = DateTimeOffset.Now;
        trackerState.SecondsElapsed = secondsElapsed;

        await _randomizerContext.SaveChangesAsync();
    }

    private void SaveLocationStates(List<World> worlds, TrackerState trackerState)
    {
        var totalLocations = worlds.SelectMany(x => x.Locations).Count();
        var clearedLocations = worlds.SelectMany(x => x.Locations).Count(x => x.State.Cleared);
        var percCleared = (int)Math.Floor((double)clearedLocations / totalLocations * 100);
        trackerState.PercentageCleared = percCleared;
    }

    private void SaveItemStates(List<World> worlds, TrackerState trackerState)
    {
        // Add any new item states
        var itemStates = worlds
            .SelectMany(x => x.AllItems)
            .Select(x => x.State).Distinct()
            .Where(x => !trackerState.ItemStates.Contains(x))
            .NonNull()
            .ToList();
        itemStates.ForEach(x => trackerState.ItemStates.Add(x) );
    }

    private void SaveBossStates(List<World> worlds, TrackerState trackerState)
    {
        // Add any new item states
        var bossStates = worlds
            .SelectMany(x => x.AllBosses)
            .Select(x => x.State).Distinct()
            .Where(x => !trackerState.BossStates.Contains(x))
            .NonNull()
            .ToList();
        bossStates.ForEach(x => trackerState.BossStates.Add(x));
    }
}
