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

    }
}
