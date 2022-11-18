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
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Brinstar.BlueBrinstar),
                    Name = new("Blue Brinstar"),
                    Hints = new("Samus might have been there before."),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Green Brinstar",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Brinstar.GreenBrinstar),
                    Name = new("Green Brinstar"),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Kraid's Lair",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Brinstar.KraidsLair),
                    Name = new("Kraid's Lair"),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Pink Brinstar",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Brinstar.PinkBrinstar),
                    Name = new("Pink Brinstar"),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Red Brinstar",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Brinstar.RedBrinstar),
                    Name = new("Red Brinstar"),
                    MapName = "Brinstar"
                },
                new RegionInfo()
                {
                    Region = "Central Crateria",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Crateria.CentralCrateria),
                    Name = new("Central Crateria"),
                    MapName = "Crateria"
                },
                new RegionInfo()
                {
                    Region = "East Crateria",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Crateria.EastCrateria),
                    Name = new("East Crateria"),
                    MapName = "Crateria"
                },
                new RegionInfo()
                {
                    Region = "West Crateria",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Crateria.WestCrateria),
                    Name = new("West Crateria"),
                    MapName = "Crateria"
                },
                new RegionInfo()
                {
                    Region = "Inner Maridia",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Maridia.InnerMaridia),
                    Name = new("Inner Maridia"),
                    Hints = new("You should go see Shaktool when you're in the area.", "It's in a wet place."),
                    MapName = "Maridia"
                },
                new RegionInfo()
                {
                    Region = "Outer Maridia",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Maridia.OuterMaridia),
                    Name = new("Outer Maridia"),
                    Hints = new("It's in a wet place."),
                    MapName = "Maridia"
                },
                new RegionInfo()
                {
                    Region = "Lower Norfair, East",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Norfair.LowerNorfairEast),
                    Name = new("Lower Norfair, East"),
                    Hints = new("I heard Ridley hangs out around there."),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Lower Norfair, West",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Norfair.LowerNorfairWest),
                    Name = new("Lower Norfair, West"),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Upper Norfair, Crocomire",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Norfair.UpperNorfairCrocomire),
                    Name = new("Upper Norfair, Crocomire"),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Upper Norfair, East",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Norfair.UpperNorfairEast),
                    Name = new("Upper Norfair, East"),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Upper Norfair, West",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Norfair.UpperNorfairWest),
                    Name = new("Upper Norfair, West"),
                    MapName = "Norfair"
                },
                new RegionInfo()
                {
                    Region = "Wrecked Ship",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.WreckedShip),
                    Name = new("Wrecked Ship"),
                    MapName = "Wrecked Ship"
                },
                new RegionInfo()
                {
                    Region = "Castle Tower",
                    Type = typeof(Data.WorldData.Regions.Zelda.CastleTower),
                    Name = new("Castle Tower", "Agahnim's Tower", "Hyrule Castle Tower"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Dark World Mire",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DarkWorldMire),
                    Name = new("Dark World Mire"),
                    Hints = new("It's in a wet place."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World North East",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DarkWorldNorthEast),
                    Name = new("Dark World North East"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World North West",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DarkWorldNorthWest),
                    Name = new("Dark World North West"),
                    Hints = new("Check around the villages."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World South",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DarkWorldSouth),
                    Name = new("Dark World South"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World Death Mountain East",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainEast),
                    Name = new("Dark World Death Mountain East"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Dark World Death Mountain West",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainWest),
                    Name = new("Dark World Death Mountain West"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Desert Palace",
                    Type = typeof(Data.WorldData.Regions.Zelda.DesertPalace),
                    Name = new("Desert Palace"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Eastern Palace",
                    Type = typeof(Data.WorldData.Regions.Zelda.EasternPalace),
                    Name = new("Eastern Palace"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Ganon's Tower",
                    Type = typeof(Data.WorldData.Regions.Zelda.GanonsTower),
                    Name = new("Ganon's Tower"),
                    Hints = new("I hope you don't need it."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Hyrule Castle",
                    Type = typeof(Data.WorldData.Regions.Zelda.HyruleCastle),
                    Name = new("Hyrule Castle"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Ice Palace",
                    Type = typeof(Data.WorldData.Regions.Zelda.IcePalace),
                    Name = new("Ice Palace"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Light World Death Mountain East",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.DeathMountain.LightWorldDeathMountainEast),
                    Name = new("Light World Death Mountain East"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Light World Death Mountain West",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.DeathMountain.LightWorldDeathMountainWest),
                    Name = new("Light World Death Mountain West"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Light World North East",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldNorthEast),
                    Name = new("Light World North East"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Light World North West",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldNorthWest),
                    Name = new("Light World North West"),
                    Hints = new("Check around the villages."),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Light World South",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldSouth),
                    Name = new("Light World South"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Misery Mire",
                    Type = typeof(Data.WorldData.Regions.Zelda.MiseryMire),
                    Name = new("Misery Mire"),
                    Hints = new("It's in a wet place.", "You need a medallion to get in there."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Palace of Darkness",
                    Type = typeof(Data.WorldData.Regions.Zelda.PalaceOfDarkness),
                    Name = new("Palace of Darkness"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Skull Woods",
                    Type = typeof(Data.WorldData.Regions.Zelda.SkullWoods),
                    Name = new("Skull Woods"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Swamp Palace",
                    Type = typeof(Data.WorldData.Regions.Zelda.SwampPalace),
                    Name = new("Swamp Palace"),
                    Hints = new("It's in a wet place."),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Thieves' Town",
                    Type = typeof(Data.WorldData.Regions.Zelda.ThievesTown),
                    Name = new("Thieves' Town"),
                    MapName = "Dark World"
                },
                new RegionInfo()
                {
                    Region = "Tower of Hera",
                    Type = typeof(Data.WorldData.Regions.Zelda.TowerOfHera),
                    Name = new("Tower of Hera"),
                    MapName = "Light World"
                },
                new RegionInfo()
                {
                    Region = "Turtle Rock",
                    Type = typeof(Data.WorldData.Regions.Zelda.TurtleRock),
                    Name = new("Turtle Rock", new("Tortoise Rock", 0.1)),
                    Hints = new("You need a medallion to get in there."),
                    MapName = "Dark World"
                },
            };
        }
    }
}
