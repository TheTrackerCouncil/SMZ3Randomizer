using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;

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
                CreatePlayerReward(RewardType.Kraid, world, regions),
                CreatePlayerReward(RewardType.Phantoon, world, regions),
                CreatePlayerReward(RewardType.Draygon, world, regions),
                CreatePlayerReward(RewardType.Ridley, world, regions),
            };
        }

        private static Reward CreatePlayerReward(RewardType reward, World world, ICollection<IHasReward> regions)
        {
            var region = regions.First(x => x.Reward == reward);
            regions.Remove(region);
            return new(reward, world, region);
        }
    }
}
