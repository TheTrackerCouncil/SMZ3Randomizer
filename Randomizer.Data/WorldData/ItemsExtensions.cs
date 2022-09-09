using System;
using System.Linq;
using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.Data.WorldData
{
    public static class ItemsExtensions
    {

        public static Item Get(this IEnumerable<Item> items, ItemType itemType)
        {
            var item = items.FirstOrDefault(i => i.Type == itemType);
            if (item == null)
                throw new InvalidOperationException($"Could not find an item of type {itemType}");
            return item;
        }

        public static Item Get(this IEnumerable<Item> items, ItemType itemType, World world)
        {
            var item = items.FirstOrDefault(i => i.Is(itemType, world));
            if (item == null)
                throw new InvalidOperationException($"Could not find an item of type {itemType} in world {world.Id}");
            return item;
        }

    }

}
