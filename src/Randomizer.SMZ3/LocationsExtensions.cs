using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;

namespace Randomizer.SMZ3
{
    internal static class LocationsExtensions
    {
        public static Location Get(this IEnumerable<Location> locations, string name)
        {
            var location = locations.FirstOrDefault(l => l.Name == name);
            if (location == null)
                throw new ArgumentException($"Could not find location name {name}", nameof(name));
            return location;
        }

        /// <summary>
        /// Returns all locations that do not yet have items assigned to them.
        /// </summary>
        /// <param name="locations">The locations to filter.</param>
        /// <returns>A collection of locations without items.</returns>
        public static IEnumerable<Location> Empty(this IEnumerable<Location> locations)
        {
            return locations.Where(l => l.Item == null);
        }

        /// <summary>
        /// Returns all locations that already have an item assigned to them.
        /// </summary>
        /// <param name="locations"></param>
        /// <returns>A collection of locations with items.</returns>
        public static IEnumerable<Location> Filled(this IEnumerable<Location> locations)
        {
            return locations.Where(l => l.Item != null);
        }

        /// <summary>
        /// Returns all locations that can be accessed with the items from the
        /// same world.
        /// </summary>
        /// <param name="locations">The locations to filter.</param>
        /// <param name="items">The available items.</param>
        /// <returns>
        /// A collection of locations that can be accessed with
        /// <paramref name="items"/> from the same world as the locations
        /// themselves.
        /// </returns>
        public static IEnumerable<Location> AvailableWithinWorld(this IEnumerable<Location> locations, IEnumerable<Item> items, LogicConfig logicConfig)
        {
            return locations
                .Select(x => x.Region.World)
                .Distinct()
                .SelectMany(world => locations
                    .Where(l => l.Region.World == world)
                    .Available(items.Where(i => i.World == world), logicConfig));
        }

        /// <summary>
        /// Returns all locations that can be accessed with the specified set
        /// of items.
        /// </summary>
        /// <param name="locations">The locations to filter.</param>
        /// <param name="items">The available items.</param>
        /// <returns>
        /// A collection of locations that can be accessed with <paramref name="items"/>.
        /// </returns>
        public static IEnumerable<Location> Available(this IEnumerable<Location> locations, IEnumerable<Item> items, LogicConfig logicConfig)
        {
            var progression = new Progression(items, logicConfig);
            return locations.Where(l => l.IsAvailable(progression));
        }

        /// <summary>
        /// Determines the locations the specified item can be assigned to.
        /// </summary>
        /// <param name="locations">The locations to assign the item to.</param>
        /// <param name="item">The item to assign to a location.</param>
        /// <param name="items">The available items.</param>
        /// <returns>
        /// A collection of locations that <paramref name="item"/> can be
        /// assigned to based on the available items.
        /// </returns>
        public static IEnumerable<Location> CanFillWithinWorld(this IEnumerable<Location> locations, Item item, IEnumerable<Item> items, LogicConfig logicConfig)
        {
            var itemWorldProgression = new Progression(items.Where(i => i.World == item.World).Append(item), logicConfig);
            var worldProgression = locations
                .Select(x => x.Region.World)
                .Distinct()
                .ToDictionary(world => world.Id, world => new Progression(items.Where(i => i.World == world), logicConfig));

            return locations.Where(l =>
                l.CanFill(item, worldProgression[l.Region.World.Id])
                && item.World.Locations.Single(ll => ll.Id == l.Id).IsAvailable(itemWorldProgression));
        }

    }

}
