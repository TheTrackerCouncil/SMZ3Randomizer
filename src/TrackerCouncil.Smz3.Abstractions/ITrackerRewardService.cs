using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerRewardService
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
    public void SetAreaReward(IHasReward rewardRegion, RewardType? reward = null, float? confidence = null,
        bool autoTracked = false);

    /// <summary>
    /// Gives the area's reward to the player
    /// </summary>
    /// <param name="rewardRegion">The region to give the reward for</param>
    /// <param name="isAutoTracked">If this is from auto tracking</param>
    /// <param name="stateResponse">If the response should be stated</param>
    public void GiveAreaReward(IHasReward rewardRegion, bool isAutoTracked, bool stateResponse);

    /// <summary>
    /// Sets the reward of all unmarked dungeons.
    /// </summary>
    /// <param name="reward">The reward to set.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void SetUnmarkedRewards(RewardType reward, float? confidence = null);

    public void UpdateAccessibility(Progression? actualProgression = null, Progression? withKeysProgression = null);

    public void UpdateAccessibility(Reward reward, Progression? actualProgression = null, Progression? withKeysProgression = null);

    public void UpdateAccessibility(IHasReward region, Progression? actualProgression = null, Progression? withKeysProgression = null);
}
