using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.Tracking.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional room information
    /// </summary>
    public class RoomConfig : List<RoomInfo>, IMergeable<RoomInfo>, IConfigFile<RoomConfig>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RoomConfig() : base()
        {
        }

        /// <summary>
        /// Returns default room information
        /// </summary>
        /// <returns></returns>
        public static RoomConfig Default()
        {
            return new RoomConfig
            {
                new RoomInfo()
                {
                    Room = "Blue Brinstar Top",
                    Type = typeof(Regions.SuperMetroid.Brinstar.BlueBrinstar.BlueBrinstarTopRoom),
                    Name = new("Blue Brinstar Top", "Billy Mays Room"),
                    X = 872,
                    Y = 340,
                },
                new RoomInfo()
                {
                    Room = "Mockball Hall Hidden Room",
                    Type = typeof(Regions.SuperMetroid.Brinstar.GreenBrinstar.MockballHallHiddenRoom),
                    Name = new("Mockball Hall Hidden Room"),
                    X = 392,
                    Y = 212,
                },
                new RoomInfo()
                {
                    Room = "Gauntlet Shaft",
                    Type = typeof(Regions.SuperMetroid.Crateria.WestCrateria.GauntletShaftRoom),
                    Name = new("Gauntlet Shaft"),
                    X = 264,
                    Y = 180,
                },
                new RoomInfo()
                {
                    Room = "Left Sand Pit",
                    Type = typeof(Regions.SuperMetroid.Maridia.InnerMaridia.LeftSandPitRoom),
                    Name = new("Left Sand Pit"),
                    X = 552,
                    Y = 532,
                },
                new RoomInfo()
                {
                    Room = "Watering Hole",
                    Type = typeof(Regions.SuperMetroid.Maridia.InnerMaridia.WateringHoleRoom),
                    Name = new("Watering Hole"),
                    X = 296,
                    Y = 276,
                },
                new RoomInfo()
                {
                    Room = "Bubble Mountain Hidden Hall",
                    Type = typeof(Regions.SuperMetroid.Norfair.UpperNorfairEast.BubbleMountainHiddenHallRoom),
                    Name = new("Bubble Mountain Hidden Hall"),
                    X = 632,
                    Y = 148,
                },
                new RoomInfo()
                {
                    Room = "Castle Tower Dark Maze",
                    Type = typeof(Regions.Zelda.CastleTower.DarkMazeRoom),
                    Name = new("Castle Tower Dark Maze"),
                },
                new RoomInfo()
                {
                    Room = "Castle Tower Foyer",
                    Type = typeof(Regions.Zelda.CastleTower.FoyerRoom),
                    Name = new("Castle Tower Foyer"),
                },
                new RoomInfo()
                {
                    Room = "Mire Shed",
                    Type = typeof(Regions.Zelda.DarkWorld.DarkWorldMire.MireShedRoom),
                    Name = new("Mire Shed"),
                    X = 77,
                    Y = 1600,
                },
                new RoomInfo()
                {
                    Room = "Pyramid Fairy",
                    Type = typeof(Regions.Zelda.DarkWorld.DarkWorldNorthEast.PyramidFairyChamber),
                    Name = new("Pyramid Fairy", "Cursed Fairy"),
                    X = 940,
                    Y = 976,
                },
                new RoomInfo()
                {
                    Room = "Hype Cave",
                    Type = typeof(Regions.Zelda.DarkWorld.DarkWorldSouth.HypeCaveRoom),
                    Name = new("Hype Cave"),
                    X = 1200,
                    Y = 1560,
                },
                new RoomInfo()
                {
                    Room = "Hookshot Cave",
                    Type = typeof(Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainEast.HookshotCaveRoom),
                    Name = new("Hookshot Cave"),
                    X = 1670,
                    Y = 126,
                },
                new RoomInfo()
                {
                    Room = "Superbunny Cave",
                    Type = typeof(Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainEast.SuperbunnyCaveRoom),
                    Name = new("Superbunny Cave"),
                    X = 1695,
                    Y = 290,
                },
                new RoomInfo()
                {
                    Room = "Spike Cave",
                    Type = typeof(Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainWest.SpikeCaveRoom),
                    Name = new("Spike Cave"),
                    X = 1151,
                    Y = 294,
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Big Key Room",
                    Type = typeof(Regions.Zelda.GanonsTower.BigKeyRoomRoom),
                    Name = new("Ganon's Tower Big Key Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Compass Room",
                    Type = typeof(Regions.Zelda.GanonsTower.CompassRoomRoom),
                    Name = new("Ganon's Tower Compass Room"),
                },
                new RoomInfo()
                {
                    Room = "DM's Room",
                    Type = typeof(Regions.Zelda.GanonsTower.DMsRoomRoom),
                    Name = new("DM's Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Hope Room",
                    Type = typeof(Regions.Zelda.GanonsTower.HopeRoomRoom),
                    Name = new("Ganon's Tower Hope Room", "Ganon's Tower Right Side First Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Mini Helmasaur Room",
                    Type = typeof(Regions.Zelda.GanonsTower.MiniHelmasaurRoomRoom),
                    Name = new("Ganon's Tower Mini Helmasaur Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Randomizer Room",
                    Type = typeof(Regions.Zelda.GanonsTower.RandomizerRoomRoom),
                    Name = new("Ganon's Tower Randomizer Room"),
                },
                new RoomInfo()
                {
                    Room = "Back of Escape",
                    Type = typeof(Regions.Zelda.HyruleCastle.BackOfEscapeRoom),
                    Name = new("Back of Escape", "Sewers"),
                },
                new RoomInfo()
                {
                    Room = "Paradox Cave",
                    Type = typeof(Regions.Zelda.LightWorld.DeathMountain.LightWorldDeathMountainEast.ParadoxCaveRoom),
                    Name = new("Paradox Cave"),
                    X = 1731,
                    Y = 434,
                },
                new RoomInfo()
                {
                    Room = "Sahasrahla's Hut",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldNorthEast.SahasrahlasHideoutRoom),
                    Name = new("Sahasrahla's Hut"),
                    X = 1630,
                    Y = 900,
                },
                new RoomInfo()
                {
                    Room = "Waterfall Fairy",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldNorthEast.WaterfallFairyChamber),
                    Name = new("Waterfall Fairy"),
                    X = 1806,
                    Y = 286,
                },
                new RoomInfo()
                {
                    Room = "Zora's Domain",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldNorthEast.ZorasDomainArea),
                    Name = new("Zora's Domain"),
                    X = 1920,
                    Y = 273,
                },
                new RoomInfo()
                {
                    Room = "Blind's Hideout",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldNorthWest.BlindsHideoutRoom),
                    Name = new("Blind's Hideout"),
                    X = 307,
                    Y = 840,
                },
                new RoomInfo()
                {
                    Room = "Kakariko Well",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldNorthWest.KakarikoWellArea),
                    Name = new("Kakariko Well"),
                    X = 47,
                    Y = 833,
                },
                new RoomInfo()
                {
                    Room = "Mini Moldorm Cave",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldSouth.MiniMoldormCaveRoom),
                    Name = new("Mini Moldorm Cave"),
                    X = 1309,
                    Y = 1887,
                },
                new RoomInfo()
                {
                    Room = "Swamp Ruins",
                    Type = typeof(Regions.Zelda.LightWorld.LightWorldSouth.SwampRuinsRoom),
                    Name = new("Swamp Ruins"),
                    X = 944,
                    Y = 1880,
                },
                new RoomInfo()
                {
                    Room = "Palace of Darkness Dark Basement",
                    Type = typeof(Regions.Zelda.PalaceOfDarkness.DarkBasementRoom),
                    Name = new("Palace of Darkness Dark Basement"),
                },
                new RoomInfo()
                {
                    Room = "Palace of Darkness Dark Maze",
                    Type = typeof(Regions.Zelda.PalaceOfDarkness.DarkMazeRoom),
                    Name = new("Palace of Darkness Dark Maze"),
                },
                new RoomInfo()
                {
                    Room = "Swamp Palace Flooded Room",
                    Type = typeof(Regions.Zelda.SwampPalace.FloodedRoomRoom),
                    Name = new("Swamp Palace Flooded Room"),
                },
                new RoomInfo()
                {
                    Room = "Eye Bridge",
                    Type = typeof(Regions.Zelda.TurtleRock.LaserBridgeRoom),
                    Name = new("Eye Bridge", "Laser Bridge"),
                },
                new RoomInfo()
                {
                    Room = "Turtle Rock Roller Room",
                    Type = typeof(Regions.Zelda.TurtleRock.RollerRoomRoom),
                    Name = new("Turtle Rock Roller Room"),
                },
            };
        }
    }
}
