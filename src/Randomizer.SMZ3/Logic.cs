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
                yield break;
            }
            else
            {
                // 1. Create item pools
                var itemPool = Item.CreateProgressionPool(null).Select(x => x.Type).ToList();

                // 2. Subtract items in Progression from item pools
                foreach (var ownedItem in items)
                    itemPool.Remove(ownedItem);

                // 3. For every item in the progression item pool:
                foreach (var missingItem in itemPool)
                {
                    //    1. Add the items to the progression check
                    var progression = items.Clone();
                    progression.Add(missingItem);
                    //    2. If this makes the location accessible, yield return that item
                    if (location.IsAvailable(progression))
                        yield return new[] { missingItem };
                }

                // 4. Repeat the above, but for every combination of two items in the pool
            }
        }
    }
}
