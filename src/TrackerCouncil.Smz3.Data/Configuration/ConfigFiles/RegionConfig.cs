using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Brinstar;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Crateria;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Maridia;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Norfair;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld.DeathMountain;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

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
                Type = typeof(BlueBrinstar)
            },
            new()
            {
                Region = "Green Brinstar",
                Type = typeof(GreenBrinstar),
            },
            new()
            {
                Region = "Kraid's Lair",
                Type = typeof(KraidsLair),
            },
            new()
            {
                Region = "Pink Brinstar",
                Type = typeof(PinkBrinstar),
            },
            new()
            {
                Region = "Red Brinstar",
                Type = typeof(RedBrinstar),
            },
            new()
            {
                Region = "Central Crateria",
                Type = typeof(CentralCrateria),
            },
            new()
            {
                Region = "East Crateria",
                Type = typeof(EastCrateria),
            },
            new()
            {
                Region = "West Crateria",
                Type = typeof(WestCrateria),
            },
            new()
            {
                Region = "Inner Maridia",
                Type = typeof(InnerMaridia),
            },
            new()
            {
                Region = "Outer Maridia",
                Type = typeof(OuterMaridia),
            },
            new()
            {
                Region = "Lower Norfair, East",
                Type = typeof(LowerNorfairEast),
            },
            new()
            {
                Region = "Lower Norfair, West",
                Type = typeof(LowerNorfairWest),
            },
            new()
            {
                Region = "Upper Norfair, Crocomire",
                Type = typeof(UpperNorfairCrocomire),
            },
            new()
            {
                Region = "Upper Norfair, East",
                Type = typeof(UpperNorfairEast),
            },
            new()
            {
                Region = "Upper Norfair, West",
                Type = typeof(UpperNorfairWest),
            },
            new()
            {
                Region = "Wrecked Ship",
                Type = typeof(WreckedShip),
            },
            new()
            {
                Region = "Castle Tower",
                Type = typeof(CastleTower),
            },
            new()
            {
                Region = "Dark World Mire",
                Type = typeof(DarkWorldMire),
            },
            new()
            {
                Region = "Dark World North East",
                Type = typeof(DarkWorldNorthEast),
            },
            new()
            {
                Region = "Dark World North West",
                Type = typeof(DarkWorldNorthWest),
            },
            new()
            {
                Region = "Dark World South",
                Type = typeof(DarkWorldSouth),
            },
            new()
            {
                Region = "Dark World Death Mountain East",
                Type = typeof(DarkWorldDeathMountainEast),
            },
            new()
            {
                Region = "Dark World Death Mountain West",
                Type = typeof(DarkWorldDeathMountainWest),
            },
            new()
            {
                Region = "Desert Palace",
                Type = typeof(DesertPalace),
            },
            new()
            {
                Region = "Eastern Palace",
                Type = typeof(EasternPalace),
            },
            new()
            {
                Region = "Ganon's Tower",
                Type = typeof(GanonsTower),
            },
            new()
            {
                Region = "Hyrule Castle",
                Type = typeof(HyruleCastle),
            },
            new()
            {
                Region = "Ice Palace",
                Type = typeof(IcePalace),
            },
            new()
            {
                Region = "Light World Death Mountain East",
                Type = typeof(LightWorldDeathMountainEast),
            },
            new()
            {
                Region = "Light World Death Mountain West",
                Type = typeof(LightWorldDeathMountainWest),
            },
            new()
            {
                Region = "Light World North East",
                Type = typeof(LightWorldNorthEast),
            },
            new()
            {
                Region = "Light World North West",
                Type = typeof(LightWorldNorthWest),
            },
            new()
            {
                Region = "Light World South",
                Type = typeof(LightWorldSouth),
            },
            new()
            {
                Region = "Misery Mire",
                Type = typeof(MiseryMire),
            },
            new()
            {
                Region = "Palace of Darkness",
                Type = typeof(PalaceOfDarkness),
            },
            new()
            {
                Region = "Skull Woods",
                Type = typeof(SkullWoods),
            },
            new()
            {
                Region = "Swamp Palace",
                Type = typeof(SwampPalace),
            },
            new()
            {
                Region = "Thieves' Town",
                Type = typeof(ThievesTown),
            },
            new()
            {
                Region = "Tower of Hera",
                Type = typeof(TowerOfHera),
            },
            new()
            {
                Region = "Turtle Rock",
                Type = typeof(TurtleRock),
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
