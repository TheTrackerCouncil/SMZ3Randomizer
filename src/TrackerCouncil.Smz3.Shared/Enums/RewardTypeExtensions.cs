using System;
using System.Linq;
using System.Reflection;

namespace TrackerCouncil.Smz3.Shared.Enums;

public static class RewardTypeExtensions
{
    /// <summary>
    /// Determines whether the reward type is in the specified category.
    /// </summary>
    /// <param name="rewardType">The type of reward.</param>
    /// <param name="category">The category to match.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="rewardType"/> is in
    /// <paramref name="category"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsInCategory(this RewardType rewardType, RewardCategory category)
    {
        return GetCategories(rewardType).Any(x => x == category);
    }

    /// <summary>
    /// Determines whether the reward type matches any of the specified
    /// categories.
    /// </summary>
    /// <param name="rewardType">The type of reward.</param>
    /// <param name="categories">The categories to match.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="rewardType"/> matches any of
    /// <paramref name="categories"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsInAnyCategory(this RewardType rewardType, params RewardCategory[] categories)
    {
        return GetCategories(rewardType).Any(categories.Contains);
    }

    /// <summary>
    /// Determines the categories for the specified reward type.
    /// </summary>
    /// <param name="rewardType">The type of reward.</param>
    /// <returns>
    /// A collection of categories for <paramref name="rewardType"/>, or an
    /// empty array.
    /// </returns>
    public static RewardCategory[] GetCategories(this RewardType rewardType)
    {
        var valueType = rewardType.GetType().GetField(rewardType.ToString());
        if (valueType != null)
        {
            var attrib = valueType.GetCustomAttribute<RewardCategoryAttribute>(inherit: false);
            if (attrib != null) return attrib.Categories;
        }

        return Array.Empty<RewardCategory>();
    }

}
