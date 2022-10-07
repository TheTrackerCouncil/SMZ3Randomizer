using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
{
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
    }
}
