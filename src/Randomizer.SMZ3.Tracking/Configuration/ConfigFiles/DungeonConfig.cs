using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.Tracking.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using System.Linq;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional dungeon information
    /// </summary>
    public class DungeonConfig : List<DungeonInfo>, IMergeable<DungeonInfo>, IConfigFile<DungeonConfig>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DungeonConfig() : base()
        {
        }

        /// <summary>
        /// Returns default dungeon information
        /// </summary>
        /// <returns></returns>
        public static DungeonConfig Default()
        {
            return new DungeonConfig
            {
                new DungeonInfo()
                {
                    Dungeon = "Eastern Palace",
                    Name = new("Eastern Palace"),
                    Abbreviation = "EP",
                    Boss = new("Armos Knights"),
                    Type = typeof(Regions.Zelda.EasternPalace),
                    LocationId = 364,
                    HasReward = true,
                    TreasureRemaining = 3,
                    WithinRegionType = typeof(Regions.Zelda.LightWorld.LightWorldNorthEast),
                    X = 1925,
                    Y = 791,
                },
                new DungeonInfo()
                {
                    Dungeon = "Desert Palace",
                    Name = new("Desert Palace"),
                    Abbreviation = "DP",
                    Boss = new("Lanmolas"),
                    Type = typeof(Regions.Zelda.DesertPalace),
                    LocationId = 370,
                    HasReward = true,
                    TreasureRemaining = 2,
                    WithinRegionType = typeof(Regions.Zelda.LightWorld.LightWorldSouth),
                    X = 146,
                    Y = 1584,
                },
                new DungeonInfo()
                {
                    Dungeon = "Tower of Hera",
                    Name = new("Tower of Hera"),
                    Abbreviation = "TH",
                    Boss = new("Moldorm"),
                    Type = typeof(Regions.Zelda.TowerOfHera),
                    LocationId = 376,
                    HasReward = true,
                    TreasureRemaining = 2,
                    WithinRegionType = typeof(Regions.Zelda.LightWorld.DeathMountain.LightWorldDeathMountainWest),
                    X = 1126,
                    Y = 68,
                },
                new DungeonInfo()
                {
                    Dungeon = "Palace of Darkness",
                    Name = new("Palace of Darkness", "Dark Palace"),
                    Abbreviation = "PD",
                    Boss = new("Helmasaur King", "Helmasaur"),
                    Type = typeof(Regions.Zelda.PalaceOfDarkness),
                    LocationId = 390,
                    HasReward = true,
                    TreasureRemaining = 5,
                    WithinRegionType = typeof(Regions.Zelda.DarkWorld.DarkWorldNorthEast),
                    X = 1924,
                    Y = 800,
                },
                new DungeonInfo()
                {
                    Dungeon = "Swamp Palace",
                    Name = new("Swamp Palace"),
                    Abbreviation = "SP",
                    Boss = new("Arrghus"),
                    Type = typeof(Regions.Zelda.SwampPalace),
                    LocationId = 400,
                    HasReward = true,
                    TreasureRemaining = 6,
                    WithinRegionType = typeof(Regions.Zelda.DarkWorld.DarkWorldSouth),
                    X = 940,
                    Y = 1880,
                },
                new DungeonInfo()
                {
                    Dungeon = "Skull Woods",
                    Name = new("Skull Woods"),
                    Abbreviation = "SW",
                    Boss = new(new("Mothula", 0), "MOTHyula"),
                    Type = typeof(Regions.Zelda.SkullWoods),
                    LocationId = 408,
                    HasReward = true,
                    TreasureRemaining = 2,
                    WithinRegionType = typeof(Regions.Zelda.DarkWorld.DarkWorldNorthWest),
                    X = 79,
                    Y = 121,
                },
                new DungeonInfo()
                {
                    Dungeon = "Thieves' Town",
                    Name = new("Thieves' Town"),
                    Abbreviation = "TT",
                    Boss = new("Blind"),
                    Type = typeof(Regions.Zelda.ThievesTown),
                    LocationId = 416,
                    HasReward = true,
                    TreasureRemaining = 4,
                    WithinRegionType = typeof(Regions.Zelda.DarkWorld.DarkWorldNorthWest),
                    X = 251,
                    Y = 971,
                },
                new DungeonInfo()
                {
                    Dungeon = "Ice Palace",
                    Name = new("Ice Palace"),
                    Abbreviation = "IP",
                    Boss = new("Kholdstare"),
                    Type = typeof(Regions.Zelda.IcePalace),
                    LocationId = 424,
                    HasReward = true,
                    TreasureRemaining = 3,
                    WithinRegionType = typeof(Regions.Zelda.DarkWorld.DarkWorldSouth),
                    X = 1600,
                    Y = 1735,
                },
                new DungeonInfo()
                {
                    Dungeon = "Misery Mire",
                    Name = new("Misery Mire"),
                    Abbreviation = "MM",
                    Boss = new("Vitreous"),
                    Type = typeof(Regions.Zelda.MiseryMire),
                    LocationId = 432,
                    HasReward = true,
                    TreasureRemaining = 2,
                    WithinRegionType = typeof(Regions.Zelda.DarkWorld.DarkWorldMire),
                    X = 150,
                    Y = 1670,
                },
                new DungeonInfo()
                {
                    Dungeon = "Turtle Rock",
                    Name = new("Turtle Rock"),
                    Abbreviation = "TR",
                    Boss = new("Trinexx"),
                    Type = typeof(Regions.Zelda.TurtleRock),
                    LocationId = 444,
                    HasReward = true,
                    TreasureRemaining = 5,
                    WithinRegionType = typeof(Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainEast),
                    X = 1890,
                    Y = 144,
                },
                new DungeonInfo()
                {
                    Dungeon = "Ganon's Tower",
                    Name = new(new("Ganon's Tower", 0), "Gannon's Tower"),
                    Abbreviation = "GT",
                    Boss = new("Ganon", "Gannon", "Gannondorf", new("Gaynon", 0)),
                    Type = typeof(Regions.Zelda.GanonsTower),
                    HasReward = false,
                    TreasureRemaining = 20,
                    WithinRegionType = typeof(Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainWest),
                    X = 1126,
                    Y = 68,
                },
                new DungeonInfo()
                {
                    Dungeon = "Hyrule Castle",
                    Name = new("Hyrule Castle"),
                    Abbreviation = "HC",
                    Boss = new("Ball and Chain Soldier"),
                    Type = typeof(Regions.Zelda.HyruleCastle),
                    HasReward = false,
                    TreasureRemaining = 6,
                    WithinRegionType = typeof(Regions.Zelda.LightWorld.LightWorldNorthEast),
                    X = 1003,
                    Y = 906,
                },
                new DungeonInfo()
                {
                    Dungeon = "Agahnim's Tower",
                    Name = new("Agahnim's Tower", "Castle Tower"),
                    Abbreviation = "AT",
                    Boss = new(new("Agahnim", 0), "Aganihm"),
                    Type = typeof(Regions.Zelda.CastleTower),
                    Reward = RewardItem.Agahnim,
                    HasReward = true,
                    WithinRegionType = typeof(Regions.Zelda.LightWorld.LightWorldNorthEast),
                },
            };
        }
    }
}
