using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.SeedGenerator.Generation;

public class ParsedRomFiller : IFiller
{
    private Random _random = new Random();
    private ParsedRomDetails _parsedRomDetails = null!;

    public void SetRandom(Random random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public void Fill(List<World> worlds, Config config, CancellationToken cancellationToken)
    {
        _parsedRomDetails = config.ParsedRomDetails ??
                            throw new InvalidOperationException("ParsedRomDetails not found in config object");

        var world = worlds[0];
        FillItems(world);
        AssignRewards(world);
        AssignPrerequisites(world);
    }

    private void FillItems(World world)
    {
        foreach (var locationDetails in _parsedRomDetails.Locations)
        {
            var itemType = locationDetails.ItemType ?? ItemType.OtherGameItem;
            var location = world.FindLocation(locationDetails.Location);

            if (locationDetails.IsLocalPlayerItem)
            {
                location.Item = new Item(itemType, world);
            }
            else
            {
                location.Item = new Item(itemType, world, itemType.GetDescription(), new ItemData(itemType), new TrackerItemState(), locationDetails.IsProgression, locationDetails.PlayerName, locationDetails.ItemName);
            }
        }
    }

    private void AssignRewards(World world)
    {
        foreach (var reward in world.Rewards)
        {
            reward.Region = null;
        }

        foreach (var regionDetails in _parsedRomDetails.Rewards)
        {
            var region = world.Regions.FirstOrDefault(x => x.GetType() == regionDetails.RegionType)
                ?? throw new InvalidOperationException($"Could not find region of type '{regionDetails.RegionType.Name}'");
            if (region is not IHasReward rewardRegion)
                throw new InvalidOperationException($"{region.Name} is configured with a reward but that region cannot be configured with rewards.");

            rewardRegion.SetRewardType(regionDetails.RewardType);
        }
    }

    private void AssignPrerequisites(World world)
    {
        foreach (var prerequisite in _parsedRomDetails.Prerequisites)
        {
            var item = prerequisite.PrerequisiteItem;
            var region = world.Regions.FirstOrDefault(x => x.GetType() == prerequisite.RegionType)
                ?? throw new InvalidOperationException($"Could not find a region of type '{prerequisite.RegionType.Name}'");
            if (region is not IHasPrerequisite prerequisiteRegion)
                throw new PlandoConfigurationException($"{region.Name} is configured with a medallion but that region cannot be configured with medallions.");
            prerequisiteRegion.RequiredItem = item;
        }
    }
}
