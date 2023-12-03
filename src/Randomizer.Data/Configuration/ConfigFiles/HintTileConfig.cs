using System.Collections.Generic;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData;

namespace Randomizer.Data.Configuration.ConfigFiles;

/// <summary>
/// Config file for the responses to viewing and clearing hint tiles
/// and the names and details of the individual hint tiles
/// </summary>
public class HintTileConfig : IMergeable<HintTileConfig>, IConfigFile<HintTileConfig>
{
    /// <summary>
    /// The list of hint tiles and their details
    /// </summary>
    public List<HintTile> HintTiles { get; set; } = new();

    /// <summary>
    /// The response for when asking for what a hint tile says
    /// </summary>
    /// <remarks>
    /// <c>{0}</c> is a placeholder for the hint tile text
    /// </remarks>
    public SchrodingersString RequestedHintTile { get; set; } = new("That hint tile says that {0}");

    /// <summary>
    /// The response when trying to clear a hint tile despite not looking at a hint tile
    /// </summary>
    public SchrodingersString NoPreviousHintTile { get; set; } = new("You haven't looked at any hint tiles yet");

    /// <summary>
    /// The response when trying to clear an already cleared hint tile or one that has nothing to clear
    /// </summary>
    public SchrodingersString ClearHintTileFailed { get; set; } = new("I can't do anything with that hint tile");

    /// <summary>
    /// The response for viewing a hint tile with multiple locations
    /// </summary>
    public SchrodingersString ViewedHintTile { get; set; } = new("Recorded that hint tile");

    /// <summary>
    /// The response for asking about a hint tile when there aren't any generated hint tiles (plando or no hints)
    /// </summary>
    public SchrodingersString NoHintTiles { get; set; } = new("I'm sorry, but there are no hint tiles on file for this seed.");

    /// <summary>
    /// Returns default hint tile information
    /// </summary>
    /// <returns></returns>
    public static HintTileConfig Default()
    {
        return new HintTileConfig()
        {
            HintTiles = new List<HintTile>()
            {
                new()
                {
                    HintTileKey = "telepathic_tile_eastern_palace",
                    Room = 168,
                    TopLeftX = 4465,
                    TopLeftY = 5224,
                    Name = new SchrodingersString("Eastern Palace")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_tower_of_hera_floor_4",
                    Room = 39,
                    TopLeftX = 3825,
                    TopLeftY = 1288,
                    Name = new SchrodingersString("Tower of Hera Upstairs", "Tower of Hera Floor 4")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_spectacle_rock",
                    Room = 234,
                    TopLeftX = 5289,
                    TopLeftY = 7328,
                    Name = new SchrodingersString("Spectacle Rock", "Spectacle Rock Cave")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_swamp_entrance",
                    Room = 40,
                    TopLeftX = 4393,
                    TopLeftY = 1232,
                    Name = new SchrodingersString("Swamp Palace")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_thieves_town_upstairs",
                    Room = 100,
                    TopLeftX = 2121,
                    TopLeftY = 3376,
                    Name = new SchrodingersString("Thieves Town")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_misery_mire",
                    Room = 151,
                    TopLeftX = 3881,
                    TopLeftY = 4776,
                    Name = new SchrodingersString("Misery Mire")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_palace_of_darkness",
                    Room = 75,
                    TopLeftX = 6001,
                    TopLeftY = 2096,
                    Name = new SchrodingersString("Palace of Darkness")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_desert_bonk_torch_room",
                    Room = 115,
                    TopLeftX = 1905,
                    TopLeftY = 3632,
                    Name = new SchrodingersString("Desert Palace")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_castle_tower",
                    Room = 176,
                    TopLeftX = 313,
                    TopLeftY = 5680,
                    Name = new SchrodingersString("Castle Tower", "Agahnim's Tower")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_ice_large_room",
                    Room = 126,
                    TopLeftX = 7537,
                    TopLeftY = 3632,
                    Name = new SchrodingersString("Ice Palace Big Room", "Ice Palace Tall Room")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_turtle_rock",
                    Room = 214,
                    TopLeftX = 3369,
                    TopLeftY = 7056,
                    Name = new SchrodingersString("Turtle Rock")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_ice_entrance",
                    Room = 14,
                    TopLeftX = 7537,
                    TopLeftY = 304,
                    Name = new SchrodingersString("Ice Palace Entrance")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_ice_stalfos_knights_room",
                    Room = 62,
                    TopLeftX = 7537,
                    TopLeftY = 1584,
                    Name = new SchrodingersString("Ice Palace Stalfos Knights Room", "Ice Palace Stalfos Room")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_tower_of_hera_entrance",
                    Room = 119,
                    TopLeftX = 3825,
                    TopLeftY = 3816,
                    Name = new SchrodingersString("Tower of Hera Entrance")
                },
                new()
                {
                    HintTileKey = "telepathic_tile_south_east_darkworld_cave",
                    Room = 293,
                    TopLeftX = 2929,
                    TopLeftY = 9520,
                    Name = new SchrodingersString("Dark World Cave", "Maridia Cave", "Dark World South East")
                },
            }
        };
    }
}
