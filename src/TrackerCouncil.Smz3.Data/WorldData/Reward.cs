using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData;

/// <summary>
/// Represents a reward for a particular player
/// </summary>
public class Reward
{
    public Reward(RewardType type, World world, IMetadataService? metadata)
    {
        Type = type;
        World = world;
        Metadata = metadata?.Reward(type) ?? new RewardInfo(type);
    }

    public RewardType Type { get; }

    public World World { get; }

    public RewardInfo Metadata { get; }

    public IHasReward? Region { get; set; }

    public TrackerRewardState State => Region?.RewardState ?? new TrackerRewardState();

}
