using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Represents the currently active tracker configuration.
    /// </summary>
    public class TrackerConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerConfig"/> class
        /// with the specified item data.
        /// </summary>
        /// <param name="items">The item data to use.</param>
        public TrackerConfig(IReadOnlyCollection<ItemData> items)
        {
            Items = items;
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        public IReadOnlyCollection<ItemData> Items { get; init; }

        /// <summary>
        /// Returns the item that has the specified name.
        /// </summary>
        /// <param name="name">The name of the item to find.</param>
        /// <returns>
        /// The <see cref="ItemData"/> with the specified name, or <c>null</c>
        /// if no items have a matching name.
        /// </returns>
        /// <remarks>
        /// Names are case insensitive, and items can be found either by their
        /// default name, additional names, or stage names.
        /// </remarks>
        public ItemData? FindItemByName(string name)
        {
            return Items.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?? Items.SingleOrDefault(x => x.OtherNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                ?? Items.Where(x => x.Stages != null)
                        .SingleOrDefault(x => x.Stages!.SelectMany(stage => stage.Value).Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }

        public Progression GetProgression()
        {
            var items = new List<Item>();
            foreach (var item in Items)
            {
                for (var i = 0; i < item.TrackingState; i++)
                    items.Add(new Item(item.InternalItemType));
            }
            return new Progression(items);
        }
    }
}
