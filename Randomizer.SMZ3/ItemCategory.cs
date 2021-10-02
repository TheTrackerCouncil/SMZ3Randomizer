namespace Randomizer.SMZ3
{
    /// <summary>
    /// Specifies the categories of an item.
    /// </summary>
    public enum ItemCategory
    {
        /// <summary>
        /// The item has no categories.
        /// </summary>
        None = 0,

        /// <summary>
        /// The item is from A Link to the Past.
        /// </summary>
        Zelda,

        /// <summary>
        /// The item is from Super Metroid.
        /// </summary>
        Metroid,

        /// <summary>
        /// The item is a dungeon map.
        /// </summary>
        Map,

        /// <summary>
        /// The item is a dungeon compass.
        /// </summary>
        Compass,

        /// <summary>
        /// The item is a boss key for a dungeon.
        /// </summary>
        BigKey,

        /// <summary>
        /// The item is a small key for a dungeon.
        /// </summary>
        SmallKey,

        /// <summary>
        /// The item is a keycard for a door in Super Metroid.
        /// </summary>
        Keycard,

        /// <summary>
        /// The item is not worth paying 500 rupees for, but not necessarily junk.
        /// </summary>
        Scam,

        /// <summary>
        /// The item is considered junk (e.g. arrows, rupees, capacity upgrades).
        /// </summary>
        Junk,
    }
}
