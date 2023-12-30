namespace Randomizer.Data.Configuration.ConfigTypes
{
    /// <summary>
    /// Provides the phrases for map.
    /// </summary>
    public class MapResponseConfig : IMergeable<MapResponseConfig>
    {
        /// <summary>
        /// Gets the phrases to respond with when showing the player a dark room map
        /// </summary>
        public SchrodingersString? UpdateMap { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when showing the player a dark room map
        /// </summary>
        public SchrodingersString? ShowDarkRoomMap { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when the player asks for a dark room map but they're not in one
        /// </summary>
        public SchrodingersString? NotInDarkRoom { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when hiding a dark room map
        /// </summary>
        public SchrodingersString? HideDarkRoomMap { get; init; }

        /// <summary>
        /// Gets the phrases to respond with when hiding a dark room map but there was no previous map
        /// </summary>
        public SchrodingersString? NoPrevDarkRoomMap { get; init; }

        /// <summary>
        /// Gets the phrases to respond with saying that they can't see but have the lamp
        /// </summary>
        public SchrodingersString? HasLamp { get; init; }

    }
}
