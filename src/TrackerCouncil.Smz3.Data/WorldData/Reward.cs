using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Shared;
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
    public Reward(RewardType type, World world, IHasReward region, IMetadataService? metadata = null, TrackerDungeonState? dungeonState = null)
    {
        Type = type;
        World = world;
        Region = region;
        Metadata = metadata?.Reward(type) ?? new RewardInfo(type);
        State = dungeonState ?? new TrackerDungeonState();
    }

    public RewardType Type { get; set; }

    public World World { get; set; }

    public IHasReward Region { get; set; }

    public RewardInfo Metadata { get; set; }

    public TrackerDungeonState State { get; set; }

    public static ICollection<Reward> CreatePool(World world)
    {
        var regions = world.Regions.OfType<IHasReward>().ToList();

        return new List<Reward>()
        {
            CreatePlayerReward(RewardType.PendantGreen, world, regions),
            CreatePlayerReward(RewardType.PendantRed, world, regions),
            CreatePlayerReward(RewardType.PendantBlue, world, regions),
            CreatePlayerReward(RewardType.CrystalBlue, world, regions),
            CreatePlayerReward(RewardType.CrystalBlue, world, regions),
            CreatePlayerReward(RewardType.CrystalBlue, world, regions),
            CreatePlayerReward(RewardType.CrystalBlue, world, regions),
            CreatePlayerReward(RewardType.CrystalBlue, world, regions),
            CreatePlayerReward(RewardType.CrystalRed, world, regions),
            CreatePlayerReward(RewardType.CrystalRed, world, regions),
        };
    }

    private static Reward CreatePlayerReward(RewardType reward, World world, ICollection<IHasReward> regions)
    {
        var region = regions.First(x => x.RewardType == reward);
        regions.Remove(region);
        return new(reward, world, region);
    }
}
