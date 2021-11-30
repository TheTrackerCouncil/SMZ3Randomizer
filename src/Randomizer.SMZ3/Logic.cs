using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Randomizer.Shared;

namespace Randomizer.SMZ3
{
    public static class Logic
    {
        public static IEnumerable<ItemType[]> GetMissingRequiredItems(Location location, Progression items)
        {
            if (location.IsAvailable(items))
            {
                return Enumerable.Empty<ItemType[]>();
            }

            // Build an item pool of all missing progression items
            var combinations = new List<ItemType[]>();
            var itemPool = Item.CreateProgressionPool(null).Select(x => x.Type).ToList();
            foreach (var ownedItem in items)
                itemPool.Remove(ownedItem);

            // Try all items by themselves
            foreach (var missingItem in itemPool)
            {
                var progression = items.Clone();
                progression.Add(missingItem);

                if (location.IsAvailable(progression))
                    combinations.Add(new[] { missingItem });
            }

            // Remove all successfull combinations from the pool to prevent redundant combinations
            foreach (var combination in combinations.SelectMany(x => x))
                itemPool.Remove(combination);

            // Try all combinations of two
            foreach (var missingItem in itemPool)
            {
                foreach (var missingItem2 in itemPool)
                {
                    var progression = items.Clone();
                    progression.Add(missingItem);
                    progression.Add(missingItem2);
                    if (location.IsAvailable(progression))
                        combinations.Add(new[] { missingItem, missingItem2 });
                }
            }

            return combinations;
        }
    }
}
