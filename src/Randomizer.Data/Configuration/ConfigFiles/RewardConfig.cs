using System;
using System.Collections.Generic;
using Randomizer.Shared;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional reward information
    /// </summary>
    public class RewardConfig : List<RewardInfo>, IMergeable<RewardInfo>, IConfigFile<RewardConfig>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RewardConfig() : base()
        {
        }

        /// <summary>
        /// Returns default reward information
        /// </summary>
        /// <returns></returns>
        public static RewardConfig Default()
        {
            return new RewardConfig
            {
                new RewardInfo()
                {
                    Reward = "Unknown",
                    Name = new("Unknown"),
                    Article = null,
                    RewardType = RewardType.None,
                },
                new RewardInfo()
                {
                    Reward = "Crystal",
                    Name = new("Crystal", "Blue Crystal"),
                    Article = "a",
                    RewardType = RewardType.CrystalBlue,
                },
                new RewardInfo()
                {
                    Reward = "Red Crystal",
                    Name = new("Red Crystal"),
                    Article = "a",
                    RewardType = RewardType.CrystalRed,
                },
                new RewardInfo()
                {
                    Reward = "Green Pendant",
                    Name = new("Green Pendant"),
                    Article = "the",
                    RewardType = RewardType.PendantGreen,
                },
                new RewardInfo()
                {
                    Reward = "Red Pendant",
                    Name = new("Red Pendant"),
                    Article = "the",
                    RewardType = RewardType.PendantRed,
                },
                new RewardInfo()
                {
                    Reward = "Blue Pendant",
                    Name = new("Blue Pendant"),
                    Article = "the",
                    RewardType = RewardType.PendantBlue,
                },
                new RewardInfo()
                {
                    Reward = "Agahnim",
                    Name = new("Agahnim"),
                    Article = null,
                    RewardType = RewardType.Agahnim,
                },
            };
        }
    }
}
