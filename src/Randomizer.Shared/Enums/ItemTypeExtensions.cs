using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared
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
            return GetCategories(itemType).Any(x => categories.Contains(x));
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
            var attrib = valueType.GetCustomAttribute<ItemCategoryAttribute>(inherit: false);
            if (attrib == null)
                return Array.Empty<ItemCategory>();

            return attrib.Categories;
        }

        /// <summary>
        /// Returns true if this item type is a progression item type
        /// </summary>
        /// <param name="itemType">The item type to check</param>
        /// <returns>True if it's a progression item type, false otherwise</returns>
        public static bool IsProgression(this ItemType itemType)
        {
            return !(itemType.IsInAnyCategory(new[] { ItemCategory.Junk, ItemCategory.Scam, ItemCategory.Map, ItemCategory.Compass,
                    ItemCategory.SmallKey, ItemCategory.BigKey, ItemCategory.Keycard, ItemCategory.NonRandomized, ItemCategory.Nice  }) || itemType == ItemType.Nothing);
        }
    }
}
