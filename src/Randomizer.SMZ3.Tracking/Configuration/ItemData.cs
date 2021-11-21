using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Represents a trackable item.
    /// </summary>
    public class ItemData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemData"/> class with
        /// the specified item name and type.
        /// </summary>
        /// <param name="name">The possible names of the item.</param>
        /// <param name="internalItemType">
        /// The internal <see cref="ItemType"/> of the item.
        /// </param>
        public ItemData(SchrodingersString name, ItemType internalItemType)
        {
            Name = name;
            InternalItemType = internalItemType;
        }

        /// <summary>
        /// Gets the possible names for the item.
        /// </summary>
        public SchrodingersString Name { get; init; }

        /// <summary>
        /// Gets the grammatical article for the item (e.g. "a" or "the").
        /// </summary>
        public string? Article { get; init; }

        /// <summary>
        /// Gets the possible plural names for the item.
        /// </summary>
        public SchrodingersString? Plural { get; init; }

        /// <summary>
        /// Gets the name of the article, prefixed with "a", "the" or none,
        /// depending on the item.
        /// </summary>
        public string NameWithArticle => string.Join(" ",
            Article ?? (Multiple || HasStages ? "a" : "the"),
            Name);

        /// <summary>
        /// Gets the internal <see cref="ItemType"/> of the item.
        /// </summary>
        public ItemType InternalItemType { get; init; }

        /// <summary>
        /// Indicates whether the item can be tracked more than once.
        /// </summary>
        public bool Multiple { get; init; }

        /// <summary>
        /// Gets the number the item counter should be multiplied with, in the
        /// case of items that can be tracked more than once.
        /// </summary>
        public int? CounterMultiplier { get; init; }

        /// <summary>
        /// Gets the number of actual items as displayed or mentioned by
        /// tracker, or <c>0</c> if the item does not have copies.
        /// </summary>
        public int Counter => Multiple && !HasStages ? TrackingState * (CounterMultiplier ?? 1) : 0;

        /// <summary>
        /// Gets the stages and their names of a progressive item.
        /// </summary>
        public IReadOnlyDictionary<int, SchrodingersString>? Stages { get; init; }

        /// <summary>
        /// Gets or sets the zero-based index of the column in which the item
        /// should be displayed on the tracker.
        /// </summary>
        public int? Column { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of the column in which the item
        /// should be displayed on the tracker.
        /// </summary>
        public int? Row { get; set; }

        /// <summary>
        /// Gets or sets the path to the image to be displayed on the tracker.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets the highest stage the item supports, or 0 if the item does not
        /// have stages.
        /// </summary>
        public int MaxStage => HasStages ? Stages.Max(x => x.Key) : default;

        /// <summary>
        /// Indicates whether the item has stages.
        /// </summary>
        [MemberNotNullWhen(true, nameof(Stages))]
        public bool HasStages => Stages != null && Stages.Count > 0;

        /// <summary>
        /// Gets or sets the tracking state of the item.
        /// </summary>
        /// <remarks>
        /// For example, 0 represents an untracked item, 1 represents an
        /// obtained item and higher values indicate items that have been
        /// obtained more than once.
        /// </remarks>
        public int TrackingState { get; set; }

        /// <summary>
        /// Returns the stage of the item with the specifies name.
        /// </summary>
        /// <param name="name">The name of the stage.</param>
        /// <returns>
        /// The number of the stage with the given name, or <c>null</c> if the
        /// name does not match a stage.
        /// </returns>
        public int? GetStage(string name)
        {
            if (Stages?.Any(x => x.Value.Contains(name, StringComparison.OrdinalIgnoreCase)) == true)
                return Stages.Single(x => x.Value.Contains(name, StringComparison.OrdinalIgnoreCase)).Key;

            return null;
        }

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
            if (TrackingState == 0 // Item hasn't been tracked yet (any case)
                || (!HasStages && Multiple) // Multiple items always track
                || (HasStages && TrackingState < MaxStage)) // Hasn't reached max. stage yet
            {
                TrackingState++;
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
            if (TrackingState == 0)
                return false;

            TrackingState--;
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
            if (!HasStages)
                throw new ArgumentException($"The item '{Name}' does not have multiple stages.");

            if (stage > MaxStage)
                throw new ArgumentOutOfRangeException($"Cannot advance item '{Name}' to stage {stage} as the highest state is {MaxStage}.");

            if (TrackingState < stage)
            {
                TrackingState = stage;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the item is of the specified type.
        /// </summary>
        /// <param name="type">The type of item to check against.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="type"/> matches this item's type;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If this item's type is Nothing, the item is considered the same if
        /// it is a scam item.
        /// </remarks>
        public bool Is(ItemType type)
        {
            return InternalItemType == type
                || (InternalItemType == ItemType.Nothing
                    && type.IsInCategory(ItemCategory.Scam));
        }

        /// <summary>
        /// Returns a string representing the item.
        /// </summary>
        /// <returns>A string representing the item.</returns>
        public override string ToString() => Name[0];
    }
}
