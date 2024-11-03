using System;

namespace TrackerCouncil.Smz3.Shared.Enums;

/// <summary>
/// Specifies the item categories for an item type.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class ItemCategoryAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="ItemCategoryAttribute"/> class with the specified categories.
    /// </summary>
    /// <param name="categories">
    /// The categories to assign to the item type.
    /// </param>
    public ItemCategoryAttribute(params ItemCategory[] categories)
    {
        Categories = categories;
    }

    /// <summary>
    /// Gets the categories for the type of item.
    /// </summary>
    public ItemCategory[] Categories { get; }
}
