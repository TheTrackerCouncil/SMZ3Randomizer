﻿using System;
using System.Collections.Generic;
using System.Linq;

using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Manages items and their tracking state.
    /// </summary>
    public class ItemService : IItemService
    {
        private static readonly Random s_random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemService"/> class
        /// with the specified dependencies.
        /// </summary>
        /// <param name="trackerConfig">
        /// Specifies the configuration that contains the item data to be
        /// managed.
        /// </param>
        public ItemService(TrackerConfig trackerConfig)
        {
            Items = trackerConfig.Items;
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        protected IReadOnlyCollection<ItemData> Items { get; }

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
        public virtual ItemData? FindOrDefault(string name)
            => Items.SingleOrDefault(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            ?? Items.SingleOrDefault(x => x.GetStage(name) != null);

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
        public virtual ItemData? GetOrDefault(ItemType itemType)
            => Items.RandomOrDefault(x => x.InternalItemType == itemType, s_random);

        /// <summary>
        /// Returns the item data of the item at the specified location.
        /// </summary>
        /// <param name="location">The location whose item to check.</param>
        /// <returns>
        /// An <see cref="ItemData"/> representing the type of item found at the
        /// specified location, or <see langword="null"/> if the location has no
        /// item or if the item at the location is not configured.
        /// </returns>
        public virtual ItemData? GetOrDefault(Location location) // TODO: move this to IWorldService, whenever that becomes available
            => GetOrDefault(location.Item.Type);

        /// <summary>
        /// Indicates whether an item of the specified type has been tracked.
        /// </summary>
        /// <param name="itemType">The type of item to check.</param>
        /// <returns>
        /// <see langword="true"/> if an item with the specified type has been
        /// tracked at least once; otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool IsTracked(ItemType itemType)
            => Items.Any(x => x.InternalItemType == itemType && x.TrackingState > 0);

        /// <summary>
        /// Enumerates all items that can be tracked.
        /// </summary>
        /// <returns>A collection of items.</returns>
        public virtual IEnumerable<ItemData> AllItems() // I really want to discourage this, but necessary for now
            => Items;

        /// <summary>
        /// Enumarates all currently tracked items.
        /// </summary>
        /// <returns>
        /// A collection of items that have been tracked at least once.
        /// </returns>
        public virtual IEnumerable<ItemData> TrackedItems()
            => Items.Where(x => x.TrackingState > 0);

        /// <summary>
        /// Returns a random name for the specified item including article, e.g.
        /// "an E-Tank" or "the Book of Mudora".
        /// </summary>
        /// <param name="itemType">The type of item whose name to get.</param>
        /// <returns>
        /// The name of the type of item, including "a", "an" or "the" if
        /// applicable.
        /// </returns>
        public virtual string GetName(ItemType itemType)
        {
            var item = GetOrDefault(itemType);
            return item?.NameWithArticle ?? itemType.GetDescription();
        }

        // TODO: Tracking methods
    }
}
