using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerRewardService(ILogger<TrackerRewardService> logger, IPlayerProgressionService playerProgressionService, IMetadataService metadataService) : TrackerService, ITrackerRewardService
{
    public void SetAreaReward(IHasReward rewardRegion, RewardType? reward = null, float? confidence = null, bool autoTracked = false)
    {
        var originalReward = rewardRegion.MarkedReward;
        if (reward == null)
        {
            var currentValue = rewardRegion.MarkedReward;
            rewardRegion.MarkedReward = Enum.IsDefined(currentValue + 1) ? currentValue + 1 : RewardType.None;
            // Cycling through rewards is done via UI, so speaking the
            // reward out loud for multiple clicks is kind of annoying
        }
        else
        {
            rewardRegion.MarkedReward = reward.Value;

            var rewardObj = World.Rewards.FirstOrDefault(x => x.Type == reward.Value);
            if (rewardObj == null)
            {
                logger.LogError("Could not find a reward of type {Type} in the world", reward.Value);
                Tracker.Error();
                return;
            }

            Tracker.Say(response: Responses.DungeonRewardMarked, args: [rewardRegion.Metadata.Name, rewardObj.Metadata.Name ?? reward.GetDescription()]);
        }

        AddUndo(autoTracked, () =>
        {
            rewardRegion.MarkedReward = originalReward;
        });
    }

    public void GiveAreaReward(IHasReward rewardRegion, bool isAutoTracked, bool stateResponse)
    {
        if (rewardRegion.HasReceivedReward)
        {
            return;
        }
        
        var previousMarkedReward = rewardRegion.MarkedReward;
        rewardRegion.HasReceivedReward = true;

        if (isAutoTracked && !rewardRegion.HasCorrectlyMarkedReward)
        {
            rewardRegion.MarkedReward = rewardRegion.RewardType;

            if (rewardRegion is not SMRegion || rewardRegion.World.Config.RomGenerator != RomGenerator.Cas)
            {
                var rewardObj = rewardRegion.Reward;
                Tracker.Say(response: Responses.DungeonRewardMarked, args: [rewardRegion.Metadata.Name, rewardObj.Metadata.Name ?? rewardObj.Type.GetDescription()]);
            }

        }

        UpdateAllAccessibility(false);

        // TODO: Add a response

        AddUndo(isAutoTracked, () =>
        {
            rewardRegion.HasReceivedReward = false;
            rewardRegion.MarkedReward = previousMarkedReward;
            UpdateAllAccessibility(true);
        });
    }

    public void RemoveAreaReward(IHasReward rewardRegion, bool stateResponse)
    {
        if (!rewardRegion.HasReceivedReward)
        {
            return;
        }

        rewardRegion.HasReceivedReward = false;

        UpdateAllAccessibility(true);

        // TODO: Add a response

        AddUndo(() =>
        {
            rewardRegion.HasReceivedReward = false;
            UpdateAllAccessibility(false);
        });
    }

    public void SetUnmarkedRewards(RewardType reward, float? confidence = null)
    {
        var unmarkedRegions = World.RewardRegions
            .Where(x => x.MarkedReward == RewardType.None)
            .ToImmutableList();

        if (unmarkedRegions.Count > 0)
        {
            Tracker.Say(response: Responses.RemainingDungeonsMarked, args: [metadataService.GetName(reward)]);
            unmarkedRegions.ForEach(dungeon => dungeon.MarkedReward = reward);
            AddUndo(() =>
            {
                unmarkedRegions.ForEach(dungeon => dungeon.MarkedReward = RewardType.None);
            });
        }
        else
        {
            Tracker.Say(response: Responses.NoRemainingDungeons);
        }
    }

    public void UpdateAccessibility(Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        actualProgression ??= playerProgressionService.GetProgression(false);
        withKeysProgression ??= playerProgressionService.GetProgression(true);

        foreach (var region in Tracker.World.RewardRegions)
        {
            UpdateAccessibility(region, actualProgression, withKeysProgression);
        }
    }

    public void UpdateAccessibility(Reward reward, Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        if (reward.Region == null) return;
        UpdateAccessibility(reward.Region, actualProgression, withKeysProgression);
    }

    public void UpdateAccessibility(IHasReward region, Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        if (region.HasReceivedReward)
        {
            region.RewardAccessibility = Accessibility.Cleared;
            return;
        }

        actualProgression ??= playerProgressionService.GetProgression(false);
        withKeysProgression ??= playerProgressionService.GetProgression(true);
        region.Reward.UpdateAccessibility(actualProgression, withKeysProgression);
    }
}
