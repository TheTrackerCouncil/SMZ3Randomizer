using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Randomizer.Shared;
using SharpYaml.Serialization;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
{
    /// <summary>
    /// Represents a trackable item.
    /// </summary>
    public class ItemData : IMergeable<ItemData>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ItemData()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemData"/> class with
        /// the specified item name and type.
        /// </summary>
        /// <param name="name">The possible names of the item.</param>
        /// <param name="internalItemType">
        /// The internal <see cref="ItemType"/> of the item.
        /// </param>
        public ItemData(SchrodingersString name, ItemType internalItemType, SchrodingersString hints)
        {
            Item = internalItemType.GetDescription();
            Name = name;
            InternalItemType = internalItemType;
            Hints = hints;
        }

        /// <summary>
        /// Unique key to connect the ItemData with other configs
        /// </summary>
        [MergeKey]
        public string Item { get; set; }

        /// <summary>
        /// Gets the possible names for the item.
        /// </summary>
        public SchrodingersString Name { get; set; }

        /// <summary>
        /// Gets the grammatical article for the item (e.g. "a" or "the").
        /// </summary>
        public string? Article { get; set; }

        /// <summary>
        /// Gets the possible plural names for the item.
        /// </summary>
        public SchrodingersString? Plural { get; set; }

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
        public ItemType InternalItemType { get; set; }

        /// <summary>
        /// Indicates whether the item can be tracked more than once.
        /// </summary>
        public bool Multiple { get; set; }

        /// <summary>
        /// Gets the number the item counter should be multiplied with, in the
        /// case of items that can be tracked more than once.
        /// </summary>
        public int? CounterMultiplier { get; set; }

        /// <summary>
        /// Gets the number of actual items as displayed or mentioned by
        /// tracker, or <c>0</c> if the item does not have copies.
        /// </summary>
        public int Counter => Multiple && !HasStages ? TrackingState * (CounterMultiplier ?? 1) : 0;

        /// <summary>
        /// Gets the stages and their names of a progressive item.
        /// </summary>
        public Dictionary<int, SchrodingersString>? Stages { get; set; }

        /// <summary>
        /// Gets the phrases to respond with when tracking this item, or
        /// <c>null</c> to use the generic item responses. The dictionary key
        /// represents the current <see cref="TrackingState"/>.
        /// </summary>
        /// <remarks>
        /// <c>{0}</c> is a placeholder for the item counter.
        /// <para>
        /// If the key is missing, it will fall back to the closest smaller key.
        /// Use <c>null</c> to reset to the default item responses.
        /// </para>
        /// </remarks>
        public Dictionary<int, SchrodingersString>? WhenTracked { get; set; }

        /// <summary>
        /// Gets or sets the path to the image to be displayed on the tracker.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets the possible hints for the item, if any are defined.
        /// </summary>
        public SchrodingersString? Hints { get; set; }

        /// <summary>
        /// Gets the highest stage the item supports, or 1 if the item does not
        /// have stages, or 0 if the item has no limit.
        /// </summary>
        public int MaxStage => HasStages ? Stages.Max(x => x.Key) : Multiple ? 0 : 1;

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
                || !HasStages && Multiple // Multiple items always track
                || HasStages && TrackingState < MaxStage) // Hasn't reached max. stage yet
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
                || InternalItemType == ItemType.Nothing
                    && type.IsInCategory(ItemCategory.Scam)
                || InternalItemType == ItemType.HeartContainer
                    && type == ItemType.HeartContainerRefill
                || InternalItemType == ItemType.HeartContainerRefill
                    && type == ItemType.HeartContainer;
        }

        /// <summary>
        /// Returns a string representing the item.
        /// </summary>
        /// <returns>A string representing the item.</returns>
        public override string ToString() => Name[0];

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
            if (WhenTracked == null)
            {
                response = null;
                return false;
            }

            if (WhenTracked.TryGetValue(TrackingState, out response))
                return response != null;

            var smallerKeys = WhenTracked.Keys.TakeWhile(x => x < TrackingState).OrderBy(x => x);
            if (!smallerKeys.Any())
            {
                response = null;
                return false;
            }

            var closestSmallerKey = smallerKeys.Last();
            if (WhenTracked.TryGetValue(closestSmallerKey, out response))
                return response != null;

            response = null;
            return false;
        }

        /// <summary>
        /// Determines whether the item is worth getting given the specified
        /// configuration, assuming one is provided.
        /// </summary>
        /// <param name="config">The randomizer configuration.</param>
        /// <returns>
        /// <c>true</c> if the item is considered good; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method only considers the item's value on its own. Call <see
        /// cref="Tracker.IsWorth(ItemData)"/> to include items that this item
        /// might logically lead to.
        /// </remarks>
        public bool IsGood(Config? config) => !IsJunk(config);

        /// <summary>
        /// Determines whether the item is junk given the specified
        /// configuration, assuming one is provided.
        /// </summary>
        /// <param name="config">The randomizer configuration.</param>
        /// <returns>
        /// <c>true</c> if the item is considered junk; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method only considers the item's value on its own. Call <see
        /// cref="Tracker.IsWorth(ItemData)"/> to include items that this item
        /// might logically lead to.
        /// </remarks>
        public bool IsJunk(Config? config)
        {
            if (InternalItemType == ItemType.Nothing || InternalItemType.IsInAnyCategory(new[] { ItemCategory.Junk, ItemCategory.Scam, ItemCategory.NonRandomized, ItemCategory.Compass }))
                return true;

            if (config?.ZeldaKeysanity == false && InternalItemType.IsInAnyCategory(new[] { ItemCategory.SmallKey, ItemCategory.BigKey, ItemCategory.Map }))
                return true;

            if (config?.MetroidKeysanity == false && InternalItemType.IsInCategory(ItemCategory.Keycard))
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether the item is progression given the specified
        /// configuration, assuming one is provided.
        /// </summary>
        /// <param name="config">The randomizer configuration.</param>
        /// <returns>
        /// <c>true</c> if the item is considered progression; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method only considers the item's value on its own. Call <see
        /// cref="Tracker.IsWorth(ItemData)"/> to include items that this item
        /// might logically lead to.
        /// </remarks>
        public bool IsProgression(Config? config)
        {
            if (InternalItemType == ItemType.Nothing || InternalItemType.IsInAnyCategory(new[] { ItemCategory.Junk, ItemCategory.Scam, ItemCategory.NonRandomized, ItemCategory.Map, ItemCategory.Compass, ItemCategory.Nice }))
                return false;

            if (InternalItemType.IsInAnyCategory(new[] { ItemCategory.SmallKey, ItemCategory.BigKey }))
                return config?.ZeldaKeysanity == true;

            if (InternalItemType.IsInCategory(ItemCategory.Keycard))
                return config?.MetroidKeysanity == true;

            // Todo: We can add special logic like checking if it's one of the first two swords

            return true;
        }

        /// <summary>
        /// Determines whether the item is a dungeon item such as a key or map
        /// </summary>
        /// <returns>True if the item is a dungeon item</returns>
        public bool IsDungeonItem()
        {
            return InternalItemType.IsInAnyCategory(new[] { ItemCategory.SmallKey, ItemCategory.BigKey, ItemCategory.Map, ItemCategory.Compass });
        }
    }
}
