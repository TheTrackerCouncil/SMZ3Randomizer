namespace TrackerCouncil.Smz3.Shared.Enums;

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
    /// The item is a keycard for a level 1 door in Super Metroid.
    /// </summary>
    KeycardL1,

    /// <summary>
    /// The item is a keycard for a level 2 door in Super Metroid.
    /// </summary>
    KeycardL2,

    /// <summary>
    /// The item is a keycard for a boss door in Super Metroid.
    /// </summary>
    KeycardBoss,

    /// <summary>
    /// The item is not worth paying 500 rupees for, but not necessarily
    /// junk.
    /// </summary>
    Scam,

    /// <summary>
    /// The item is considered junk (e.g. arrows, rupees, capacity
    /// upgrades).
    /// </summary>
    Junk,

    /// <summary>
    /// The item is especially numerious and can be found in a large number
    /// of locations.
    /// </summary>
    Plentiful,

    /// <summary>
    /// This is an item that is not randomized, such as filled bottles
    /// </summary>
    NonRandomized,

    /// <summary>
    ///This is an item that is useful, but not progression
    /// </summary>
    Nice,

    /// <summary>
    /// This is a medallion that can be used to enter a dungeon
    /// </summary>
    Medallion,

    /// <summary>
    /// This is one of the bottle item types
    /// </summary>
    Bottle,

    /// <summary>
    /// If this should not be given out when completing multiplayer games
    /// </summary>
    IgnoreOnMultiplayerCompletion,

    /// <summary>
    /// An item that is never considered progression, such as maps, compasses, or arrows
    /// </summary>
    NeverProgression,

    /// <summary>
    /// An item that can be the progression if it's one of the first ones picked up
    /// </summary>
    ProgressionOnLimitedAmount,

    /// <summary>
    /// An item that is at least sometimes required to complete the game
    /// </summary>
    PossibleProgression,

    /// <summary>
    /// If this is a Metroid map in an archipelago game
    /// </summary>
    MetroidMap
}
