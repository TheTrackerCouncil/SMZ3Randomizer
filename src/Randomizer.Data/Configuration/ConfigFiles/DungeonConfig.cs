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
                    TypeName = "EasternPalace",
                    LocationId = LocationId.EasternPalaceArmosKnights,
                },
                new DungeonInfo()
                {
                    Dungeon = "Desert Palace",
                    Name = new("Desert Palace"),
                    Abbreviation = "DP",
                    Boss = new("Lanmolas"),
                    TypeName = "DesertPalace",
                    LocationId = LocationId.DesertPalaceLanmolas,
                },
                new DungeonInfo()
                {
                    Dungeon = "Tower of Hera",
                    Name = new("Tower of Hera"),
                    Abbreviation = "TH",
                    Boss = new("Moldorm"),
                    TypeName = "TowerOfHera",
                    LocationId = LocationId.TowerOfHeraMoldorm,
                },
                new DungeonInfo()
                {
                    Dungeon = "Palace of Darkness",
                    Name = new("Palace of Darkness", "Dark Palace"),
                    Abbreviation = "PD",
                    Boss = new("Helmasaur King", "Helmasaur"),
                    TypeName = "PalaceOfDarkness",
                    LocationId = LocationId.PalaceOfDarknessHelmasaurKing,
                },
                new DungeonInfo()
                {
                    Dungeon = "Swamp Palace",
                    Name = new("Swamp Palace"),
                    Abbreviation = "SP",
                    Boss = new("Arrghus"),
                    TypeName = "SwampPalace",
                    LocationId = LocationId.SwampPalaceArrghus,
                },
                new DungeonInfo()
                {
                    Dungeon = "Skull Woods",
                    Name = new("Skull Woods"),
                    Abbreviation = "SW",
                    Boss = new(new("Mothula", 0), "MOTHyula"),
                    TypeName = "SkullWoods",
                    LocationId = LocationId.SkullWoodsMothula,
                },
                new DungeonInfo()
                {
                    Dungeon = "Thieves' Town",
                    Name = new("Thieves' Town"),
                    Abbreviation = "TT",
                    Boss = new("Blind"),
                    TypeName = "ThievesTown",
                    LocationId = LocationId.ThievesTownBlind,
                },
                new DungeonInfo()
                {
                    Dungeon = "Ice Palace",
                    Name = new("Ice Palace"),
                    Abbreviation = "IP",
                    Boss = new("Kholdstare"),
                    TypeName = "IcePalace",
                    LocationId = LocationId.IcePalaceKholdstare,
                },
                new DungeonInfo()
                {
                    Dungeon = "Misery Mire",
                    Name = new("Misery Mire"),
                    Abbreviation = "MM",
                    Boss = new("Vitreous"),
                    TypeName = "MiseryMire",
                    LocationId = LocationId.MiseryMireVitreous,
                },
                new DungeonInfo()
                {
                    Dungeon = "Turtle Rock",
                    Name = new("Turtle Rock"),
                    Abbreviation = "TR",
                    Boss = new("Trinexx"),
                    TypeName = "TurtleRock",
                    LocationId = LocationId.TurtleRockTrinexx,
                },
                new DungeonInfo()
                {
                    Dungeon = "Ganon's Tower",
                    Name = new(new("Ganon's Tower", 0), "Gannon's Tower"),
                    Abbreviation = "GT",
                    Boss = new("Ganon", "Gannon", "Gannondorf", new("Gaynon", 0)),
                    TypeName = "GanonsTower",
                },
                new DungeonInfo()
                {
                    Dungeon = "Hyrule Castle",
                    Name = new("Hyrule Castle"),
                    Abbreviation = "HC",
                    Boss = new("Ball and Chain Soldier"),
                    TypeName = "HyruleCastle",
                },
                new DungeonInfo()
                {
                    Dungeon = "Castle Tower",
                    Name = new("Agahnim's Tower", "Castle Tower"),
                    Abbreviation = "AT",
                    Boss = new(new("Agahnim", 0), "Aganihm"),
                    TypeName = "CastleTower",
                },
            };
        }
    }
}
