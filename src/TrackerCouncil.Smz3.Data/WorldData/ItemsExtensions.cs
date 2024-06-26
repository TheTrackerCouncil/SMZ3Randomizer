using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.WorldData;

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
