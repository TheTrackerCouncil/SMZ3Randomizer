using System.Collections.Generic;
using System.ComponentModel;
using Randomizer.Shared;
using static Randomizer.Data.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData.Regions.Zelda;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional dungeon information
    /// </summary>
    [Description("Config file for the various Zelda dungeons with collectable treasure in them")]
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
                new()
                {
                    Dungeon = "Eastern Palace",
                    Abbreviation = "EP",
                    Type = typeof(EasternPalace),
                    LocationId = LocationId.EasternPalaceArmosKnights,
                },
                new()
                {
                    Dungeon = "Desert Palace",
                    Abbreviation = "DP",
                    Type = typeof(DesertPalace),
                    LocationId = LocationId.DesertPalaceLanmolas,
                },
                new()
                {
                    Dungeon = "Tower of Hera",
                    Abbreviation = "TH",
                    Type = typeof(TowerOfHera),
                    LocationId = LocationId.TowerOfHeraMoldorm,
                },
                new()
                {
                    Dungeon = "Palace of Darkness",
                    Abbreviation = "PD",
                    Type = typeof(PalaceOfDarkness),
                    LocationId = LocationId.PalaceOfDarknessHelmasaurKing,
                },
                new()
                {
                    Dungeon = "Swamp Palace",
                    Abbreviation = "SP",
                    Type = typeof(SwampPalace),
                    LocationId = LocationId.SwampPalaceArrghus,
                },
                new()
                {
                    Dungeon = "Skull Woods",
                    Abbreviation = "SW",
                    Type = typeof(SkullWoods),
                    LocationId = LocationId.SkullWoodsMothula,
                },
                new()
                {
                    Dungeon = "Thieves' Town",
                    Abbreviation = "TT",
                    Type = typeof(ThievesTown),
                    LocationId = LocationId.ThievesTownBlind,
                },
                new()
                {
                    Dungeon = "Ice Palace",
                    Abbreviation = "IP",
                    Type = typeof(IcePalace),
                    LocationId = LocationId.IcePalaceKholdstare,
                },
                new()
                {
                    Dungeon = "Misery Mire",
                    Abbreviation = "MM",
                    Type = typeof(MiseryMire),
                    LocationId = LocationId.MiseryMireVitreous,
                },
                new()
                {
                    Dungeon = "Turtle Rock",
                    Abbreviation = "TR",
                    Type = typeof(TurtleRock),
                    LocationId = LocationId.TurtleRockTrinexx,
                },
                new()
                {
                    Dungeon = "Ganon's Tower",
                    Abbreviation = "GT",
                    Type = typeof(GanonsTower),
                },
                new()
                {
                    Dungeon = "Hyrule Castle",
                    Abbreviation = "HC",
                    Type = typeof(HyruleCastle),
                },
                new()
                {
                    Dungeon = "Castle Tower",
                    Abbreviation = "AT",
                    Type = typeof(CastleTower),
                },
            };
        }

        public static object Example()
        {
            return new DungeonConfig()
            {
                new()
                {
                    Dungeon = "Palace of Darkness",
                    Name = new("Palace of Darkness", new Possibility("Dark Palace", 0.1)),
                    Boss = new ("Helmasaur King", new Possibility("The Helmasaur King", 0.1)),
                },
            };
        }
    }
}
