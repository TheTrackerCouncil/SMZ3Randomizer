using System.Collections.Generic;
using System.ComponentModel;
using static Randomizer.Data.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared.Enums;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional boss information
    /// </summary>
    [Description("Config file for the various Metroid Bosses and other things that should be beatable")]
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
                new()
                {
                    Boss = "Spore Spawn",
                    MemoryAddress = 1,
                    MemoryFlag = 0x2,
                },
                new()
                {
                    Boss = "Botwoon",
                    MemoryAddress = 4,
                    MemoryFlag = 0x2,
                },
                new()
                {
                    Boss = "Kraid",
                    Type = BossType.Kraid,
                    MemoryAddress = 1,
                    MemoryFlag = 0x1,
                },
                new()
                {
                    Boss = "Crocomire",
                    MemoryAddress = 2,
                    MemoryFlag = 0x2,
                },
                new()
                {
                    Boss = "Phantoon",
                    Type = BossType.Phantoon,
                    MemoryAddress = 3,
                    MemoryFlag = 0x1,
                },
                new()
                {
                    Boss = "Shaktool",
                },
                new()
                {
                    Boss = "Draygon",
                    Type = BossType.Draygon,
                    MemoryAddress = 4,
                    MemoryFlag = 0x1,
                },
                new()
                {
                    Boss = "Ridley",
                    Type = BossType.Ridley,
                    MemoryAddress = 2,
                    MemoryFlag = 0x1,
                },
                new()
                {
                    Boss = "Mother Brain",
                },
                new()
                {
                    Boss = "Bomb Torizo",
                    MemoryAddress = 0,
                    MemoryFlag = 0x4,
                },
                new()
                {
                    Boss = "Golden Torizo",
                    MemoryAddress = 2,
                    MemoryFlag = 0x4,
                },
            };
        }

        public static object Example()
        {
            return new BossConfig
            {
                new()
                {
                    Boss = "Bomb Torizo",
                    Name = new("Bomb Torizo", "Bomb Chozo", new Possibility("Bozo", 0.1)),
                    WhenTracked = new SchrodingersString("Message when clearing the boss", new Possibility("Another message when clearing the boss", 0.1)),
                    WhenDefeated = new SchrodingersString("Message when defeating the boss", new Possibility("Another message when defeating the boss", 0.1)),
                },
            };
        }
    }
}
