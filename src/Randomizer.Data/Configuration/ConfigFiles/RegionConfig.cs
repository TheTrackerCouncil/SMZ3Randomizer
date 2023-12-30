using System.Collections.Generic;
using System.ComponentModel;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData.Regions.SuperMetroid;
using Randomizer.Data.WorldData.Regions.SuperMetroid.Brinstar;
using Randomizer.Data.WorldData.Regions.SuperMetroid.Crateria;
using Randomizer.Data.WorldData.Regions.SuperMetroid.Maridia;
using Randomizer.Data.WorldData.Regions.SuperMetroid.Norfair;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.Data.WorldData.Regions.Zelda.DarkWorld;
using Randomizer.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain;
using Randomizer.Data.WorldData.Regions.Zelda.LightWorld;
using Randomizer.Data.WorldData.Regions.Zelda.LightWorld.DeathMountain;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional region information
    /// </summary>
    [Description("Config file for region names and various tracker responses when dying in particular locations")]
    public class RegionConfig : List<RegionInfo>, IMergeable<RegionInfo>, IConfigFile<RegionConfig>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RegionConfig() : base()
        {
        }

        /// <summary>
        /// Returns default region information
        /// </summary>
        /// <returns></returns>
        public static RegionConfig Default()
        {
            return new RegionConfig
            {
                new()
                {
                    Region = "Blue Brinstar",
                    Type = typeof(BlueBrinstar),
                    MapName = "Brinstar"
                },
                new()
                {
                    Region = "Green Brinstar",
                    Type = typeof(GreenBrinstar),
                    MapName = "Brinstar"
                },
                new()
                {
                    Region = "Kraid's Lair",
                    Type = typeof(KraidsLair),
                    MapName = "Brinstar"
                },
                new()
                {
                    Region = "Pink Brinstar",
                    Type = typeof(PinkBrinstar),
                    MapName = "Brinstar"
                },
                new()
                {
                    Region = "Red Brinstar",
                    Type = typeof(RedBrinstar),
                    MapName = "Brinstar"
                },
                new()
                {
                    Region = "Central Crateria",
                    Type = typeof(CentralCrateria),
                    MapName = "Crateria"
                },
                new()
                {
                    Region = "East Crateria",
                    Type = typeof(EastCrateria),
                    MapName = "Crateria"
                },
                new()
                {
                    Region = "West Crateria",
                    Type = typeof(WestCrateria),
                    MapName = "Crateria"
                },
                new()
                {
                    Region = "Inner Maridia",
                    Type = typeof(InnerMaridia),
                    MapName = "Maridia"
                },
                new()
                {
                    Region = "Outer Maridia",
                    Type = typeof(OuterMaridia),
                    MapName = "Maridia"
                },
                new()
                {
                    Region = "Lower Norfair, East",
                    Type = typeof(LowerNorfairEast),
                    MapName = "Norfair"
                },
                new()
                {
                    Region = "Lower Norfair, West",
                    Type = typeof(LowerNorfairWest),
                    MapName = "Norfair"
                },
                new()
                {
                    Region = "Upper Norfair, Crocomire",
                    Type = typeof(UpperNorfairCrocomire),
                    MapName = "Norfair"
                },
                new()
                {
                    Region = "Upper Norfair, East",
                    Type = typeof(UpperNorfairEast),
                    MapName = "Norfair"
                },
                new()
                {
                    Region = "Upper Norfair, West",
                    Type = typeof(UpperNorfairWest),
                    MapName = "Norfair"
                },
                new()
                {
                    Region = "Wrecked Ship",
                    Type = typeof(WreckedShip),
                    MapName = "Wrecked Ship"
                },
                new()
                {
                    Region = "Castle Tower",
                    Type = typeof(CastleTower),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Dark World Mire",
                    Type = typeof(DarkWorldMire),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Dark World North East",
                    Type = typeof(DarkWorldNorthEast),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Dark World North West",
                    Type = typeof(DarkWorldNorthWest),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Dark World South",
                    Type = typeof(DarkWorldSouth),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Dark World Death Mountain East",
                    Type = typeof(DarkWorldDeathMountainEast),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Dark World Death Mountain West",
                    Type = typeof(DarkWorldDeathMountainWest),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Desert Palace",
                    Type = typeof(DesertPalace),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Eastern Palace",
                    Type = typeof(EasternPalace),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Ganon's Tower",
                    Type = typeof(GanonsTower),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Hyrule Castle",
                    Type = typeof(HyruleCastle),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Ice Palace",
                    Type = typeof(IcePalace),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Light World Death Mountain East",
                    Type = typeof(LightWorldDeathMountainEast),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Light World Death Mountain West",
                    Type = typeof(LightWorldDeathMountainWest),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Light World North East",
                    Type = typeof(LightWorldNorthEast),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Light World North West",
                    Type = typeof(LightWorldNorthWest),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Light World South",
                    Type = typeof(LightWorldSouth),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Misery Mire",
                    Type = typeof(MiseryMire),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Palace of Darkness",
                    Type = typeof(PalaceOfDarkness),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Skull Woods",
                    Type = typeof(SkullWoods),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Swamp Palace",
                    Type = typeof(SwampPalace),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Thieves' Town",
                    Type = typeof(ThievesTown),
                    MapName = "Dark World"
                },
                new()
                {
                    Region = "Tower of Hera",
                    Type = typeof(TowerOfHera),
                    MapName = "Light World"
                },
                new()
                {
                    Region = "Turtle Rock",
                    Type = typeof(TurtleRock),
                    MapName = "Dark World"
                },
            };
        }

        public static object Example()
        {
            return new RegionConfig()
            {
                new()
                {
                    Region = "Turtle Rock",
                    Name = new("Turtle Rock", new("Tortoise Rock", 0.1)),
                    Hints = new("A hint tracker will give when asking for where an item is"),
                    WhenDiedInRoom = new Dictionary<int, SchrodingersString>()
                    {
                        { 15 , new SchrodingersString("Message when dying in room/screen 15 of the region ")}
                    },
                    OutOfLogic = new SchrodingersString("Message tracker will state when getting items out of logic in this region")
                }
            };
        }
    }
}
