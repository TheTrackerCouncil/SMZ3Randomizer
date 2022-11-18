using System.Collections.Generic;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Defines methods for managing items and their tracking state.
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Enumerates all items that can be tracked.
        /// </summary>
        /// <returns>A collection of items.</returns>

        IEnumerable<Item> AllItems();

        /// <summary>
        /// Enumarates all currently tracked items.
        /// </summary>
        /// <returns>
        /// A collection of items that have been tracked at least once.
        /// </returns>
        IEnumerable<Item> TrackedItems();

        /// <summary>
        /// Finds the item with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the item or item stage to find.
        /// </param>
        /// <returns>
        /// An <see cref="ItemData"/> representing the item with the specified
        /// name, or <see langword="null"/> if there is no item that has the
        /// specified name.
        /// </returns>
        Item? FirstOrDefault(string name);

        /// <summary>
        /// Finds an item with the specified item type.
        /// </summary>
        /// <param name="itemType">The type of item to find.</param>
        /// <returns>
        /// An <see cref="ItemData"/> representing the item. If there are
        /// multiple configured items with the same type, this method returns
        /// one at random. If there no configured items with the specified type,
        /// this method returns <see langword="null"/>.
        /// </returns>
        Item? FirstOrDefault(ItemType itemType);

        /// <summary>
        /// Returns a random name for the specified item including article, e.g.
        /// "an E-Tank" or "the Book of Mudora".
        /// </summary>
        /// <param name="itemType">The type of item whose name to get.</param>
        /// <returns>
        /// The name of the type of item, including "a", "an" or "the" if
        /// applicable.
        /// </returns>
        string GetName(ItemType itemType);

        /// <summary>
        /// Indicates whether an item of the specified type has been tracked.
        /// </summary>
        /// <param name="itemType">The type of item to check.</param>
        /// <returns>
        /// <see langword="true"/> if an item with the specified type has been
        /// tracked at least once; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsTracked(ItemType itemType);

        /// <summary>
        /// Finds an reward with the specified item type.
        /// </summary>
        /// <param name="rewardType">The type of reward to find.</param>
        /// <returns>
        /// An <see cref="RewardInfo"/> representing the reward. If there are
        /// multiple configured rewards with the same type, this method returns
        /// one at random. If there no configured rewards with the specified type,
        /// this method returns <see langword="null"/>.
        /// </returns>
        Reward? FirstOrDefault(RewardType rewardType);

        /// <summary>
        /// Returns a random name for the specified item including article, e.g.
        /// "a blue crystal" or "the green pendant".
        /// </summary>
        /// <param name="rewardType">The reward of item whose name to get.</param>
        /// <returns>
        /// The name of the reward of item, including "a", "an" or "the" if
        /// applicable.
        /// </returns>
        string GetName(RewardType rewardType);

        /// <summary>
        /// Enumerates all rewards that can be tracked.
        /// </summary>
        /// <returns>A collection of rewards.</returns>

        IEnumerable<Reward> AllRewards();

        /// <summary>
        /// Enumarates all currently tracked rewards.
        /// </summary>
        /// <returns>
        /// A collection of reward that have been tracked.
        /// </returns>
        IEnumerable<Reward> TrackedRewards();

        /// <summary>
        /// Enumerates all bosses that can be tracked.
        /// </summary>
        /// <returns>A collection of bosses.</returns>

        IEnumerable<Boss> AllBosses();

        /// <summary>
        /// Enumarates all currently tracked bosses.
        /// </summary>
        /// <returns>
        /// A collection of bosses that have been tracked.
        /// </returns>
        IEnumerable<Boss> TrackedBosses();

        /// <summary>
        /// Retrieves the progression containing all of the tracked items, rewards, and bosses
        /// for determining in logic locations
        /// </summary>
        /// <param name="assumeKeys">If it should be assumed that the player has all keys and keycards</param>
        /// <returns></returns>
        Progression GetProgression(bool assumeKeys);

        /// <summary>
        /// Retrieves the progression containing all of the tracked items, rewards, and bosses
        /// for determining in logic locations
        /// </summary>
        /// <param name="area">The area being looked at to see if keys/keycards should be assumed or not</param>
        /// <returns></returns>
        Progression GetProgression(IHasLocations area);

        /// <summary>
        /// Clears cached progression
        /// </summary>
        void ResetProgression();
    }
}
