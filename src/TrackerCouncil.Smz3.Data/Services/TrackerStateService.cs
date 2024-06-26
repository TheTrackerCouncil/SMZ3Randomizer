using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
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
        var state = CreateTrackerState(worlds);

        foreach (var world in worlds)
        {
            world.State = state;
        }

        generatedRom.TrackerState = state;
        await _randomizerContext.SaveChangesAsync();

    }

    public TrackerState CreateTrackerState(IEnumerable<World> worlds)
    {
        var worldList = worlds.ToList();

        var locationStates = worldList
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
        foreach (var item in worldList.SelectMany(x => x.AllItems))
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

        var dungeonStates = worldList
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

        var bossStates = worldList
            .SelectMany(x => x.AllBosses)
            .Select(boss => new TrackerBossState()
            {
                BossName = boss.Name,
                Type = boss.Type,
                WorldId = boss.World.Id,
            })
            .ToList();

        var hintStates = new List<TrackerHintState>();
        foreach (var hint in worldList.SelectMany(x => x.HintTiles))
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
        foreach (var world in worldList)
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
        foreach (var world in worldList)
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
            DungeonStates = dungeonStates,
            BossStates = bossStates,
            LocalWorldId = worldList.First(x => x.IsLocalWorld).Id,
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
                });
            world.State = trackerState;
        }

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

    private void SaveLocationStates(IEnumerable<World> worlds, TrackerState trackerState)
    {
        var totalLocations = worlds.SelectMany(x => x.Locations).Count();
        var clearedLocations = worlds.SelectMany(x => x.Locations).Count(x => x.State.Cleared == true);
        var percCleared = (int)Math.Floor((double)clearedLocations / totalLocations * 100);
        trackerState.PercentageCleared = percCleared;
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

    private void SaveBossStates(IEnumerable<World> worlds, TrackerState trackerState)
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
