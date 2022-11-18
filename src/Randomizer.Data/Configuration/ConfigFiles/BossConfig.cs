using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.Data.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.Data.Configuration.ConfigTypes;
using System.Linq;
using Randomizer.Shared.Enums;

namespace Randomizer.Data.Configuration.ConfigFiles
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
                    MemoryAddress = 1,
                    MemoryFlag = 0x2,
                },
                new BossInfo()
                {
                    Boss = "Botwoon",
                    Name = new("Botwoon"),
                    MemoryAddress = 4,
                    MemoryFlag = 0x2,
                },
                new BossInfo()
                {
                    Boss = "Kraid",
                    Name = new("Kraid"),
                    Type = BossType.Kraid,
                    MemoryAddress = 1,
                    MemoryFlag = 0x1,
                },
                new BossInfo()
                {
                    Boss = "Crocomire",
                    Name = new("Crocomire"),
                    MemoryAddress = 2,
                    MemoryFlag = 0x2,
                },
                new BossInfo()
                {
                    Boss = "Phantoon",
                    Name = new("Phantoon"),
                    Type = BossType.Phantoon,
                    MemoryAddress = 3,
                    MemoryFlag = 0x1,
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
                    Type = BossType.Draygon,
                    MemoryAddress = 4,
                    MemoryFlag = 0x1,
                },
                new BossInfo()
                {
                    Boss = "Ridley",
                    Name = new("Ridley"),
                    Type = BossType.Ridley,
                    MemoryAddress = 2,
                    MemoryFlag = 0x1,
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
                    MemoryAddress = 0,
                    MemoryFlag = 0x4,
                },
                new BossInfo()
                {
                    Boss = "Golden Torizo",
                    Name = new("Golden Torizo", "Golden Chozo"),
                    MemoryAddress = 2,
                    MemoryFlag = 0x4,
                },
            };
        }
    }
}
