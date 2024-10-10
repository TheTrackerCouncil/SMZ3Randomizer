using System;

namespace TrackerCouncil.Smz3.Shared.Enums;

/// <summary>
/// Specifies the reward categories for a reward type.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class RewardCategoryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="TrackerCouncil.Smz3.Shared.Enums.RewardCategoryAttribute"/> class with the specified categories.
    /// </summary>
    /// <param name="categories">
    /// The categories to assign to the reward type.
    /// </param>
    public RewardCategoryAttribute(params RewardCategory[] categories)
    {
        Categories = categories;
    }

    /// <summary>
    /// Gets the categories for the type of reward.
    /// </summary>
    public RewardCategory[] Categories { get; }
}
