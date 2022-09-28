using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData
{
    /// <summary>
    /// Represents a reward for a particular player
    /// </summary>
    public class Reward
    {
        public Reward(RewardType type)
        {
            Type = type;
        }

        public Reward(RewardType type, World world, IHasReward region)
        {
            Type = type;
            World = world;
            Region = region;
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
}
