using System;
using System.Linq;
using System.Reflection;

namespace TrackerCouncil.Smz3.Shared.Enums;

public static class BossTypeExtensions
{
    /// <summary>
    /// Determines whether the boss type is in the specified category.
    /// </summary>
    /// <param name="bossType">The type of boss.</param>
    /// <param name="category">The category to match.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="bossType"/> is in
    /// <paramref name="category"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsInCategory(this BossType bossType, BossCategory category)
    {
        return GetCategories(bossType).Any(x => x == category);
    }

    /// <summary>
    /// Determines whether the boss type matches any of the specified
    /// categories.
    /// </summary>
    /// <param name="bossType">The type of boss.</param>
    /// <param name="categories">The categories to match.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="bossType"/> matches any of
    /// <paramref name="categories"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsInAnyCategory(this BossType bossType, params BossCategory[] categories)
    {
        return GetCategories(bossType).Any(categories.Contains);
    }

    /// <summary>
    /// Determines the categories for the specified boss type.
    /// </summary>
    /// <param name="bossType">The type of boss.</param>
    /// <returns>
    /// A collection of categories for <paramref name="bossType"/>, or an
    /// empty array.
    /// </returns>
    public static BossCategory[] GetCategories(this BossType bossType)
    {
        var valueType = bossType.GetType().GetField(bossType.ToString());
        if (valueType != null)
        {
            var attrib = valueType.GetCustomAttribute<BossCategoryAttribute>(inherit: false);
            if (attrib != null) return attrib.Categories;
        }

        return Array.Empty<BossCategory>();
    }

}
