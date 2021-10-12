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
    }
}
