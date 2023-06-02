namespace Randomizer.Data.WorldData
{
    public enum LocationType
    {
        Regular,
        HeraStandingKey,
        Pedestal,
        Ether,
        Bombos,
        NotInDungeon,

        /// <summary>
        /// The item is sitting out in the open on Zebes.
        /// </summary>
        Visible,

        /// <summary>
        /// The item is hiding in a Chozo egg that must be damaged reveal it.
        /// It may or may not be being held by a Chozo statue.
        /// </summary>
        Chozo,

        /// <summary>
        /// The item is hiding in a wall or ceiling tile that must be damaged
        /// to reveal it.
        /// </summary>
        Hidden
    }

}
