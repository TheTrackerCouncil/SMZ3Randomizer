using System;
using System.Collections.Generic;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerPrerequisiteService(IPlayerProgressionService playerProgressionService, ITrackerLocationService trackerLocationService, ITrackerBossService trackerBossService, ITrackerRewardService trackerRewardService) : TrackerService, ITrackerPrerequisiteService
{
    public void SetDungeonRequirement(IHasPrerequisite region, ItemType? medallion = null, float? confidence = null, bool autoTracked = false)
    {
        var originalRequirement = region.MarkedItem ?? ItemType.Nothing;

        // If no medallion was passed, increment by one
        if (medallion == null)
        {
            var medallionItems = new List<ItemType>(Enum.GetValues<ItemType>());
            medallionItems.Insert(0, ItemType.Nothing);
            var index = (medallionItems.IndexOf(originalRequirement) + 1) % medallionItems.Count;
            region.MarkedItem = medallionItems[index];
        }
        else
        {
            if (region.RequiredItem != ItemType.Nothing
                && region.RequiredItem != medallion.Value
                && confidence >= Options.MinimumSassConfidence)
            {
                Tracker.Say(response: Responses.DungeonRequirementMismatch,
                    args: [
                        HintsEnabled ? "a different medallion" : region.RequiredItem.ToString(),
                        region.Metadata.Name,
                        medallion.Value.ToString()
                    ]);
            }

            region.MarkedItem = medallion.Value;
            Tracker.Say(response: Responses.DungeonRequirementMarked, args: [medallion.ToString(), region.Metadata.Name]);
        }

        UpdateAccessibility(region);

        AddUndo(autoTracked, () =>
        {
            region.MarkedItem = originalRequirement;
            UpdateAccessibility(region);
        });
    }

    private void UpdateAccessibility(IHasPrerequisite region)
    {
        var actualProgression = playerProgressionService.GetProgression(false);
        var withKeysProgression = playerProgressionService.GetProgression(true);

        foreach (var location in ((Region)region).Locations)
        {
            trackerLocationService.UpdateAccessibility(location, actualProgression, withKeysProgression);
        }

        if (region is IHasBoss bossRegion)
        {
            trackerBossService.UpdateAccessibility(bossRegion,  actualProgression, withKeysProgression);
        }

        if (region is IHasReward rewardRegion)
        {
            trackerRewardService.UpdateAccessibility(rewardRegion, actualProgression, withKeysProgression);
        }
    }
}
