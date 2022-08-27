using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
{
    /// <summary>
    /// Provides the phrases for cheats
    /// </summary>
    public class CheatsConfig : IMergeable<CheatsConfig>
    {
        /// <summary>
        /// Gets the phrases to respond with when cheats are turned on.
        /// </summary>
        public SchrodingersString EnabledCheats { get; init; }
            = new("Toggled cheats on.");

        /// <summary>
        /// Gets the phrases to respond with when cheats are turned off.
        /// </summary>
        public SchrodingersString DisabledCheats { get; init; }
            = new("Toggled cheats off.");

        /// <summary>
        /// Gets the phrases to respond when a cheat command is given and cheats
        /// are turned off
        /// </summary>
        public SchrodingersString PromptEnableCheats { get; init; }
            = new("If you would like to cheat, say 'Hey tracker, enable cheats'");

        /// <summary>
        /// Gets the phrases to respond when a cheat command is given and auto
        /// tracker is not connected
        /// </summary>
        public SchrodingersString PromptEnableAutoTracker { get; init; }
            = new("If you would like to cheat, please enable auto tracking");

        /// <summary>
        /// Gets the phrases to respond with when connected to to emulator.
        /// </summary>
        public SchrodingersString CheatPerformed { get; init; }
            = new("Cheat code activated");

        /// <summary>
        /// Gets the phrases to respond when the player can't perform a certain cheat.
        /// </summary>
        public SchrodingersString CheatFailed { get; init; }
            = new("Sorry, I can't perform that cheat for you at this time");

        /// <summary>
        /// Gets the phrases to respond with when the player tries to cheat
        /// themselves an item that does not exist.
        /// </summary>
        public SchrodingersString CheatInvalidItem { get; init; }
            = new("Invalid item requested");
    }
}
