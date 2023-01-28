using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.Data.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
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
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Brinstar.BlueBrinstar.BlueBrinstarTopRoom),
                    Name = new("Blue Brinstar Top", "Billy Mays Room"),
                },
                new RoomInfo()
                {
                    Room = "Mockball Hall Hidden Room",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Brinstar.GreenBrinstar.MockballHallHiddenRoom),
                    Name = new("Mockball Hall Hidden Room"),
                },
                new RoomInfo()
                {
                    Room = "Gauntlet Shaft",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Crateria.WestCrateria.GauntletShaftRoom),
                    Name = new("Gauntlet Shaft"),
                },
                new RoomInfo()
                {
                    Room = "Left Sand Pit",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Maridia.InnerMaridia.LeftSandPitRoom),
                    Name = new("Left Sand Pit"),
                },
                new RoomInfo()
                {
                    Room = "Watering Hole",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Maridia.InnerMaridia.WateringHoleRoom),
                    Name = new("Watering Hole"),
                },
                new RoomInfo()
                {
                    Room = "Bubble Mountain Hidden Hall",
                    Type = typeof(Data.WorldData.Regions.SuperMetroid.Norfair.UpperNorfairEast.BubbleMountainHiddenHallRoom),
                    Name = new("Bubble Mountain Hidden Hall"),
                },
                new RoomInfo()
                {
                    Room = "Castle Tower Dark Maze",
                    Type = typeof(Data.WorldData.Regions.Zelda.CastleTower.DarkMazeRoom),
                    Name = new("Castle Tower Dark Maze"),
                },
                new RoomInfo()
                {
                    Room = "Castle Tower Foyer",
                    Type = typeof(Data.WorldData.Regions.Zelda.CastleTower.FoyerRoom),
                    Name = new("Castle Tower Foyer"),
                },
                new RoomInfo()
                {
                    Room = "Mire Shed",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DarkWorldMire.MireShedRoom),
                    Name = new("Mire Shed"),
                },
                new RoomInfo()
                {
                    Room = "Pyramid Fairy",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DarkWorldNorthEast.PyramidFairyChamber),
                    Name = new("Pyramid Fairy", "Cursed Fairy"),
                },
                new RoomInfo()
                {
                    Room = "Hype Cave",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DarkWorldSouth.HypeCaveRoom),
                    Name = new("Hype Cave"),
                },
                new RoomInfo()
                {
                    Room = "Hookshot Cave",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainEast.HookshotCaveRoom),
                    Name = new("Hookshot Cave"),
                },
                new RoomInfo()
                {
                    Room = "Superbunny Cave",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainEast.SuperbunnyCaveRoom),
                    Name = new("Superbunny Cave"),
                },
                new RoomInfo()
                {
                    Room = "Spike Cave",
                    Type = typeof(Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain.DarkWorldDeathMountainWest.SpikeCaveRoom),
                    Name = new("Spike Cave"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Big Key Room",
                    Type = typeof(Data.WorldData.Regions.Zelda.GanonsTower.BigKeyRoomRoom),
                    Name = new("Ganon's Tower Big Key Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Compass Room",
                    Type = typeof(Data.WorldData.Regions.Zelda.GanonsTower.CompassRoomRoom),
                    Name = new("Ganon's Tower Compass Room"),
                },
                new RoomInfo()
                {
                    Room = "DM's Room",
                    Type = typeof(Data.WorldData.Regions.Zelda.GanonsTower.DMsRoomRoom),
                    Name = new("DM's Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Hope Room",
                    Type = typeof(Data.WorldData.Regions.Zelda.GanonsTower.HopeRoomRoom),
                    Name = new("Ganon's Tower Hope Room", "Ganon's Tower Right Side First Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Mini Helmasaur Room",
                    Type = typeof(Data.WorldData.Regions.Zelda.GanonsTower.MiniHelmasaurRoomRoom),
                    Name = new("Ganon's Tower Mini Helmasaur Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Randomizer Room",
                    Type = typeof(Data.WorldData.Regions.Zelda.GanonsTower.RandomizerRoomRoom),
                    Name = new("Ganon's Tower Randomizer Room"),
                },
                new RoomInfo()
                {
                    Room = "Back of Escape",
                    Type = typeof(Data.WorldData.Regions.Zelda.HyruleCastle.BackOfEscapeRoom),
                    Name = new("Back of Escape", "Sewers"),
                },
                new RoomInfo()
                {
                    Room = "Paradox Cave",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.DeathMountain.LightWorldDeathMountainEast.ParadoxCaveRoom),
                    Name = new("Paradox Cave"),
                },
                new RoomInfo()
                {
                    Room = "Sahasrahla's Hut",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldNorthEast.SahasrahlasHideoutRoom),
                    Name = new("Sahasrahla's Hut"),
                },
                new RoomInfo()
                {
                    Room = "Waterfall Fairy",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldNorthEast.WaterfallFairyChamber),
                    Name = new("Waterfall Fairy"),
                },
                new RoomInfo()
                {
                    Room = "Zora's Domain",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldNorthEast.ZorasDomainArea),
                    Name = new("Zora's Domain"),
                },
                new RoomInfo()
                {
                    Room = "Blind's Hideout",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldNorthWest.BlindsHideoutRoom),
                    Name = new("Blind's Hideout"),
                },
                new RoomInfo()
                {
                    Room = "Kakariko Well",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldNorthWest.KakarikoWellArea),
                    Name = new("Kakariko Well"),
                },
                new RoomInfo()
                {
                    Room = "Mini Moldorm Cave",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldSouth.MiniMoldormCaveRoom),
                    Name = new("Mini Moldorm Cave"),
                },
                new RoomInfo()
                {
                    Room = "Swamp Ruins",
                    Type = typeof(Data.WorldData.Regions.Zelda.LightWorld.LightWorldSouth.SwampRuinsRoom),
                    Name = new("Swamp Ruins"),
                },
                new RoomInfo()
                {
                    Room = "Palace of Darkness Dark Basement",
                    Type = typeof(Data.WorldData.Regions.Zelda.PalaceOfDarkness.DarkBasementRoom),
                    Name = new("Palace of Darkness Dark Basement"),
                },
                new RoomInfo()
                {
                    Room = "Palace of Darkness Dark Maze",
                    Type = typeof(Data.WorldData.Regions.Zelda.PalaceOfDarkness.DarkMazeRoom),
                    Name = new("Palace of Darkness Dark Maze"),
                },
                new RoomInfo()
                {
                    Room = "Swamp Palace Flooded Room",
                    Type = typeof(Data.WorldData.Regions.Zelda.SwampPalace.FloodedRoomRoom),
                    Name = new("Swamp Palace Flooded Room"),
                },
                new RoomInfo()
                {
                    Room = "Eye Bridge",
                    Type = typeof(Data.WorldData.Regions.Zelda.TurtleRock.LaserBridgeRoom),
                    Name = new("Eye Bridge", "Laser Bridge"),
                },
                new RoomInfo()
                {
                    Room = "Turtle Rock Roller Room",
                    Type = typeof(Data.WorldData.Regions.Zelda.TurtleRock.RollerRoomRoom),
                    Name = new("Turtle Rock Roller Room"),
                },
            };
        }
    }
}
