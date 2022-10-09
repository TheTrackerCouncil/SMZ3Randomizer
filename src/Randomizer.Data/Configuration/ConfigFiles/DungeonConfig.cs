using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.Data.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.Data.Configuration.ConfigTypes;
using System.Linq;
using Randomizer.Data;

namespace Randomizer.Data.Configuration.ConfigFiles
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
                    Type = typeof(Data.WorldData.Regions.Zelda.EasternPalace),
                    LocationId = 364,
                },
                new DungeonInfo()
                {
                    Dungeon = "Desert Palace",
                    Name = new("Desert Palace"),
                    Abbreviation = "DP",
                    Boss = new("Lanmolas"),
                    Type = typeof(Data.WorldData.Regions.Zelda.DesertPalace),
                    LocationId = 370,
                },
                new DungeonInfo()
                {
                    Dungeon = "Tower of Hera",
                    Name = new("Tower of Hera"),
                    Abbreviation = "TH",
                    Boss = new("Moldorm"),
                    Type = typeof(Data.WorldData.Regions.Zelda.TowerOfHera),
                    LocationId = 376,
                },
                new DungeonInfo()
                {
                    Dungeon = "Palace of Darkness",
                    Name = new("Palace of Darkness", "Dark Palace"),
                    Abbreviation = "PD",
                    Boss = new("Helmasaur King", "Helmasaur"),
                    Type = typeof(Data.WorldData.Regions.Zelda.PalaceOfDarkness),
                    LocationId = 390,
                },
                new DungeonInfo()
                {
                    Dungeon = "Swamp Palace",
                    Name = new("Swamp Palace"),
                    Abbreviation = "SP",
                    Boss = new("Arrghus"),
                    Type = typeof(Data.WorldData.Regions.Zelda.SwampPalace),
                    LocationId = 400,
                },
                new DungeonInfo()
                {
                    Dungeon = "Skull Woods",
                    Name = new("Skull Woods"),
                    Abbreviation = "SW",
                    Boss = new(new("Mothula", 0), "MOTHyula"),
                    Type = typeof(Data.WorldData.Regions.Zelda.SkullWoods),
                    LocationId = 408,
                },
                new DungeonInfo()
                {
                    Dungeon = "Thieves' Town",
                    Name = new("Thieves' Town"),
                    Abbreviation = "TT",
                    Boss = new("Blind"),
                    Type = typeof(Data.WorldData.Regions.Zelda.ThievesTown),
                    LocationId = 416,
                },
                new DungeonInfo()
                {
                    Dungeon = "Ice Palace",
                    Name = new("Ice Palace"),
                    Abbreviation = "IP",
                    Boss = new("Kholdstare"),
                    Type = typeof(Data.WorldData.Regions.Zelda.IcePalace),
                    LocationId = 424,
                },
                new DungeonInfo()
                {
                    Dungeon = "Misery Mire",
                    Name = new("Misery Mire"),
                    Abbreviation = "MM",
                    Boss = new("Vitreous"),
                    Type = typeof(Data.WorldData.Regions.Zelda.MiseryMire),
                    LocationId = 432,
                },
                new DungeonInfo()
                {
                    Dungeon = "Turtle Rock",
                    Name = new("Turtle Rock"),
                    Abbreviation = "TR",
                    Boss = new("Trinexx"),
                    Type = typeof(Data.WorldData.Regions.Zelda.TurtleRock),
                    LocationId = 444,
                },
                new DungeonInfo()
                {
                    Dungeon = "Ganon's Tower",
                    Name = new(new("Ganon's Tower", 0), "Gannon's Tower"),
                    Abbreviation = "GT",
                    Boss = new("Ganon", "Gannon", "Gannondorf", new("Gaynon", 0)),
                    Type = typeof(Data.WorldData.Regions.Zelda.GanonsTower),
                },
                new DungeonInfo()
                {
                    Dungeon = "Hyrule Castle",
                    Name = new("Hyrule Castle"),
                    Abbreviation = "HC",
                    Boss = new("Ball and Chain Soldier"),
                    Type = typeof(Data.WorldData.Regions.Zelda.HyruleCastle),
                },
                new DungeonInfo()
                {
                    Dungeon = "Castle Tower",
                    Name = new("Agahnim's Tower", "Castle Tower"),
                    Abbreviation = "AT",
                    Boss = new(new("Agahnim", 0), "Aganihm"),
                    Type = typeof(Data.WorldData.Regions.Zelda.CastleTower),
                },
            };
        }
    }
}
