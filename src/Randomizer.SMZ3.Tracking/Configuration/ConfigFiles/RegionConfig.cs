using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.Tracking.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigFiles
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
                    Type = typeof(Regions.SuperMetroid.Brinstar.BlueBrinstar),
                    Name = new("Blue Brinstar"),
                    Hints = new("Samus might have been there before."),
                },
                new RegionInfo()
                {
                    Region = "Green Brinstar",
                    Type = typeof(Regions.SuperMetroid.Brinstar.GreenBrinstar),
                    Name = new("Green Brinstar"),
                },
                new RegionInfo()
                {
                    Region = "Kraid's Lair",
                    Type = typeof(Regions.SuperMetroid.Brinstar.KraidsLair),
                    Name = new("Kraid's Lair"),
                },
                new RegionInfo()
                {
                    Region = "Pink Brinstar",
                    Type = typeof(Regions.SuperMetroid.Brinstar.PinkBrinstar),
                    Name = new("Pink Brinstar"),
                },
                new RegionInfo()
                {
                    Region = "Red Brinstar",
                    Type = typeof(Regions.SuperMetroid.Brinstar.RedBrinstar),
                    Name = new("Red Brinstar"),
                },
                new RegionInfo()
                {
                    Region = "Central Crateria",
                    Type = typeof(Regions.SuperMetroid.Crateria.CentralCrateria),
                    Name = new("Central Crateria"),
                },
                new RegionInfo()
                {
                    Region = "East Crateria",
                    Type = typeof(Regions.SuperMetroid.Crateria.EastCrateria),
                    Name = new("East Crateria"),
                },
                new RegionInfo()
                {
                    Region = "West Crateria",
                    Type = typeof(Regions.SuperMetroid.Crateria.WestCrateria),
                    Name = new("West Crateria"),
                },
                new RegionInfo()
                {
                    Region = "Inner Maridia",
                    Type = typeof(Regions.SuperMetroid.Maridia.InnerMaridia),
                    Name = new("Inner Maridia"),
                    Hints = new("You should go see Shaktool when you're in the area.", "It's in a wet place."),
                },
                new RegionInfo()
                {
                    Region = "Outer Maridia",
                    Type = typeof(Regions.SuperMetroid.Maridia.OuterMaridia),
                    Name = new("Outer Maridia"),
                    Hints = new("It's in a wet place."),
                },
                new RegionInfo()
                {
                    Region = "Lower Norfair, East",
                    Type = typeof(Regions.SuperMetroid.Norfair.LowerNorfairEast),
                    Name = new("Lower Norfair, East"),
                    Hints = new("I heard Ridley hangs out around there."),
                },
                new RegionInfo()
                {
                    Region = "Lower Norfair, West",
                    Type = typeof(Regions.SuperMetroid.Norfair.LowerNorfairWest),
                    Name = new("Lower Norfair, West"),
                },
                new RegionInfo()
                {
                    Region = "Upper Norfair, Crocomire",
                    Type = typeof(Regions.SuperMetroid.Norfair.UpperNorfairCrocomire),
                    Name = new("Upper Norfair, Crocomire"),
                },
                new RegionInfo()
                {
                    Region = "Upper Norfair, East",
                    Type = typeof(Regions.SuperMetroid.Norfair.UpperNorfairEast),
                    Name = new("Upper Norfair, East"),
                },
                new RegionInfo()
                {
                    Region = "Upper Norfair, West",
                    Type = typeof(Regions.SuperMetroid.Norfair.UpperNorfairWest),
                    Name = new("Upper Norfair, West"),
                },
                new RegionInfo()
                {
                    Region = "Wrecked Ship",
                    Type = typeof(Regions.SuperMetroid.WreckedShip),
                    Name = new("Wrecked Ship"),
                },
                new RegionInfo()
                {
                    Region = "Castle Tower",
                    Type = typeof(Regions.Zelda.CastleTower),
                    Name = new("Castle Tower", "Agahnim's Tower", "Hyrule Castle Tower"),
                },
                new RegionInfo()
                {
                    Region = "Dark World Mire",
                    Type = typeof(Regions.Zelda.DarkWorld.DarkWorldMire),
                    Name = new("Dark World Mire"),
                    Hints = new("It's in a wet place."),
                },
                new RegionInfo()
                {
                    Region = "Dark World North East",
                    Type = typeof(Regions.Zelda.DarkWorld.DarkWorldNorthEast),
                    Name = new("Dark World North East"),
                },
                new RegionInfo()
                {
                    Region = "Dark World North West",
                    Type = typeof(Regions.Zelda.DarkWorld.DarkWorldNorthWest),
                    Name = new("Dark World North West"),
                    Hints = new("Check around the villages."),
                },
                new RegionInfo()
                {
                    Region = "Dark World South",
                    Type = typeof(Regions.Zelda.DarkWorld.DarkWorldSouth),
                    Name = new("Dark World South"),
                },
                new RegionInfo()
                {
                    Region = "Dark World Death Mountain East",
                    Type = typeof(Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainEast),
                    Name = new("Dark World Death Mountain East"),
                },
                new RegionInfo()
                {
                    Region = "Dark World Death Mountain West",
                    Type = typeof(Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainWest),
                    Name = new("Dark World Death Mountain West"),
                },
                new RegionInfo()
                {
                    Region = "Desert Palace",
                    Type = typeof(Regions.Zelda.DesertPalace),
                    Name = new("Desert Palace"),
                },
                new RegionInfo()
                {
                    Region = "Eastern Palace",
                    Type = typeof(Regions.Zelda.EasternPalace),
                    Name = new("Eastern Palace"),
                },
                new RegionInfo()
                {
                    Region = "Ganon's Tower",
                    Type = typeof(Regions.Zelda.GanonsTower),
                    Name = new("Ganon's Tower"),
                    Hints = new("I hope you don't need it."),
                },
                new RegionInfo()
                {
                    Region = "Hyrule Castle",
                    Type = typeof(Regions.Zelda.HyruleCastle),
                    Name = new("Hyrule Castle"),
                },
                new RegionInfo()
                {
                    Region = "Ice Palace",
                    Type = typeof(Regions.Zelda.IcePalace),
                    Name = new("Ice Palace"),
                },
                new RegionInfo()
                {
                    Region = "Light World Death Mountain East",
                    Type = typeof(Regions.Zelda.LightWorld.DeathMountain.LightWorldDeathMountainEast),
                    Name = new("Light World Death Mountain East"),
                },
                new RegionInfo()
                {
                    Region = "Light World Death Mountain West",
                    Type = typeof(Regions.Zelda.LightWorld.DeathMountain.LightWorldDeathMountainWest),
                    Name = new("Light World Death Mountain West"),
                },
                new RegionInfo()
                {
                    Region = "Light World North East",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldNorthEast),
                    Name = new("Light World North East"),
                },
                new RegionInfo()
                {
                    Region = "Light World North West",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldNorthWest),
                    Name = new("Light World North West"),
                    Hints = new("Check around the villages."),
                },
                new RegionInfo()
                {
                    Region = "Light World South",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldSouth),
                    Name = new("Light World South"),
                },
                new RegionInfo()
                {
                    Region = "Misery Mire",
                    Type = typeof(Regions.Zelda.MiseryMire),
                    Name = new("Misery Mire"),
                    Hints = new("It's in a wet place.", "You need a medallion to get in there."),
                },
                new RegionInfo()
                {
                    Region = "Palace of Darkness",
                    Type = typeof(Regions.Zelda.PalaceOfDarkness),
                    Name = new("Palace of Darkness"),
                },
                new RegionInfo()
                {
                    Region = "Skull Woods",
                    Type = typeof(Regions.Zelda.SkullWoods),
                    Name = new("Skull Woods"),
                },
                new RegionInfo()
                {
                    Region = "Swamp Palace",
                    Type = typeof(Regions.Zelda.SwampPalace),
                    Name = new("Swamp Palace"),
                    Hints = new("It's in a wet place."),
                },
                new RegionInfo()
                {
                    Region = "Thieves' Town",
                    Type = typeof(Regions.Zelda.ThievesTown),
                    Name = new("Thieves' Town"),
                },
                new RegionInfo()
                {
                    Region = "Tower of Hera",
                    Type = typeof(Regions.Zelda.TowerOfHera),
                    Name = new("Tower of Hera"),
                },
                new RegionInfo()
                {
                    Region = "Turtle Rock",
                    Type = typeof(Regions.Zelda.TurtleRock),
                    Name = new("Turtle Rock", new("Tortoise Rock", 0.1)),
                    Hints = new("You need a medallion to get in there."),
                },
            };
        }
    }
}
