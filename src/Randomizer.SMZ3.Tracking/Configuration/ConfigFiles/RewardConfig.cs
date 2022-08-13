using System;
using System.Collections.Generic;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigFiles
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
                    RewardItem = RewardItem.Unknown
                },
                new RewardInfo()
                {
                    Reward = "Crystal",
                    Name = new("Crystal", "Blue Crystal"),
                    Article = "a",
                    RewardType = RewardType.CrystalBlue,
                    RewardItem = RewardItem.Crystal
                },
                new RewardInfo()
                {
                    Reward = "Red Crystal",
                    Name = new("Red Crystal"),
                    Article = "a",
                    RewardType = RewardType.CrystalRed,
                    RewardItem = RewardItem.RedCrystal
                },
                new RewardInfo()
                {
                    Reward = "Green Pendant",
                    Name = new("Green Pendant"),
                    Article = "the",
                    RewardType = RewardType.PendantGreen,
                    RewardItem = RewardItem.GreenPendant
                },
                new RewardInfo()
                {
                    Reward = "Red Pendant",
                    Name = new("Red Pendant"),
                    Article = "the",
                    RewardType = RewardType.PendantRed,
                    RewardItem = RewardItem.RedPendant
                },
                new RewardInfo()
                {
                    Reward = "Blue Pendant",
                    Name = new("Blue Pendant"),
                    Article = "the",
                    RewardType = RewardType.PendantBlue,
                    RewardItem = RewardItem.BluePendant
                },
                new RewardInfo()
                {
                    Reward = "Agahnim",
                    Name = new("Agahnim"),
                    Article = null,
                    RewardType = RewardType.Agahnim,
                    RewardItem = RewardItem.Agahnim
                },
                new RewardInfo()
                {
                    Reward = "Non Green Pendant",
                    Name = new("Non Green Pendant"),
                    Article = "a",
                    RewardType = RewardType.PendantBlue,
                    RewardItem = RewardItem.NonGreenPendant
                },
                new RewardInfo()
                {
                    Reward = "Kraid",
                    Name = new("Kraid"),
                    Article = null,
                    RewardType = RewardType.Kraid,
                    RewardItem = RewardItem.Unknown
                },
                new RewardInfo()
                {
                    Reward = "Phantoon",
                    Name = new("Phantoon"),
                    Article = null,
                    RewardType = RewardType.Phantoon,
                    RewardItem = RewardItem.Unknown
                },
                new RewardInfo()
                {
                    Reward = "Draygon",
                    Name = new("Draygon"),
                    Article = null,
                    RewardType = RewardType.Phantoon,
                    RewardItem = RewardItem.Unknown
                },
                new RewardInfo()
                {
                    Reward = "Ridley",
                    Name = new("Ridley"),
                    Article = null,
                    RewardType = RewardType.Phantoon,
                    RewardItem = RewardItem.Unknown
                },
            };
        }
    }
}
