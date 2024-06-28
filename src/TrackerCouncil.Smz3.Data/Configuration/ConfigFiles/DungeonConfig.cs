using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using static TrackerCouncil.Smz3.Data.Configuration.ConfigTypes.SchrodingersString;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

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
                Type = typeof(EasternPalace),
            },
            new()
            {
                Dungeon = "Desert Palace",
                Type = typeof(DesertPalace),
            },
            new()
            {
                Dungeon = "Tower of Hera",
                Type = typeof(TowerOfHera),
            },
            new()
            {
                Dungeon = "Palace of Darkness",
                Type = typeof(PalaceOfDarkness),
            },
            new()
            {
                Dungeon = "Swamp Palace",
                Type = typeof(SwampPalace),
            },
            new()
            {
                Dungeon = "Skull Woods",
                Type = typeof(SkullWoods),
            },
            new()
            {
                Dungeon = "Thieves' Town",
                Type = typeof(ThievesTown),
            },
            new()
            {
                Dungeon = "Ice Palace",
                Type = typeof(IcePalace),
            },
            new()
            {
                Dungeon = "Misery Mire",
                Type = typeof(MiseryMire),
            },
            new()
            {
                Dungeon = "Turtle Rock",
                Type = typeof(TurtleRock),
            },
            new()
            {
                Dungeon = "Ganon's Tower",
                Type = typeof(GanonsTower),
            },
            new()
            {
                Dungeon = "Hyrule Castle",
                Type = typeof(HyruleCastle),
            },
            new()
            {
                Dungeon = "Castle Tower",
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
