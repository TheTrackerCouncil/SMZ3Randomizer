using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.Data.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional region information
    /// </summary>
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
                new RegionInfo()
                {
                    Region = "Blue Brinstar",
                    TypeName = "BlueBrinstar",
                    Name = new("Blue Brinstar"),
                    Hints = new("Samus might have been there before."),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Green Brinstar",
                    TypeName = "GreenBrinstar",
                    Name = new("Green Brinstar"),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Kraid's Lair",
                    TypeName = "KraidsLair",
                    Name = new("Kraid's Lair"),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Pink Brinstar",
                    TypeName = "PinkBrinstar",
                    Name = new("Pink Brinstar"),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Red Brinstar",
                    TypeName = "RedBrinstar",
                    Name = new("Red Brinstar"),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Central Crateria",
                    TypeName = "CentralCrateria",
                    Name = new("Central Crateria"),
                    MapName = "Crateria"
                },
                new RegionInfo()
                {
                    Region = "East Crateria",
                    TypeName = "EastCrateria",
                    Name = new("East Crateria"),
                    MapName = "Crateria"
                },
                new RegionInfo()
                {
                    Region = "West Crateria",
                    TypeName = "WestCrateria",
                    Name = new("West Crateria"),
                    MapName = "Crateria"
                },
                new RegionInfo()
                {
                    Region = "Inner Maridia",
                    TypeName = "InnerMaridia",
                    Name = new("Inner Maridia"),
                    Hints = new("You should go see Shaktool when you're in the area.", "It's in a wet place."),
                    MapName = "Maridia"
                },
                new RegionInfo()
                {
                    Region = "Outer Maridia",
                    TypeName = "OuterMaridia",
                    Name = new("Outer Maridia"),
                    Hints = new("It's in a wet place."),
                    MapName = "Maridia"
                },
                new RegionInfo()
                {
                    Region = "Lower Norfair, East",
                    TypeName = "LowerNorfairEast",
                    Name = new("Lower Norfair, East"),
                    Hints = new("I heard Ridley hangs out around there."),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Lower Norfair, West",
                    TypeName = "LowerNorfairWest",
                    Name = new("Lower Norfair, West"),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Upper Norfair, Crocomire",
                    TypeName = "UpperNorfairCrocomire",
                    Name = new("Upper Norfair, Crocomire"),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Upper Norfair, East",
                    TypeName = "UpperNorfairEast",
                    Name = new("Upper Norfair, East"),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Upper Norfair, West",
                    TypeName = "UpperNorfairWest",
                    Name = new("Upper Norfair, West"),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Wrecked Ship",
                    TypeName = "WreckedShip",
                    Name = new("Wrecked Ship"),
                    MapName = "Wrecked Ship"
                },
                new RegionInfo()
                {
                    Region = "Castle Tower",
                    TypeName = "CastleTower",
                    Name = new("Castle Tower", "Agahnim's Tower", "Hyrule Castle Tower"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Dark World Mire",
                    TypeName = "DarkWorldMire",
                    Name = new("Dark World Mire"),
                    Hints = new("It's in a wet place."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World North East",
                    TypeName = "DarkWorldNorthEast",
                    Name = new("Dark World North East"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World North West",
                    TypeName = "DarkWorldNorthWest",
                    Name = new("Dark World North West"),
                    Hints = new("Check around the villages."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World South",
                    TypeName = "DarkWorldSouth",
                    Name = new("Dark World South"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World Death Mountain East",
                    TypeName = "DarkWorldDeathMountainEast",
                    Name = new("Dark World Death Mountain East"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World Death Mountain West",
                    TypeName = "DarkWorldDeathMountainWest",
                    Name = new("Dark World Death Mountain West"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Desert Palace",
                    TypeName = "DesertPalace",
                    Name = new("Desert Palace"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Eastern Palace",
                    TypeName = "EasternPalace",
                    Name = new("Eastern Palace"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Ganon's Tower",
                    TypeName = "GanonsTower",
                    Name = new("Ganon's Tower"),
                    Hints = new("I hope you don't need it."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Hyrule Castle",
                    TypeName = "HyruleCastle",
                    Name = new("Hyrule Castle"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Ice Palace",
                    TypeName = "IcePalace",
                    Name = new("Ice Palace"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Light World Death Mountain East",
                    TypeName = "LightWorldDeathMountainEast",
                    Name = new("Light World Death Mountain East"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Light World Death Mountain West",
                    TypeName = "LightWorldDeathMountainWest",
                    Name = new("Light World Death Mountain West"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Light World North East",
                    TypeName = "LightWorldNorthEast",
                    Name = new("Light World North East"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Light World North West",
                    TypeName = "LightWorldNorthWest",
                    Name = new("Light World North West"),
                    Hints = new("Check around the villages."),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Light World South",
                    TypeName = "LightWorldSouth",
                    Name = new("Light World South"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Misery Mire",
                    TypeName = "MiseryMire",
                    Name = new("Misery Mire"),
                    Hints = new("It's in a wet place.", "You need a medallion to get in there."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Palace of Darkness",
                    TypeName = "PalaceOfDarkness",
                    Name = new("Palace of Darkness"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Skull Woods",
                    TypeName = "SkullWoods",
                    Name = new("Skull Woods"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Swamp Palace",
                    TypeName = "SwampPalace",
                    Name = new("Swamp Palace"),
                    Hints = new("It's in a wet place."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Thieves' Town",
                    TypeName = "ThievesTown",
                    Name = new("Thieves' Town"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Tower of Hera",
                    TypeName = "TowerOfHera",
                    Name = new("Tower of Hera"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Turtle Rock",
                    TypeName = "TurtleRock",
                    Name = new("Turtle Rock", new("Tortoise Rock", 0.1)),
                    Hints = new("You need a medallion to get in there."),
                    MapName = "Dark World"
                },
            };
        }
    }
}
