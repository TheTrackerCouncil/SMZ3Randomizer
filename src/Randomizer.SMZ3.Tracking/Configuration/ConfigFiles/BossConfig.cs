using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.Tracking.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using System.Linq;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional boss information
    /// </summary>
    public class BossConfig : List<BossInfo>, IMergeable<BossInfo>, IConfigFile<BossConfig>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BossConfig() : base()
        {
        }

        /// <summary>
        /// Returns default boss information
        /// </summary>
        /// <returns></returns>
        public static BossConfig Default()
        {
            return new BossConfig
            {
                new BossInfo()
                {
                    Boss = "Spore Spawn",
                    Name = new("Spore Spawn"),
                },
                new BossInfo()
                {
                    Boss = "Botwoon",
                    Name = new("Botwoon"),
                },
                new BossInfo()
                {
                    Boss = "Kraid",
                    Name = new("Kraid"),
                    Reward = RewardType.Kraid,
                },
                new BossInfo()
                {
                    Boss = "Crocomire",
                    Name = new("Crocomire"),
                },
                new BossInfo()
                {
                    Boss = "Phantoon",
                    Name = new("Phantoon"),
                    Reward = RewardType.Phantoon,
                },
                new BossInfo()
                {
                    Boss = "Shaktool",
                    Name = new("Shaktool", "The Shaktool"),
                },
                new BossInfo()
                {
                    Boss = "Draygon",
                    Name = new("Draygon"),
                    Reward = RewardType.Draygon,
                },
                new BossInfo()
                {
                    Boss = "Ridley",
                    Name = new("Ridley"),
                    Reward = RewardType.Ridley,
                },
                new BossInfo()
                {
                    Boss = "Mother Brain",
                    Name = new("Mother Brain"),
                },
                new BossInfo()
                {
                    Boss = "Bomb Torizo",
                    Name = new("Bomb Torizo", "Bozo", "Bomb Chozo"),
                },
                new BossInfo()
                {
                    Boss = "Golden Torizo",
                    Name = new("Golden Torizo", "Golden Chozo"),
                },
            };
        }
    }
}
