using TrackerCouncil.Data.Configuration;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Provides the phrases for playing multiplayer
/// </summary>
public class MultiplayerConfig : IMergeable<MultiplayerConfig>
{
    /// <summary>
    /// Gets the phrases for when another player tracks a useful item the local player does not have
    /// <c>{0}</c> is a placeholder for the player name
    /// <c>{1}</c> is a placeholder for the item name
    /// <c>{2}</c> is a placeholder for the item name prefixed by an article
    /// </summary>
    public SchrodingersString? OtherPlayerTrackedItem { get; init; }

    /// <summary>
    /// Gets the phrases for when another player clears a dungeon
    /// <c>{0}</c> is a placeholder for the player name
    /// <c>{1}</c> is a placeholder for the dungeon name
    /// <c>{2}</c> is a placeholder for the dungeon boss name
    /// <c>{3}</c> is a placeholder for the reward name
    /// <c>{4}</c> is a placeholder for the reward name prefixed by an article
    /// </summary>
    public SchrodingersString? OtherPlayerClearedDungeonWithReward { get; init; }

    /// <summary>
    /// Gets the phrases for when another player clears a dungeon
    /// <c>{0}</c> is a placeholder for the player name
    /// <c>{1}</c> is a placeholder for the dungeon name
    /// <c>{2}</c> is a placeholder for the dungeon boss name
    /// </summary>
    public SchrodingersString? OtherPlayerClearedDungeonWithoutReward { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a player defeats a golden boss in SM
    /// <c>{0}</c> is a placeholder for the player name
    /// <c>{1}</c> is a placeholder for the boss name
    /// </summary>
    public SchrodingersString? OtherPlayerDefeatedBoss { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a player dies
    /// <c>{0}</c> is a placeholder for the player name
    /// </summary>
    public SchrodingersString? OtherPlayedDied { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a player dies and death link is enabled
    /// <c>{0}</c> is a placeholder for the player name
    /// </summary>
    public SchrodingersString? OtherPlayedDiedDeathLink { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a player beats the game by defeating both Ganon and Mother Brain
    /// before the local player has completed or forfeited.
    /// <c>{0}</c> is a placeholder for the player name
    /// </summary>
    public SchrodingersString? OtherPlayerBeatGame { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a player beats the game by defeating both Ganon and Mother Brain
    /// before the local player has completed for forfeited, but the local player isn't getting any items
    /// <c>{0}</c> is a placeholder for the player name
    /// </summary>
    public SchrodingersString? OtherPlayerBeatGameNoItems { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a player forfeits from the game
    /// <c>{0}</c> is a placeholder for the player name
    /// </summary>
    public SchrodingersString? OtherPlayerForfeitGame { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a player gets an item that is for another player
    /// <c>{0}</c> is a placeholder for the player name the item is being gifted to
    /// <c>{1}</c> is a placeholder for the item name
    /// <c>{2}</c> is a placeholder for the item name prefixed by an article
    /// </summary>
    public SchrodingersString? GiftedUsefulItemToOtherPlayer { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a player receives a potentially progression item from another player
    /// <c>{0}</c> is a placeholder for the player name the item is being gifted from
    /// <c>{1}</c> is a placeholder for the item name
    /// <c>{2}</c> is a placeholder for the item name prefixed by an article
    /// </summary>
    public SchrodingersString? ReceivedUsefulItemFromOtherPlayer { get; init; }

    /// <summary>
    /// Gets the phrases to respond when a player receives a junk item from another player
    /// <c>{0}</c> is a placeholder for the player name the item is being gifted from
    /// <c>{1}</c> is a placeholder for the item name
    /// <c>{2}</c> is a placeholder for the item name prefixed by an article
    /// </summary>
    public SchrodingersString? ReceivedJunkItemFromOtherPlayer { get; init; }
}
