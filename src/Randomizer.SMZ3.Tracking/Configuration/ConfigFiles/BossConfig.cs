using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.Tracking.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using System.Linq;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigFiles
{
    public class BossConfig : List<BossInfo>, IMergeableConfig
    {
        public BossConfig() : base()
        {
        }

        public BossConfig(IEnumerable<BossInfo> list) : base(list)
        {
        }

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
                    Column = 7,
                    Row = 5,
                },
                new BossInfo()
                {
                    Boss = "Crocomire",
                    Name = new("Crocomire"),
                    Column = 8,
                    Row = 5,
                },
                new BossInfo()
                {
                    Boss = "Phantoon",
                    Name = new("Phantoon"),
                    Reward = RewardType.Phantoon,
                    Column = 9,
                    Row = 5,
                },
                new BossInfo()
                {
                    Boss = "Shaktool",
                    Name = new("Shaktool", "The Shaktool"),
                    Column = 6,
                    Row = 6,
                },
                new BossInfo()
                {
                    Boss = "Draygon",
                    Name = new("Draygon"),
                    Reward = RewardType.Draygon,
                    Column = 7,
                    Row = 6,
                },
                new BossInfo()
                {
                    Boss = "Ridley",
                    Name = new("Ridley"),
                    Reward = RewardType.Ridley,
                    Column = 8,
                    Row = 6,
                },
                new BossInfo()
                {
                    Boss = "Mother Brain",
                    Name = new("Mother Brain"),
                    Column = 9,
                    Row = 6,
                },
            };
        }

        public void Merge(IMergeableConfig other)
        {
            ConfigMergeFunctions.MergeLists(this, other as BossConfig);
        }
    }
}
