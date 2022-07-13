using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BunLabs;

using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Manages items and their tracking state.
    /// </summary>
    public class ItemService
    {
        private static Random s_random = new Random();

        public ItemService(TrackerConfig trackerConfig)
        {
            Items = trackerConfig.Items;
        }

        /// <summary>
        /// Gets a collection of trackable items.
        /// </summary>
        protected IReadOnlyCollection<ItemData> Items { get; }

        public virtual ItemData? Find(string name)
            => Items.SingleOrDefault(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            ?? Items.SingleOrDefault(x => x.GetStage(name) != null);

        public virtual ItemData? Get(ItemType itemType)
            => Items.RandomOrDefault(x => x.InternalItemType == itemType, s_random);

        public virtual bool IsTracked(ItemType itemType)
            => Items.Any(x => x.InternalItemType == itemType && x.TrackingState > 0);

        public virtual IEnumerable<ItemData> AllItems() // I really want to discourage this, but necessary for now
            => Items;

        public virtual IEnumerable<ItemData> TrackedItems()
            => Items.Where(x => x.TrackingState > 0);

        public virtual ItemData? Get(Location location)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a random name for the specified item including article,
        /// e.g. "an E-Tank" or "the Book of Mudora".
        /// </summary>
        /// <param name="itemType">The type of item whose name to get.</param>
        /// <returns>The name of the type of item, including "a", "an" or "the" if applicable.</returns>
        public virtual string GetName(ItemType itemType)
        {
            var item = Get(itemType);
            return item?.NameWithArticle ?? itemType.GetDescription();
        }

        public virtual void Track(ItemData itemData/* ... */)
        {
            throw new NotImplementedException();
        }

        public virtual void Track(ItemType itemType/* ... */)
        {
            throw new NotImplementedException();
        }
    }
}
