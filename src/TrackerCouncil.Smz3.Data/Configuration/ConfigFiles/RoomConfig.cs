using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Brinstar;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Crateria;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Maridia;
using TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Norfair;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda.LightWorld.DeathMountain;
using static TrackerCouncil.Smz3.Data.Configuration.ConfigTypes.SchrodingersString;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Config file for additional room information
/// </summary>
[Description("Config file for room names which contain multiple locations in them")]
public class RoomConfig : List<RoomInfo>, IMergeable<RoomInfo>, IConfigFile<RoomConfig>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RoomConfig()
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
            new()
            {
                Room = "Blue Brinstar Top",
                Type = typeof(BlueBrinstar.BlueBrinstarTopRoom),
            },
            new()
            {
                Room = "Mockball Hall Hidden Room",
                Type = typeof(GreenBrinstar.MockballHallHiddenRoom),
            },
            new()
            {
                Room = "Gauntlet Shaft",
                Type = typeof(WestCrateria.GauntletShaftRoom),
            },
            new()
            {
                Room = "Left Sand Pit",
                Type = typeof(InnerMaridia.LeftSandPitRoom),
            },
            new()
            {
                Room = "Watering Hole",
                Type = typeof(InnerMaridia.WateringHoleRoom),
            },
            new()
            {
                Room = "Bubble Mountain Hidden Hall",
                Type = typeof(UpperNorfairEast.BubbleMountainHiddenHallRoom),
            },
            new()
            {
                Room = "Castle Tower Dark Maze",
                Type = typeof(CastleTower.DarkMazeRoom),
            },
            new()
            {
                Room = "Castle Tower Foyer",
                Type = typeof(CastleTower.FoyerRoom),
            },
            new()
            {
                Room = "Mire Shed",
                Type = typeof(DarkWorldMire.MireShedRoom),
            },
            new()
            {
                Room = "Pyramid Fairy",
                Type = typeof(DarkWorldNorthEast.PyramidFairyChamber),
            },
            new()
            {
                Room = "Hype Cave",
                Type = typeof(DarkWorldSouth.HypeCaveRoom),
            },
            new()
            {
                Room = "Hookshot Cave",
                Type = typeof(DarkWorldDeathMountainEast.HookshotCaveRoom),
            },
            new()
            {
                Room = "Superbunny Cave",
                Type = typeof(DarkWorldDeathMountainEast.SuperbunnyCaveRoom),
            },
            new()
            {
                Room = "Spike Cave",
                Type = typeof(DarkWorldDeathMountainWest.SpikeCaveRoom),
            },
            new()
            {
                Room = "Ganon's Tower Big Key Room",
                Type = typeof(GanonsTower.BigKeyRoomRoom),
            },
            new()
            {
                Room = "Ganon's Tower Compass Room",
                Type = typeof(GanonsTower.CompassRoomRoom),
            },
            new()
            {
                Room = "DM's Room",
                Type = typeof(GanonsTower.DMsRoomRoom),
            },
            new()
            {
                Room = "Ganon's Tower Hope Room",
                Type = typeof(GanonsTower.HopeRoomRoom),
            },
            new()
            {
                Room = "Ganon's Tower Mini Helmasaur Room",
                Type = typeof(GanonsTower.MiniHelmasaurRoomRoom),
            },
            new()
            {
                Room = "Ganon's Tower Randomizer Room",
                Type = typeof(GanonsTower.RandomizerRoomRoom),
            },
            new()
            {
                Room = "Back of Escape",
                Type = typeof(HyruleCastle.BackOfEscapeRoom),
            },
            new()
            {
                Room = "Paradox Cave",
                Type = typeof(LightWorldDeathMountainEast.ParadoxCaveRoom),
            },
            new()
            {
                Room = "Sahasrahla's Hut",
                Type = typeof(LightWorldNorthEast.SahasrahlasHideoutRoom),
            },
            new()
            {
                Room = "Waterfall Fairy",
                Type = typeof(LightWorldNorthEast.WaterfallFairyChamber),
            },
            new()
            {
                Room = "Zora's Domain",
                Type = typeof(LightWorldNorthEast.ZorasDomainArea),
            },
            new()
            {
                Room = "Blind's Hideout",
                Type = typeof(LightWorldNorthWest.BlindsHideoutRoom),
            },
            new()
            {
                Room = "Kakariko Well",
                Type = typeof(LightWorldNorthWest.KakarikoWellArea),
            },
            new()
            {
                Room = "Mini Moldorm Cave",
                Type = typeof(LightWorldSouth.MiniMoldormCaveRoom),
            },
            new()
            {
                Room = "Swamp Ruins",
                Type = typeof(LightWorldSouth.SwampRuinsRoom),
            },
            new()
            {
                Room = "Palace of Darkness Dark Basement",
                Type = typeof(PalaceOfDarkness.DarkBasementRoom),
            },
            new()
            {
                Room = "Palace of Darkness Dark Maze",
                Type = typeof(PalaceOfDarkness.DarkMazeRoom),
            },
            new()
            {
                Room = "Swamp Palace Flooded Room",
                Type = typeof(SwampPalace.FloodedRoomRoom),
            },
            new()
            {
                Room = "Eye Bridge",
                Type = typeof(TurtleRock.LaserBridgeRoom),
            },
            new()
            {
                Room = "Turtle Rock Roller Room",
                Type = typeof(TurtleRock.RollerRoomRoom),
            },
        };
    }

    public static object Example()
    {
        return new RoomConfig()
        {
            new()
            {
                Room = "Pyramid Fairy",
                Name = new("Pyramid Fairy", new Possibility("Cursed Fairy", 0.1)),
                Hints = new("Hint for when asking for where an item is, and it's in this room"),
                OutOfLogic = new("Tracker line with getting an item out of logic within this room")
            }
        };
    }
}
