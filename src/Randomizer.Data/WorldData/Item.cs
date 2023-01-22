using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Services;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData
{
    /// <summary>
    /// Represents an item.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class with the
        /// specified item type and world.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        /// <param name="world">The world the item is in.</param>
        /// <param name="name">The override name for the item</param>
        /// <param name="metadata">The metadata service to look up additional info about the item</param>
        /// <param name="trackerState">The tracking state of the run</param>
        /// <param name="isProgression">If this is a progression item or not</param>
        public Item(ItemType itemType, World world, string? name = null, IMetadataService? metadata = null, TrackerState? trackerState = null, bool isProgression = false)
        {
            Type = itemType;
            World = world;
            Name = string.IsNullOrEmpty(name) ? itemType.GetDescription() : name ;
            Metadata = metadata?.Item(itemType) ?? new ItemData(itemType);
            State = trackerState?.ItemStates.First(x => x.ItemName == Name && x.WorldId == world.Id) ?? new TrackerItemState();
            Progression = isProgression;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class with the
        /// specified item type and world.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        /// <param name="world">The world the item is in.</param>
        /// <param name="name">The override name for the item</param>
        /// <param name="metadata">The metadata object with additional info about the item</param>
        /// <param name="state">The tracking state of the item</param>
        /// <param name="isProgression">If this is a progression item or not</param>
        public Item(ItemType itemType, World world, string name, ItemData metadata, TrackerItemState state, bool isProgression = false)
        {
            Type = itemType;
            World = world;
            Name = name;
            Metadata = metadata;
            State = state;
            Progression = isProgression;
        }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of item.
        /// </summary>
        public ItemType Type { get; private set; }

        /// <summary>
        /// Indicates whether the item is an item required to make progress.
        /// </summary>
        public bool Progression { get; protected set; }

        /// <summary>
        /// Gets the world the item is located in.
        /// </summary>
        public World World { get; protected set; }

        /// <summary>
        /// Additional information about the item
        /// </summary>
        public ItemData Metadata { get; set; }

        /// <summary>
        /// Current state of the item
        /// </summary>
        public TrackerItemState State { get; set; }

        /// <summary>
        /// Indicates whether the item is a dungeon-specific item.
        /// </summary>
        public bool IsDungeonItem => Type.IsInAnyCategory(
            ItemCategory.SmallKey,
            ItemCategory.BigKey,
            ItemCategory.Compass,
            ItemCategory.Map);

        /// <summary>
        /// Indicates whether the item is a boss key.
        /// </summary>
        public bool IsBigKey => Type.IsInCategory(ItemCategory.BigKey);

        /// <summary>
        /// Indicates whether the item is a small key.
        /// </summary>
        public bool IsKey => Type.IsInCategory(ItemCategory.SmallKey);

        /// <summary>
        /// Indicates whether the item is a dungeon map.
        /// </summary>
        public bool IsMap => Type.IsInCategory(ItemCategory.Map);

        /// <summary>
        /// Indicates whether the item is a dungeon compass.
        /// </summary>
        public bool IsCompass => Type.IsInCategory(ItemCategory.Compass);

        /// <summary>
        /// Indicates whether the item is a keycard.
        /// </summary>
        public bool IsKeycard => Type.IsInCategory(ItemCategory.Keycard);

        /// <summary>
        /// Gets the number of actual items as displayed or mentioned by
        /// tracker, or <c>0</c> if the item does not have copies.
        /// </summary>
        public int Counter => State == null || Metadata == null
            ? 0
            : Metadata.Multiple && !Metadata.HasStages
                ? State.TrackingState * (Metadata.CounterMultiplier ?? 1)
                : 0;

        /// <summary>
        /// Tracks the item.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the item was tracked; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Tracking may fail if the item is already tracked, or if the item is
        /// already at the highest stage.
        /// </remarks>
        public bool Track()
        {
            if (State == null)
                throw new InvalidOperationException($"State not loaded for item '{Name}'");

            if (State.TrackingState == 0 // Item hasn't been tracked yet (any case)
                || (Metadata?.HasStages == false && Metadata?.Multiple == true) // State.Multiple items always track
                || (Metadata?.HasStages == true && State.TrackingState < Metadata?.MaxStage)) // Hasn't reached max. stage yet
            {
                State.TrackingState++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Untracks the item or decreases the item by one step.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the item was removed; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        public bool Untrack()
        {
            if (State == null)
                throw new InvalidOperationException($"State not loaded for item '{Name}'");

            if (State.TrackingState == 0)
                return false;

            State.TrackingState--;
            return true;
        }

        /// <summary>
        /// Marks the item at the specified stage.
        /// </summary>
        /// <param name="stage">The stage to set the item to.</param>
        /// <returns>
        /// <see langword="true"/> if the item was tracked; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Tracking may fail if the item is already at a higher stage.
        /// </remarks>
        public bool Track(int stage)
        {
            if (State == null)
                throw new InvalidOperationException($"State not loaded for item '{Name}'");

            if (Metadata?.HasStages == false)
                throw new ArgumentException($"The item '{Name}' does not have Multiple stages.");

            if (stage > Metadata?.MaxStage)
                throw new ArgumentOutOfRangeException($"Cannot advance item '{Name}' to stage {stage} as the highest state is {Metadata.MaxStage}.");

            if (State?.TrackingState < stage)
            {
                State.TrackingState = stage;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the item-specific phrases to respond with when tracking
        /// the item.
        /// </summary>
        /// <param name="response">
        /// When this method returns <c>true</c>, contains the possible phrases
        /// to respond with when tracking the item.
        /// </param>
        /// <returns>
        /// <c>true</c> if a response was configured for the item at the current
        /// tracking state; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetTrackingResponse([NotNullWhen(true)] out SchrodingersString? response)
        {
            if (Metadata == null || State == null)
                throw new InvalidOperationException($"State or metadata not loaded item '{Name}'");
            return Metadata.TryGetTrackingResponse(State.TrackingState, out response);
        }

        /// <summary>
        /// Determines whether the item is of the specified type and belongs to
        /// the specified world.
        /// </summary>
        /// <param name="type">The type of item to check.</param>
        /// <param name="world">The world the item belongs to.</param>
        /// <returns>
        /// <see langword="true"/> if the item is of the specified <paramref
        /// name="type"/> and <paramref name="world"/>; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        public bool Is(ItemType type, World world)
            => Type == type && World == world;

        /// <summary>
        /// Determines whether the item is of a different type or belongs to a
        /// different world.
        /// </summary>
        /// <param name="type">The type of item to check.</param>
        /// <param name="world">The world the item belongs to.</param>
        /// <returns>
        /// <see langword="true"/> if the item is not of the specified <paramref
        /// name="type"/> or <paramref name="world"/>; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        public bool IsNot(ItemType type, World world)
            => !Is(type, world);

        /// <summary>
        /// Determines if an item matches the type or name
        /// </summary>
        /// <param name="type">The type to compare against</param>
        /// <param name="name">The name to compare against if the item type is set to Nothing</param>
        /// <see langword="true"/> if the item matches the given type or name
        /// name="type"/> otherwise, <see langword="false"/>.
        public bool Is(ItemType type, string name)
            => (Type != ItemType.Nothing && Type == type) || (Type == ItemType.Nothing && Name == name);

        /// <summary>
        /// Updates an item in the middle of world generation
        /// </summary>
        /// <param name="type">The item type change to</param>
        public void UpdateItemType(ItemType type)
        {
            Type = type;
            Name = type.GetDescription();
        }

        /// <summary>
        /// Returns a string that represents the item.
        /// </summary>
        /// <returns>A string representing this item.</returns>
        public override string ToString() => $"{Name}";

        private static List<Item> Copies(int nr, Func<Item> template)
        {
            return Enumerable.Range(1, nr).Select(i => template()).ToList();
        }
    }
}
