namespace Randomizer.Data.Configuration.ConfigTypes
{
    /// <summary>
    /// Provides the phrases for playing multiplayer
    /// </summary>
    public class MultiplayerConfig : IMergeable<MultiplayerConfig>
    {
        /// <summary>
        /// Gets the phrases for when another player tracks a useful item the local player does not have
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the player name
        /// <c>{1}</c> is a placeholder for the item name
        /// <c>{2}</c> is a placeholder for the item name prefixed by an article
        /// </remarks>
        public SchrodingersString OtherPlayerTrackedItem { get; init; }
            = new("You should congratulate {0} on getting the {1}.");

        /// <summary>
        /// Gets the phrases for when another player clears a dungeon
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the player name
        /// <c>{1}</c> is a placeholder for the dungeon name
        /// <c>{2}</c> is a placeholder for the dungeon boss name
        /// <c>{3}</c> is a placeholder for the reward name
        /// <c>{4}</c> is a placeholder for the reward name prefixed by an article
        /// </remarks>
        public SchrodingersString OtherPlayerClearedDungeonWithReward { get; init; }
            = new("You should congratulate {0} on getting the {3} from {1}.");

        /// <summary>
        /// Gets the phrases for when another player clears a dungeon
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the player name
        /// <c>{1}</c> is a placeholder for the dungeon name
        /// <c>{2}</c> is a placeholder for the dungeon boss name
        /// </remarks>
        public SchrodingersString OtherPlayerClearedDungeonWithoutReward { get; init; }
            = new("You should congratulate {0} on completing {1}.");

        /// <summary>
        /// Gets the phrases to respond when a player defeats a golden boss in SM
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the player name
        /// <c>{1}</c> is a placeholder for the boss name
        /// </remarks>
        public SchrodingersString OtherPlayerDefeatedBoss { get; init; }
            = new("You should congratulate {0} on defeating {1}.");

        /// <summary>
        /// Gets the phrases to respond when a player beats the game by defeating both Ganon and Mother Brain
        /// before the local player has completed for forfeited.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the player name
        /// </remarks>
        public SchrodingersString OtherPlayerBeatGame { get; init; }
            = new("You should congratulate {0} on finishing the game.");

        /// <summary>
        /// Gets the phrases to respond when a player forfeits from the game
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the player name
        /// </remarks>
        public SchrodingersString OtherPlayerForfeitGame { get; init; }
            = new("{0} has forfeit. Distributing the items from their world.");

        /// <summary>
        /// Gets the phrases to respond when a player gets an item that is for another player
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the player name the item is being gifted to
        /// <c>{1}</c> is a placeholder for the item name
        /// <c>{2}</c> is a placeholder for the item name prefixed by an article
        /// </remarks>
        public SchrodingersString GiftedUsefulItemToOtherPlayer { get; init; }
            = new("Say goodbye to {1}. That goes to {0} instead.");

        /// <summary>
        /// Gets the phrases to respond when a player receives an item from aanother player
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the player name the item is being gifted from
        /// <c>{1}</c> is a placeholder for the item name
        /// <c>{2}</c> is a placeholder for the item name prefixed by an article
        /// </remarks>
        public SchrodingersString ReceivedUsefulItemFromOtherPlayer { get; init; }
            = new("You should thank {0} for that {1}.");
    }
}
