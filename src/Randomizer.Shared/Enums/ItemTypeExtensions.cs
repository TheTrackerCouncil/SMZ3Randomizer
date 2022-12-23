using System;
using System.Linq;
using System.Reflection;

namespace Randomizer.Shared.Enums
{
    public static class ItemTypeExtensions
    {
        /// <summary>
        /// Determines whether the item type is in the specified category.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        /// <param name="category">The category to match.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="itemType"/> is in
        /// <paramref name="category"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsInCategory(this ItemType itemType, ItemCategory category)
        {
            return GetCategories(itemType).Any(x => x == category);
        }

        /// <summary>
        /// Determines whether the item type matches any of the specified
        /// categories.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        /// <param name="categories">The categories to match.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="itemType"/> matches any of
        /// <paramref name="categories"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsInAnyCategory(this ItemType itemType, params ItemCategory[] categories)
        {
            return GetCategories(itemType).Any(categories.Contains);
        }

        /// <summary>
        /// Determines the categories for the specified item type.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        /// <returns>
        /// A collection of categories for <paramref name="itemType"/>, or an
        /// empty array.
        /// </returns>
        public static ItemCategory[] GetCategories(this ItemType itemType)
        {
            var valueType = itemType.GetType().GetField(itemType.ToString());
            if (valueType != null)
            {
                var attrib = valueType.GetCustomAttribute<ItemCategoryAttribute>(inherit: false);
                if (attrib != null) return attrib.Categories;
            }

            return Array.Empty<ItemCategory>();
        }

        /// <summary>
        /// Determines whether the item type matches any of the specified
        /// categories.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        /// <param name="isZeldaKeysanity">If playing with Zelda Key Sanity enabled.</param>
        /// <param name="isMetroidKeysanity">If playing with Metroid Key Sanity enabled.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="itemType"/> matches any of
        /// could possibly be progression; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsPossibleProgression(this ItemType itemType, bool isZeldaKeysanity, bool isMetroidKeysanity)
        {
            if (itemType == ItemType.Nothing || itemType.IsInAnyCategory(new[] { ItemCategory.Junk, ItemCategory.Scam, ItemCategory.NonRandomized, ItemCategory.Map, ItemCategory.Compass, ItemCategory.Nice }))
                return false;

            if (itemType.IsInAnyCategory(new[] { ItemCategory.SmallKey, ItemCategory.BigKey }))
                return isZeldaKeysanity;

            if (itemType.IsInCategory(ItemCategory.Keycard))
                return isMetroidKeysanity;

            return true;
        }
    }
}
