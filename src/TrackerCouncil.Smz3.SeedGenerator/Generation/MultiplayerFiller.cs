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
            FillRewards(world);
            FillBosses(world);
            FillPrerequisites(world);
        }
    }

    public void SetRandom(Random random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
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

    private void FillRewards(World world)
    {
        foreach (var reward in world.Rewards)
        {
            reward.Region = null;
        }

        var rewards = world.Config.MultiplayerPlayerGenerationData!.Rewards;

        foreach (var region in world.RewardRegions)
        {
            var rewardType = rewards[region.GetType().Name];
            region.SetRewardType(rewardType);

            if (rewardType.IsInAnyCategory(RewardCategory.Metroid, RewardCategory.NonRandomized))
            {
                region.MarkedReward = rewardType;
            }
        }
    }

    private void FillBosses(World world)
    {
        foreach (var boss in world.Bosses)
        {
            boss.Region = null;
        }

        var bosses = world.Config.MultiplayerPlayerGenerationData!.Bosses;

        foreach (var region in world.BossRegions)
        {
            var bossType = bosses[region.GetType().Name];
            region.SetBossType(bossType);
        }
    }

    private void FillPrerequisites(World world)
    {
        var items = world.Config.MultiplayerPlayerGenerationData!.Prerequisites;
        foreach (var region in world.PrerequisiteRegions)
        {
            var item = items[region.GetType().Name];
            region.RequiredItem = item;
        }
    }
}
