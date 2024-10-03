using System;

namespace TrackerCouncil.Smz3.Shared.Enums;

/// <summary>
/// Specifies the boss categories for a boss type.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class BossCategoryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="TrackerCouncil.Smz3.Shared.Enums.BossCategoryAttribute"/> class with the specified categories.
    /// </summary>
    /// <param name="categories">
    /// The categories to assign to the boss type.
    /// </param>
    public BossCategoryAttribute(params BossCategory[] categories)
    {
        Categories = categories;
    }

    /// <summary>
    /// Gets the categories for the type of boss.
    /// </summary>
    public BossCategory[] Categories { get; }
}
