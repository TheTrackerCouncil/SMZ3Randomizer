using System;
using System.Linq;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions;

/// <summary>
/// Defines a region that offers a reward for completing it, e.g. a Zelda
/// dungeon or a Super Metroid boss.
/// </summary>
public interface IHasReward
{
    string Name { get; }

    RegionInfo Metadata { get; set; }

    World World { get; }

    /// <summary>
    /// Gets or sets the reward for completing the region, e.g. pendant or
    /// crystal.
    /// </summary>
    RewardType RewardType => Reward.Type;

    Reward Reward { get; protected set; }

    RewardInfo RewardMetadata => Reward.Metadata;

    RewardType DefaultRewardType { get; }

    TrackerRewardState RewardState { get; set; }

    public bool IsShuffledReward { get; }

    Region Region => (Region)this;

    public void SetReward(Reward reward)
    {
        Reward = reward;
        Reward.Region = this;
        RewardState.RewardType = reward.Type;
    }

    public void SetRewardType(RewardType rewardType)
    {
        var region = (Region)this;
        Reward = region.World.Rewards.First(x => x.Type == rewardType && x.Region == null);
        Reward.Region = this;
        RewardState.RewardType = rewardType;
        if (rewardType.IsInCategory(RewardCategory.NonRandomized))
        {
            RewardState.MarkedReward = RewardType;
        }
    }

    public RewardType MarkedReward
    {
        get => Reward.MarkedReward ?? RewardType.None;
        set => Reward.MarkedReward = value;
    }

    public Accessibility RewardAccessibility
    {
        get => Reward.Accessibility;
        set => Reward.Accessibility = value;
    }

    public bool HasCorrectlyMarkedReward => Reward.HasCorrectlyMarkedReward;

    public bool HasReceivedReward
    {
        get => Reward.HasReceivedReward;
        set => Reward.HasReceivedReward = value;
    }

    /// <summary>
    /// Determines whether the reward for the region can be obtained.
    /// </summary>
    /// <param name="items">The items currently available.</param>
    /// <param name="isTracking">If this is checking for tracking rather than seed generation</param>
    /// <returns>
    /// <see langword="true"/> if the region can be completed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool CanRetrieveReward(Progression items, bool isTracking);

    /// <summary>
    /// Determines if the user can see what the reward is
    /// </summary>
    /// <param name="items">The items currently available</param>
    /// <returns>
    /// <see langword="true"/> if the reward can be seen; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool CanSeeReward(Progression items);

    public Accessibility GetKeysanityAdjustedBossAccessibility()
    {
        return Region.GetKeysanityAdjustedAccessibility(RewardAccessibility);
    }

    public bool HasReward(params RewardType[] types) => types.Contains(RewardType);

    public void ApplyState(TrackerState? state)
    {
        var region = (Region)this;

        if (state == null)
        {
            RewardState = new TrackerRewardState
            {
                WorldId = region.World.Id, RegionName = GetType().Name
            };
            SetRewardType(DefaultRewardType);
        }
        else
        {
            RewardState = state.RewardStates.First(x =>
                x.WorldId == region.World.Id && x.RegionName == GetType().Name);
            SetRewardType(RewardState.RewardType);
        }
    }
}
