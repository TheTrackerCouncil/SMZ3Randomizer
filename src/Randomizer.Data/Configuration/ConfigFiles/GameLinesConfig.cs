using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config for lines that will be injected into the game
    /// </summary>
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
            = new SchrodingersString("But how?! I placed\nall of the items\nrandomly about\nHyrule and another\nplanet, and yet\nyou still got here?");

        /// <summary>
        /// Gets the phrases for blind
        /// </summary>
        public SchrodingersString? BlindIntro { get; init; }
            = new SchrodingersString("I'm actually a\nbedsheet and three\nfloating heads.\nObserve:");

        /// <summary>
        /// Gets the phrases for the won game screen in Zelda
        /// </summary>
        public SchrodingersString? TriforceRoom { get; init; }
            = new SchrodingersString("\n     G G");

        /// <summary>
        /// Hints for stating that a location is mandatory for completing
        /// the game
        /// </summary>
        public SchrodingersString? HintLocationIsMandatory { get; init; }
            = new SchrodingersString("{0} is on the way of the hero.", "{0} is mandatory.");

        /// <summary>
        /// Hints for stating that a location has an item that is useful, but not
        /// mandatory for completing the game
        /// </summary>
        public SchrodingersString? HintLocationHasUsefulItem { get; init; }
            = new SchrodingersString("{0} has something nice, but it's not vital.", "{0} may be useful, but it's not required.");

        /// <summary>
        /// Hints for stating that a location has no useful items
        /// </summary>
        public SchrodingersString? HintLocationEmpty { get; init; }
            = new SchrodingersString("{0} is barren.", "{0} has nothing useful.");

        /// <summary>
        /// Hints for stating that a location has a specific item
        /// </summary>
        public SchrodingersString? HintLocationHasItem { get; init; }
            = new SchrodingersString("{0} has {1}.", "You can find {1} at {0}.");

        /// <summary>
        /// Line for King Zora saying what he has
        /// </summary>
        public SchrodingersString? KingZora { get; init; }
            = new SchrodingersString("You got 500 rupees to buy {0}?", "Carrying around 500 rupees in pocket change? I'll give you {0} for it!");

        /// <summary>
        /// Line for the bottle merchant saying what he has
        /// </summary>
        public SchrodingersString? BottleMerchant { get; init; }
            = new SchrodingersString("I'll take 100 rupees if you want {0}.", "Gimme 100 rupees, and I'll give you {0}.");

        /// <summary>
        /// Options for replying with yes to a dialog choice
        /// </summary>
        public SchrodingersString? ChoiceYes { get; init; }
            = new SchrodingersString("Yeah!", "Duh!", "Sure", "Okay.", "Of course!");

        /// <summary>
        /// Options for replying with no to a dialog choice
        /// </summary>
        public SchrodingersString? ChoiceNo { get; init; }
            = new SchrodingersString("Nah.", "Nope.", "No way!");
    }
}
