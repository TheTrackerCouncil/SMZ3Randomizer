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
                    TypeName = "BlueBrinstarTopRoom",
                    Name = new("Blue Brinstar Top", "Billy Mays Room"),
                },
                new RoomInfo()
                {
                    Room = "Mockball Hall Hidden Room",
                    TypeName = "MockballHallHiddenRoom",
                    Name = new("Mockball Hall Hidden Room"),
                },
                new RoomInfo()
                {
                    Room = "Gauntlet Shaft",
                    TypeName = "GauntletShaftRoom",
                    Name = new("Gauntlet Shaft"),
                },
                new RoomInfo()
                {
                    Room = "Left Sand Pit",
                    TypeName = "LeftSandPitRoom",
                    Name = new("Left Sand Pit"),
                },
                new RoomInfo()
                {
                    Room = "Watering Hole",
                    TypeName = "WateringHoleRoom",
                    Name = new("Watering Hole"),
                },
                new RoomInfo()
                {
                    Room = "Bubble Mountain Hidden Hall",
                    TypeName = "BubbleMountainHiddenHallRoom",
                    Name = new("Bubble Mountain Hidden Hall"),
                },
                new RoomInfo()
                {
                    Room = "Castle Tower Dark Maze",
                    TypeName = "DarkMazeRoom",
                    Name = new("Castle Tower Dark Maze"),
                },
                new RoomInfo()
                {
                    Room = "Castle Tower Foyer",
                    TypeName = "FoyerRoom",
                    Name = new("Castle Tower Foyer"),
                },
                new RoomInfo()
                {
                    Room = "Mire Shed",
                    TypeName = "MireShedRoom",
                    Name = new("Mire Shed"),
                },
                new RoomInfo()
                {
                    Room = "Pyramid Fairy",
                    TypeName = "PyramidFairyChamber",
                    Name = new("Pyramid Fairy", "Cursed Fairy"),
                },
                new RoomInfo()
                {
                    Room = "Hype Cave",
                    TypeName = "HypeCaveRoom",
                    Name = new("Hype Cave"),
                },
                new RoomInfo()
                {
                    Room = "Hookshot Cave",
                    TypeName = "HookshotCaveRoom",
                    Name = new("Hookshot Cave"),
                },
                new RoomInfo()
                {
                    Room = "Superbunny Cave",
                    TypeName = "SuperbunnyCaveRoom",
                    Name = new("Superbunny Cave"),
                },
                new RoomInfo()
                {
                    Room = "Spike Cave",
                    TypeName = "SpikeCaveRoom",
                    Name = new("Spike Cave"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Big Key Room",
                    TypeName = "BigKeyRoomRoom",
                    Name = new("Ganon's Tower Big Key Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Compass Room",
                    TypeName = "CompassRoomRoom",
                    Name = new("Ganon's Tower Compass Room"),
                },
                new RoomInfo()
                {
                    Room = "DM's Room",
                    TypeName = "DMsRoomRoom",
                    Name = new("DM's Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Hope Room",
                    TypeName = "HopeRoomRoom",
                    Name = new("Ganon's Tower Hope Room", "Ganon's Tower Right Side First Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Mini Helmasaur Room",
                    TypeName = "MiniHelmasaurRoomRoom",
                    Name = new("Ganon's Tower Mini Helmasaur Room"),
                },
                new RoomInfo()
                {
                    Room = "Ganon's Tower Randomizer Room",
                    TypeName = "RandomizerRoomRoom",
                    Name = new("Ganon's Tower Randomizer Room"),
                },
                new RoomInfo()
                {
                    Room = "Back of Escape",
                    TypeName = "BackOfEscapeRoom",
                    Name = new("Back of Escape", "Sewers"),
                },
                new RoomInfo()
                {
                    Room = "Paradox Cave",
                    TypeName = "ParadoxCaveRoom",
                    Name = new("Paradox Cave"),
                },
                new RoomInfo()
                {
                    Room = "Sahasrahla's Hut",
                    TypeName = "SahasrahlasHideoutRoom",
                    Name = new("Sahasrahla's Hut"),
                },
                new RoomInfo()
                {
                    Room = "Waterfall Fairy",
                    TypeName = "WaterfallFairyChamber",
                    Name = new("Waterfall Fairy"),
                },
                new RoomInfo()
                {
                    Room = "Zora's Domain",
                    TypeName = "ZorasDomainArea",
                    Name = new("Zora's Domain"),
                },
                new RoomInfo()
                {
                    Room = "Blind's Hideout",
                    TypeName = "BlindsHideoutRoom",
                    Name = new("Blind's Hideout"),
                },
                new RoomInfo()
                {
                    Room = "Kakariko Well",
                    TypeName = "KakarikoWellArea",
                    Name = new("Kakariko Well"),
                },
                new RoomInfo()
                {
                    Room = "Mini Moldorm Cave",
                    TypeName = "MiniMoldormCaveRoom",
                    Name = new("Mini Moldorm Cave"),
                },
                new RoomInfo()
                {
                    Room = "Swamp Ruins",
                    TypeName = "SwampRuinsRoom",
                    Name = new("Swamp Ruins"),
                },
                new RoomInfo()
                {
                    Room = "Palace of Darkness Dark Basement",
                    TypeName = "DarkBasementRoom",
                    Name = new("Palace of Darkness Dark Basement"),
                },
                new RoomInfo()
                {
                    Room = "Palace of Darkness Dark Maze",
                    TypeName = "DarkMazeRoom",
                    Name = new("Palace of Darkness Dark Maze"),
                },
                new RoomInfo()
                {
                    Room = "Swamp Palace Flooded Room",
                    TypeName = "FloodedRoomRoom",
                    Name = new("Swamp Palace Flooded Room"),
                },
                new RoomInfo()
                {
                    Room = "Eye Bridge",
                    TypeName = "LaserBridgeRoom",
                    Name = new("Eye Bridge", "Laser Bridge"),
                },
                new RoomInfo()
                {
                    Room = "Turtle Rock Roller Room",
                    TypeName = "RollerRoomRoom",
                    Name = new("Turtle Rock Roller Room"),
                },
            };
        }
    }
}
