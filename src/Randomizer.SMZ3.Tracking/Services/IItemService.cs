using System.Collections.Generic;

using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

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
        IEnumerable<ItemData> AllItems();

        /// <summary>
        /// Enumarates all currently tracked items.
        /// </summary>
        /// <returns>
        /// A collection of items that have been tracked at least once.
        /// </returns>
        IEnumerable<ItemData> TrackedItems();

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
        ItemData? FindOrDefault(string name);

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
        ItemData? GetOrDefault(ItemType itemType);

        /// <summary>
        /// Returns the item data of the item at the specified location.
        /// </summary>
        /// <param name="location">The location whose item to check.</param>
        /// <returns>
        /// An <see cref="ItemData"/> representing the type of item found at the
        /// specified location, or <see langword="null"/> if the location has no
        /// item or if the item at the location is not configured.
        /// </returns>
        ItemData? GetOrDefault(Location location);

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
        RewardInfo? GetOrDefault(RewardType rewardType);

        /// <summary>
        /// Finds an reward with the specified item type.
        /// </summary>
        /// <param name="rewardItem">The type of reward to find.</param>
        /// <returns>
        /// An <see cref="RewardInfo"/> representing the reward. If there are
        /// multiple configured rewards with the same type, this method returns
        /// one at random. If there no configured rewards with the specified type,
        /// this method returns <see langword="null"/>.
        /// </returns>
        RewardInfo? GetOrDefault(RewardItem rewardItem);

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
        /// Returns a random name for the specified item including article, e.g.
        /// "a blue crystal" or "the green pendant".
        /// </summary>
        /// <param name="rewardItem">The reward of item whose name to get.</param>
        /// <returns>
        /// The name of the reward of item, including "a", "an" or "the" if
        /// applicable.
        /// </returns>
        string GetName(RewardItem rewardItem);
    }
}
