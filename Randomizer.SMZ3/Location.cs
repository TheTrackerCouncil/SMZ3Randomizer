using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Randomizer.SMZ3
{
    public delegate bool Requirement(Progression items);

    public delegate bool Verification(Item item, Progression items);

    /// <summary>
    /// Represents a location holding an item.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets the internal identifier of the location.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the name of the location.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of location.
        /// </summary>
        public LocationType Type { get; }

        /// <summary>
        /// Gets the byte address of the location.
        /// </summary>
        public int Address { get; }

        /// <summary>
        /// Gets or sets the item that can be found in this location.
        /// </summary>
        public Item Item { get; set; }

        /// <summary>
        /// Gets the region the location is in.
        /// </summary>
        public Region Region { get; }

        /// <summary>
        /// Gets any alternate names for the location.
        /// </summary>
        public IReadOnlyCollection<string> AlternateNames { get; }

        /// <summary>
        /// Gets the item that can be found in this location in the vanilla
        /// game.
        /// </summary>
        public ItemType VanillaItem { get; }

        /// <summary>
        /// Gets the relative weight of this location, where negative values
        /// indicate easier to reach locations.
        /// </summary>
        public int Weight => _weight ?? Region.Weight;

        private readonly Requirement _canAccess;
        private Verification _alwaysAllow;
        private Verification _allow;
        private int? _weight;

        public bool ItemIs(ItemType type, World world) => Item?.Is(type, world) ?? false;
        public bool ItemIsNot(ItemType type, World world) => !ItemIs(type, world);

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class that
        /// is always considered accessible if the region can be entered.
        /// </summary>
        /// <param name="region">The region that contains this location.</param>
        /// <param name="id">The internal ID of the location.</param>
        /// <param name="address">The byte address of the location.</param>
        /// <param name="type">The type of location.</param>
        /// <param name="name">The name of the location.</param>
        public Location(Region region, int id, int address, LocationType type, string name)
            : this(region, id, address, type, name, Array.Empty<string>(), ItemType.Nothing, _ => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class that
        /// is always considered accessible if the region can be entered.
        /// </summary>
        /// <param name="region">The region that contains this location.</param>
        /// <param name="id">The internal ID of the location.</param>
        /// <param name="address">The byte address of the location.</param>
        /// <param name="type">The type of location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="vanillaItem">
        /// The item that can be found in this location in the regular game.
        /// </param>
        public Location(Region region, int id, int address, LocationType type, string name, ItemType vanillaItem)
            : this(region, id, address, type, name, Array.Empty<string>(), vanillaItem, _ => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class that
        /// is always considered accessible if the region can be entered.
        /// </summary>
        /// <param name="region">The region that contains this location.</param>
        /// <param name="id">The internal ID of the location.</param>
        /// <param name="address">The byte address of the location.</param>
        /// <param name="type">The type of location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="alsoKnownAs">
        /// An alternative name for the item or location.
        /// </param>
        /// <param name="vanillaItem">
        /// The item that can be found in this location in the regular game.
        /// </param>
        public Location(Region region, int id, int address, LocationType type, string name, string alsoKnownAs, ItemType vanillaItem)
            : this(region, id, address, type, name, new[] { alsoKnownAs }, vanillaItem, _ => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class that
        /// is always considered accessible if the region can be entered.
        /// </summary>
        /// <param name="region">The region that contains this location.</param>
        /// <param name="id">The internal ID of the location.</param>
        /// <param name="address">The byte address of the location.</param>
        /// <param name="type">The type of location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="alsoKnownAs">
        /// A collection of alternate names for the item or location.
        /// </param>
        /// <param name="vanillaItem">
        /// The item that can be found in this location in the regular game.
        /// </param>
        public Location(Region region, int id, int address, LocationType type, string name, string[] alsoKnownAs, ItemType vanillaItem)
            : this(region, id, address, type, name, alsoKnownAs, vanillaItem, _ => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class with
        /// a specific access requirement in addition the requirements for the
        /// region itself.
        /// </summary>
        /// <param name="region">The region that contains this location.</param>
        /// <param name="id">The internal ID of the location.</param>
        /// <param name="address">The byte address of the location.</param>
        /// <param name="type">The type of location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="access">
        /// The requirement for being able to access the location.
        /// </param>
        public Location(Region region, int id, int address, LocationType type, string name, Requirement access)
            : this(region, id, address, type, name, Array.Empty<string>(), ItemType.Nothing, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class with
        /// a specific access requirement in addition the requirements for the
        /// region itself.
        /// </summary>
        /// <param name="region">The region that contains this location.</param>
        /// <param name="id">The internal ID of the location.</param>
        /// <param name="address">The byte address of the location.</param>
        /// <param name="type">The type of location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="vanillaItem">
        /// The item that can be found in this location in the regular game.
        /// </param>
        /// <param name="access">
        /// The requirement for being able to access the location.
        /// </param>
        public Location(Region region, int id, int address, LocationType type, string name, ItemType vanillaItem, Requirement access)
            : this(region, id, address, type, name, Array.Empty<string>(), vanillaItem, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class with
        /// a specific access requirement in addition the requirements for the
        /// region itself.
        /// </summary>
        /// <param name="region">The region that contains this location.</param>
        /// <param name="id">The internal ID of the location.</param>
        /// <param name="address">The byte address of the location.</param>
        /// <param name="type">The type of location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="alsoKnownAs">
        /// An alternative name for the item or location.
        /// </param>
        /// <param name="vanillaItem">
        /// The item that can be found in this location in the regular game.
        /// </param>
        /// <param name="access">
        /// The requirement for being able to access the location.
        /// </param>
        public Location(Region region, int id, int address, LocationType type, string name, string alsoKnownAs, ItemType vanillaItem, Requirement access)
            : this(region, id, address, type, name, new[] { alsoKnownAs }, vanillaItem, access)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class with
        /// a specific access requirement in addition the requirements for the
        /// region itself.
        /// </summary>
        /// <param name="region">The region that contains this location.</param>
        /// <param name="id">The internal ID of the location.</param>
        /// <param name="address">The byte address of the location.</param>
        /// <param name="type">The type of location.</param>
        /// <param name="name">The name of the location.</param>
        /// <param name="alsoKnownAs">
        /// A collection of alternate names for the item or location.
        /// </param>
        /// <param name="vanillaItem">
        /// The item that can be found in this location in the regular game.
        /// </param>
        /// <param name="access">
        /// The requirement for being able to access the location.
        /// </param>
        public Location(Region region, int id, int address, LocationType type, string name, string[] alsoKnownAs, ItemType vanillaItem, Requirement access)
        {
            Region = region;
            Id = id;
            Name = name;
            Type = type;
            Address = address;
            AlternateNames = new ReadOnlyCollection<string>(alsoKnownAs);
            VanillaItem = vanillaItem;
            _canAccess = access;
            _alwaysAllow = (_, _) => false;
            _allow = (_, _) => true;
        }

        /// <summary>
        /// Applies a weight to this location, indicating whether the location
        /// is considered early (negative values) or not.
        /// </summary>
        /// <param name="weight">The weight to apply to the location.</param>
        /// <returns>The location.</returns>
        public Location Weighted(int? weight)
        {
            _weight = weight;
            return this;
        }

        /// <summary>
        /// Applies a filter to the items that can be assigned to this location.
        /// If <paramref name="allow"/> returns true, the item is allowed
        /// regardless of other conditions that normally apply.
        /// </summary>
        /// <param name="allow">
        /// The condition for an item to always be allowed in this location,
        /// regardless of other conditions.
        /// </param>
        /// <returns>The location.</returns>
        public Location AlwaysAllow(Verification allow)
        {
            _alwaysAllow = allow;
            return this;
        }

        /// <summary>
        /// Applies a filter to the items that can be assigned to this location.
        /// </summary>
        /// <param name="allow">
        /// The condition for an item to be allowed in this lcoation.
        /// </param>
        /// <returns>The location.</returns>
        public Location Allow(Verification allow)
        {
            _allow = allow;
            return this;
        }

        /// <summary>
        /// Determines whether the item is accessible with the specified items.
        /// </summary>
        /// <param name="items">The available items.</param>
        /// <returns>
        /// <see langword="true"/> if the item is available with
        /// <paramref name="items"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsAvailable(Progression items)
        {
            return Region.CanEnter(items) && _canAccess(items);
        }

        /// <summary>
        /// Determines whether the specified item can be assigned to this
        /// location.
        /// </summary>
        /// <param name="item">The item to assign to this location.</param>
        /// <param name="items">The available items.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> can be assigned to
        /// this location; otherwise, <see langword="false"/>.
        /// </returns>
        public bool CanFill(Item item, Progression items)
        {
            var oldItem = Item;
            Item = item;
            var fillable = _alwaysAllow(item, items) || (Region.CanFill(item, items) && _allow(item, items) && IsAvailable(items));
            Item = oldItem;
            return fillable;
        }
    }

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
        public static IEnumerable<Location> AvailableWithinWorld(this IEnumerable<Location> locations, IEnumerable<Item> items)
        {
            return locations
                .Select(x => x.Region.World)
                .Distinct()
                .SelectMany(world => locations
                    .Where(l => l.Region.World == world)
                    .Available(items.Where(i => i.World == world)));
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
        public static IEnumerable<Location> Available(this IEnumerable<Location> locations, IEnumerable<Item> items)
        {
            var progression = new Progression(items);
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
        public static IEnumerable<Location> CanFillWithinWorld(this IEnumerable<Location> locations, Item item, IEnumerable<Item> items)
        {
            var itemWorldProgression = new Progression(items.Where(i => i.World == item.World).Append(item));
            var worldProgression = locations
                .Select(x => x.Region.World)
                .Distinct()
                .ToDictionary(world => world.Id, world => new Progression(items.Where(i => i.World == world)));

            return locations.Where(l =>
                l.CanFill(item, worldProgression[l.Region.World.Id])
                && item.World.Locations.Find(ll => ll.Id == l.Id).IsAvailable(itemWorldProgression));
        }

    }

}
