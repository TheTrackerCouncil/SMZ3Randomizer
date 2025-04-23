using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Config for lines that will be injected into the game
/// </summary>
[Description("Config file for lines that will be injected into the game upon creation.\r\n" +
             "Note that all lines in game need to be no more than 19 characters.")]
public class GameLinesConfig : IMergeable<GameLinesConfig>, IConfigFile<GameLinesConfig>
{
    public static GameLinesConfig Default()
    {
        return new();
    }

    /// <summary>
    /// Gets the phrases for first dropping down into Ganon's room.
    /// </summary>
    public SchrodingersString? GanonIntro { get; init; }

    /// <summary>
    /// Gets the phrases for blind
    /// </summary>
    public SchrodingersString? BlindIntro { get; init; }

    /// <summary>
    /// Gets the phrases for the won game screen in Zelda
    /// </summary>
    public SchrodingersString? TriforceRoom { get; init; }

    /// <summary>
    /// Hints for stating that a location is mandatory for completing
    /// the game
    /// </summary>
    public SchrodingersString? HintLocationIsMandatory { get; init; }

    /// <summary>
    /// Hints for stating that a location has a dungeon key or keycard
    /// </summary>
    public SchrodingersString? HintLocationHasKey { get; init; }

    /// <summary>
    /// Hints for stating that a location has an item that is useful, but not
    /// mandatory for completing the game
    /// </summary>
    public SchrodingersString? HintLocationHasUsefulItem { get; init; }

    /// <summary>
    /// Hints for stating that a dungeon needs a medallion
    /// </summary>
    public SchrodingersString? HintDungeonMedallion { get; init; }

    /// <summary>
    /// Hints for stating that a location has a sword
    /// </summary>
    public SchrodingersString? HintLocationHasSword { get; init; }

    /// <summary>
    /// Hints for stating that a location has no useful items
    /// </summary>
    public SchrodingersString? HintLocationEmpty { get; init; }

    /// <summary>
    /// Hints for stating that a location has a specific item
    /// </summary>
    public SchrodingersString? HintLocationHasItem { get; init; }

    /// <summary>
    /// Line for King Zora saying what he has
    /// </summary>
    public SchrodingersString? KingZora { get; init; }

    /// <summary>
    /// Line for the bottle merchant saying what he has
    /// </summary>
    public SchrodingersString? BottleMerchant { get; init; }

    /// <summary>
    /// Options for replying with yes to a dialog choice
    /// </summary>
    public SchrodingersString? ChoiceYes { get; init; }

    /// <summary>
    /// Options for replying with no to a dialog choice
    /// </summary>
    public SchrodingersString? ChoiceNo { get; init; }

    /// <summary>
    /// Options for Ganon giving a hint to the silvers location
    /// </summary>
    public SchrodingersString? GanonSilversHint { get; init; }

    /// <summary>
    /// options for Ganon saying that silvers aren't found for the player in a plando
    /// </summary>
    public SchrodingersString? GanonNoSilvers { get; init; }

    /// <summary>
    /// Options for Sahasrahla's green pendant dungeon reveal text
    /// </summary>
    public SchrodingersString? SahasrahlaReveal { get; init; }

    /// <summary>
    /// Options for the bomb shop red crystal dungeon reveal text
    /// </summary>
    public SchrodingersString? BombShopReveal { get; init; }

    /// <summary>
    /// Options for the guy in the Kak bar that has a bunch of jokes in vanilla
    /// </summary>
    public SchrodingersString? TavernMan { get; init; }

    public static object Example()
    {
        return new Dictionary<string, object>()
        {
            { "GanonIntro", new SchrodingersString("In game text with\neach line being\n19 characters\nor less", new SchrodingersString.Possibility("Another example\nof in game text", 0.1)) }
        };
    }
}
