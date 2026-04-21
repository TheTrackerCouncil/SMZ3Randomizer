using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

[Description("Config file for various tracker lines and in game text for alt game modes.\r\n" +
             "Note that all lines in game need to be no more than 19 characters.")]
public class GameModesConfig : IMergeable<GameModesConfig>, IConfigFile<GameModesConfig>
{
    public static GameModesConfig Default()
    {
        return new GameModesConfig();
    }

    /// <summary>
    /// In game text for the intro to the Ganon battle
    /// </summary>
    public SchrodingersString? SpazerHuntGanonIntro { get; set; }

    /// <summary>
    /// In game text for the intro to the Ganon battle when you have not collected enough spazers
    /// </summary>
    public SchrodingersString? SpazerHuntGanonIntroGoalsNotMet { get; set; }

    /// <summary>
    /// In game text for the sign on Ganon's pyramid to explain the goal
    /// <c>{0}</c> is a placeholder for the number of spazers needed to fight Ganon
    /// <c>{1}</c> is a placeholder for the number of spazers in the item pool
    /// </summary>
    public SchrodingersString? SpazerHuntGanonGoalSign { get; set; }

    /// <summary>
    /// Tracker line for when starting a new game
    /// </summary>
    public SchrodingersString? SpazerHuntGameStarted { get; set; }

    /// <summary>
    /// In game text for the intro to the Ganon battle for All Dungeons mode
    /// </summary>
    public SchrodingersString? AllDungeonsGanonIntro { get; set; }

    /// <summary>
    /// In game text for the intro to the Ganon battle when you have not met the requirements for All Dungeons mode
    /// </summary>
    public SchrodingersString? AllDungeonsGanonIntroGoalsNotMet { get; set; }

    /// <summary>
    /// In game text for the sign on Ganon's pyramid to explain the goal for All Dungeons mode
    /// </summary>
    public SchrodingersString? AllDungeonsGanonGoalSign { get; set; }

    /// <summary>
    /// Tracker line for when starting a new game in All Dungeons mode
    /// </summary>
    public SchrodingersString? AllDungeonsGameStarted { get; set; }

    public static object Example()
    {
        return new Dictionary<string, object>()
        {
            { "SpazerHuntGanonIntro", new SchrodingersString("In game text with\neach line being\n19 characters\nor less", new SchrodingersString.Possibility("Another example\nof in game text", 0.1)) },
            { "AllDungeonsGanonIntro", new SchrodingersString("In game text with\neach line being\n19 characters\nor less", new SchrodingersString.Possibility("Another example\nof in game text", 0.1)) }
        };
    }
}
