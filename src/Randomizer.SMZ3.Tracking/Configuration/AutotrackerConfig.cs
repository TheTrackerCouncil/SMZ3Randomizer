using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Provides the phrases for chat integration.
    /// </summary>
    public class AutotrackerConfig
    {
        /// <summary>
        /// Gets the phrases to respond with when connected to to emulator.
        /// </summary>
        public SchrodingersString WhenConnected { get; init; }
            = new("Autotracker connected");

        /// <summary>
        /// Gets the phrases to respond with when the game is started.
        /// </summary>
        public SchrodingersString GameStarted { get; init; }
            = new("Incoming PB for seed {0}");

        /// <summary>
        /// Gets the phrases to respond with when nearing KAD
        /// </summary>
        public SchrodingersString NearKraidsAwfulSon { get; init; }
            = new("Oh no. Disaster imminent.");

        /// <summary>
        /// Gets the phrases to respond with when nearing shaktool
        /// </summary>
        public SchrodingersString NearShaktool { get; init; }
            = new("Finally, the moment we've all been waiting for since the start of this run. We get to see Shaktool.");

        /// <summary>
        /// Gets the phrases to respond with when falling down the pit from moldorm
        /// </summary>
        public SchrodingersString FallFromMoldorm { get; init; }
            = new("Ha ha. Moldorm strikes again. Content increased by one step.");

        /// <summary>
        /// Gets the phrases to respond with when performing Hera Pot tech
        /// </summary>
        public SchrodingersString HeraPot { get; init; }
            = new("Good job on the tech. Was it a first try?");

    }
}
