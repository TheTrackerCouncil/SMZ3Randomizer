using System;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Provides the phrases for map.
    /// </summary>
    public class MapResponseConfig : IMergeable<MapResponseConfig>
    {
        /// <summary>
        /// Gets the phrases to respond with when showing the player a dark room map
        /// </summary>
        public SchrodingersString UpdateMap { get; init; }
             = new SchrodingersString("Showing you the {0} map");

        /// <summary>
        /// Gets the phrases to respond with when showing the player a dark room map
        /// </summary>
        public SchrodingersString ShowDarkRoomMap { get; init; }
             = new SchrodingersString("Showing you the {0} map");

        /// <summary>
        /// Gets the phrases to respond with when the player asks for a dark room map but they're not in one
        /// </summary>
        public SchrodingersString NotInDarkRoom { get; init; }
             = new SchrodingersString("I don't think I can help you here.");

        /// <summary>
        /// Gets the phrases to respond with when hiding a dark room map
        /// </summary>
        public SchrodingersString HideDarkRoomMap { get; init; }
             = new SchrodingersString("Did you make it through?");

        /// <summary>
        /// Gets the phrases to respond with when hiding a dark room map but there was no previous map
        /// </summary>
        public SchrodingersString NoPrevDarkRoomMap { get; init; }
             = new SchrodingersString("I don't know which map to show you");

        /// <summary>
        /// Gets the phrases to respond with saying that they can't see but have the lamp
        /// </summary>
        public SchrodingersString HasLamp { get; init; }
             = new SchrodingersString("But you have the lamp");

    }
}
