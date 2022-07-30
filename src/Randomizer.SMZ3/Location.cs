﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Randomizer.Shared;

namespace Randomizer.SMZ3
{
    public delegate bool Requirement(Progression items);

    public delegate bool Verification(Item item, Progression items);

    /// <summary>
    /// Represents a location holding an item.
    /// </summary>
    public class Location
    {
        private readonly Requirement _canAccess;

        private Verification _alwaysAllow;

        private Verification _allow;

        private int? _weight;
 
#nullable enable
        public Location(Room room, int id, int romAddress, LocationType type, string name, string[]? alsoKnownAs = null, ItemType vanillaItem = ItemType.Nothing,
            Requirement? access = null, int? memoryAddress = null, int? memoryFlag = null, LocationMemoryType memoryType = LocationMemoryType.Default)
                    : this(room.Region, id, romAddress, type, name, alsoKnownAs, vanillaItem, access, memoryAddress, memoryFlag, memoryType)
        {
            Room = room;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Location"/> class with
        /// a specific access requirement in addition the requirements for the
        /// region itself.
        /// </summary>
        /// <param name="region">The region that contains this location.</param>
        /// <param name="id">The internal ID of the location.</param>
        /// <param name="romAddress">The byte address of the location.</param>
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
        /// <param name="memoryAddress">The address in memory to check to see if it's cleared</param>
        /// <param name="memoryFlag">The value to check at the memory address to see if it's cleared</param>
        /// <param name="memoryType">The type of location</param>

        public Location(Region region, int id, int romAddress, LocationType type, string name, string[]? alsoKnownAs = null, ItemType vanillaItem = ItemType.Nothing,
            Requirement? access = null, int? memoryAddress = null, int? memoryFlag = null, LocationMemoryType memoryType = LocationMemoryType.Default)
        {
            Region = region;
            Id = id;
            Name = name;
            Type = type;
            RomAddress = romAddress;
            AlternateNames = new ReadOnlyCollection<string>(alsoKnownAs ?? Array.Empty<string>());
            VanillaItem = vanillaItem;
            _canAccess = access ?? (_ => true);
            _alwaysAllow = (_, _) => false;
            _allow = (_, _) => true;
            MemoryAddress = memoryAddress;
            MemoryFlag = memoryFlag;
            MemoryType = memoryType;
        }
#nullable disable

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
        public int RomAddress { get; }

        /// <summary>
        /// Gets or sets the item that can be found in this location.
        /// </summary>
        public Item Item { get; set; }

        /// <summary>
        /// Gets the region the location is in.
        /// </summary>
        public Region Region { get; }

        /// <summary>
        /// Gets the room the location is in, if any.
        /// </summary>
        public Room Room { get; }

        /// <summary>
        /// The Logic to be used to determine if certain actions can be done
        /// </summary>
        public ILogic Logic => Region.Logic;

        /// <summary>
        /// Gets or sets a value indicating whether the item has been found.
        /// </summary>
        public bool Cleared { get; set; }

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
        /// The location in memory for detecting if the location was cleared
        /// </summary>
        public int? MemoryAddress { get; init; }

        /// <summary>
        /// The flag to determine if the location was cleared or not
        /// </summary>
        public int? MemoryFlag { get; init; }

        /// <summary>
        /// The type of location for accessing memory
        /// </summary>

        public LocationMemoryType MemoryType { get; init; }

        /// <summary>
        /// Gets the relative weight of this location, where negative values
        /// indicate easier to reach locations.
        /// </summary>
        public int Weight => _weight ?? Region.Weight;

        public bool ItemIs(ItemType type, World world) => Item?.Is(type, world) ?? false;

        public bool ItemIsNot(ItemType type, World world) => !ItemIs(type, world);

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
        /// <see langword="true"/> if the item is available with <paramref
        /// name="items"/>; otherwise, <see langword="false"/>.
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

        /// <summary>
        /// Returns a string that represents the location.
        /// </summary>
        /// <returns>A string that represents this location.</returns>
        public override string ToString()
        {
            return Room != null
                ? $"{Room} - {Name}"
                : $"{Region} - {Name}";
        }
    }
}
