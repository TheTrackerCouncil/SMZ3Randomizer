using System;
using System.Linq;
using System.Reflection;

namespace Randomizer.SMZ3
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
            var attrib = itemType.GetType().GetCustomAttribute<ItemCategoryAttribute>(inherit: false);
            return attrib.Categories.Any(x => x == category);
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
            var attrib = itemType.GetType().GetCustomAttribute<ItemCategoryAttribute>(inherit: false);
            return attrib.Categories.Any(x => categories.Contains(x));
        }
    }
}
