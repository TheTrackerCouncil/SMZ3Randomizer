using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

/// <summary>
/// Fills an SMZ3 world with items according to pre-planned configuration.
/// </summary>
public class MultiplayerFiller : IFiller
{
    private readonly ILogger<MultiplayerFiller> _logger;
    private Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlandoFiller"/> class
    /// with the specified configuration.
    /// </summary>
    /// <param name="logger">Used to write logging information.</param>
    public MultiplayerFiller(ILogger<MultiplayerFiller> logger)
    {
        _logger = logger;
        _random = new Random();
    }

    /// <summary>
    /// Randomly distributes items across locations in the specified worlds.
    /// </summary>
    /// <param name="worlds">The world or worlds to initialize.</param>
    /// <param name="config">The configuration to use.</param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests.
    /// </param>
    /// <exception cref="PlandoConfigurationException">
    /// The plando configuration contains one or more errors.
    /// </exception>
    public void Fill(List<World> worlds, Config config, CancellationToken cancellationToken)
    {
        foreach (var world in worlds)
        {
            FillItems(world, worlds);
            FillDungeonData(world);
        }
    }

    public void SetRandom(Random random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    private static void EnsureDungeonsHaveMedallions(World world)
    {
        var emptyMedallions = world.Regions.Where(x => x is IHasPrerequisite { RequiredItem: ItemType.Nothing }).ToList();
        if (emptyMedallions.Any())
        {
            throw new PlandoConfigurationException($"Not all dungeons have had their medallions set. Missing:\n"
                                                   + string.Join('\n', emptyMedallions.Select(x => x.Name)));
        }
    }

    private static void EnsureDungeonsHaveRewards(World world)
    {
        var emptyDungeons = world.Regions.Where(x => x is IHasReward { RewardType: RewardType.None }).ToList();
        if (emptyDungeons.Any())
        {
            throw new PlandoConfigurationException($"Not all dungeons have had their rewards set. Missing:\n"
                                                   + string.Join('\n', emptyDungeons.Select(x => x.Name)));
        }
    }

    private static void EnsureLocationsHaveItems(World world)
    {
        var emptyLocations = world.Locations.Where(x => x.Item.Type == ItemType.Nothing).ToList();
        if (emptyLocations.Any())
        {
            throw new PlandoConfigurationException($"Not all locations have been filled. Missing:\n"
                                                   + string.Join('\n', emptyLocations.Select(x => x.Name)));
        }
    }

    private void FillDungeonData(World world)
    {
        foreach (var reward in world.Rewards)
        {
            reward.Region = null;
        }

        var generatedDungeonData = world.Config.MultiplayerPlayerGenerationData!.Dungeons;
        foreach (var dungeon in world.TreasureRegions)
        {
            var generatedData = generatedDungeonData.Single(x => x.Name == dungeon.Name);
            if (generatedData.Medallion != ItemType.Nothing && dungeon is IHasPrerequisite medallionRegion)
            {
                medallionRegion.RequiredItem = generatedData.Medallion;
                _logger.LogDebug("Marked {Dungeon} as requiring {Medallion}", generatedData.Name, generatedData.Medallion);
            }
        }

        foreach (var rewardRegion in world.RewardRegions)
        {
            var generatedData = generatedDungeonData.FirstOrDefault(x => x.Name == rewardRegion.Name);

            if (generatedData?.Reward != null)
            {
                rewardRegion.SetRewardType(generatedData.Reward.Value);
                _logger.LogDebug("Marked {Dungeon} as having {Reward}", generatedData.Name, generatedData.Reward);
            }
            else
            {
                rewardRegion.SetRewardType(rewardRegion.DefaultRewardType);
            }
        }

        EnsureDungeonsHaveMedallions(world);
        EnsureDungeonsHaveRewards(world);
    }

    private void FillItems(World world, List<World> worlds)
    {
        var generatedLocationData = world.Config.MultiplayerPlayerGenerationData!.Locations;
        foreach (var location in world.Locations)
        {
            var generatedData = generatedLocationData.Single(x => x.Id == location.Id);
            var itemType = generatedData.Item;
            var itemWorld = worlds.Single(x => x.Id == generatedData.ItemWorldId);
            location.Item = new Item(itemType, itemWorld, isProgression: itemType.IsPossibleProgression(itemWorld.Config.ZeldaKeysanity, itemWorld.Config.MetroidKeysanity));
            _logger.LogDebug("Fast-filled {Item} at {Location}", generatedData.Item, location.Name);
        }
        EnsureLocationsHaveItems(world);
    }
}
