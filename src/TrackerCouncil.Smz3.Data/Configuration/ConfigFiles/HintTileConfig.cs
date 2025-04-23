using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Config file for the responses to viewing and clearing hint tiles
/// and the
/// </summary>
[Description("Config file for hint tiles location names and various tracker responses to viewing hint tiles")]
public class HintTileConfig : IMergeable<HintTileConfig>, IConfigFile<HintTileConfig>
{
    /// <summary>
    /// The list of hint tiles and their details
    /// </summary>
    public HintTileList? HintTiles { get; set; }

    /// <summary>
    /// The response for when asking for what a hint tile says
    /// <c>{0}</c> is a placeholder for the hint tile text
    /// </summary>
    public SchrodingersString? RequestedHintTile { get; set; }

    /// <summary>
    /// The response when trying to clear a hint tile despite not looking at a hint tile
    /// </summary>
    public SchrodingersString? NoPreviousHintTile { get; set; }

    /// <summary>
    /// The response when trying to clear an already cleared hint tile or one that has nothing to clear
    /// </summary>
    public SchrodingersString? ClearHintTileFailed { get; set; }

    /// <summary>
    /// The response for viewing a hint tile with multiple locations that are neither mandatory or useless
    /// </summary>
    public SchrodingersString? ViewedHintTile { get; set; }

    /// <summary>
    /// The response for viewing a hint tile for a place that's mandatory
    /// </summary>
    public SchrodingersString? ViewedHintTileMandatory { get; set; }

    /// <summary>
    /// The response for viewing a hint tile for a place that has a key or a keycard
    /// </summary>
    public SchrodingersString? ViewedHintTileKey { get; set; }

    /// <summary>
    /// The response for viewing a hint tile for a place that's useless
    /// </summary>
    public SchrodingersString? ViewedHintTileUseless { get; set; }

    /// <summary>
    /// The response for viewing a hint tile with multiple locations that have already been cleared
    /// </summary>
    public SchrodingersString? ViewedHintTileAlreadyVisited { get; set; }

    /// <summary>
    /// The response for asking about a hint tile when there aren't any generated hint tiles (plando or no hints)
    /// </summary>
    public SchrodingersString? NoHintTiles { get; set; }

    /// <summary>
    /// Returns default hint tile information
    /// </summary>
    /// <returns></returns>
    public static HintTileConfig Default()
    {
        return new HintTileConfig()
        {
            HintTiles = new HintTileList()
            {
                new()
                {
                    HintTileKey = "telepathic_tile_eastern_palace",
                    Room = 168,
                    TopLeftX = 4465,
                    TopLeftY = 5224,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_tower_of_hera_floor_4",
                    Room = 39,
                    TopLeftX = 3825,
                    TopLeftY = 1288,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_spectacle_rock",
                    Room = 234,
                    TopLeftX = 5289,
                    TopLeftY = 7328,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_swamp_entrance",
                    Room = 40,
                    TopLeftX = 4393,
                    TopLeftY = 1232,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_thieves_town_upstairs",
                    Room = 100,
                    TopLeftX = 2121,
                    TopLeftY = 3376,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_misery_mire",
                    Room = 151,
                    TopLeftX = 3881,
                    TopLeftY = 4776,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_palace_of_darkness",
                    Room = 75,
                    TopLeftX = 6001,
                    TopLeftY = 2096,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_desert_bonk_torch_room",
                    Room = 115,
                    TopLeftX = 1905,
                    TopLeftY = 3632,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_castle_tower",
                    Room = 176,
                    TopLeftX = 313,
                    TopLeftY = 5680,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_ice_large_room",
                    Room = 126,
                    TopLeftX = 7537,
                    TopLeftY = 3632,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_turtle_rock",
                    Room = 214,
                    TopLeftX = 3369,
                    TopLeftY = 7056,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_ice_entrance",
                    Room = 14,
                    TopLeftX = 7537,
                    TopLeftY = 304,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_ice_stalfos_knights_room",
                    Room = 62,
                    TopLeftX = 7537,
                    TopLeftY = 1584,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_tower_of_hera_entrance",
                    Room = 119,
                    TopLeftX = 3825,
                    TopLeftY = 3816,
                },
                new()
                {
                    HintTileKey = "telepathic_tile_south_east_darkworld_cave",
                    Room = 293,
                    TopLeftX = 2929,
                    TopLeftY = 9520,
                },
            }
        };
    }

    public static object Example()
    {
        return new Dictionary<string, object>()
        {
            {
                "HintTiles", new HintTileList()
                {
                    new()
                    {
                        HintTileKey = "telepathic_tile_south_east_darkworld_cave",
                        Name = new SchrodingersString("Dark World Cave", "Dark World South East", new SchrodingersString.Possibility("Maridia Cave", 0.1)),
                        Room = 0,
                        TopLeftX = 0,
                        TopLeftY = 0
                    }
                }
            },
            {
                "RequestedHintTile", new SchrodingersString("Example response for when asking what a hint tile says", new SchrodingersString.Possibility("Another example response for when asking what a hint tile says", 0.1))
            }
        };
    }
}
