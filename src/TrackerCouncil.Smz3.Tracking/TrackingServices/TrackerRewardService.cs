using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

public class TrackerRewardService(ILogger<TrackerRewardService> logger, IItemService itemService) : TrackerService
{
    /// <summary>
    /// Sets the dungeon's reward to the specific pendant or crystal.
    /// </summary>
    /// <param name="rewardRegion">The dungeon to mark.</param>
    /// <param name="reward">
    /// The type of pendant or crystal, or <c>null</c> to cycle through the
    /// possible rewards.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was called by the auto tracker</param>
    public void SetDungeonReward(IHasReward rewardRegion, RewardType? reward = null, float? confidence = null, bool autoTracked = false)
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

        if (!autoTracked) AddUndo(() =>
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
        rewardRegion.MarkedReward = rewardRegion.RewardType;

        // TODO: Add a response

        if (!isAutoTracked)
        {
            AddUndo(() =>
            {
                rewardRegion.HasReceivedReward = false;
                rewardRegion.MarkedReward = previousMarkedReward;
            });
        }
    }

    /// <summary>
    /// Sets the reward of all unmarked dungeons.
    /// </summary>
    /// <param name="reward">The reward to set.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void SetUnmarkedDungeonReward(RewardType reward, float? confidence = null)
    {
        var unmarkedDungeons = World.RewardRegions
            .Where(x => x.MarkedReward == RewardType.None)
            .ToImmutableList();

        if (unmarkedDungeons.Count > 0)
        {
            Tracker.Say(response: Responses.RemainingDungeonsMarked, args: [itemService.GetName(reward)]);
            unmarkedDungeons.ForEach(dungeon => dungeon.MarkedReward = reward);
            AddUndo(() =>
            {
                unmarkedDungeons.ForEach(dungeon => dungeon.MarkedReward = RewardType.None);
            });
        }
        else
        {
            Tracker.Say(response: Responses.NoRemainingDungeons);
        }
    }
}
