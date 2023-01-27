using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.Data.Options;

/// <summary>
/// Class that represents a single option in a dropdown for an item/category
/// </summary>
public class ItemSettingOption
{
    /// <summary>
    /// The text to display in the dropdown
    /// </summary>
    public string Display { get; set; } = null!;

    /// <summary>
    /// The list of memory offsets/values to modify
    /// </summary>
    public Dictionary<int, int>? MemoryValues { get; set; }

    /// <summary>
    /// The list of item types that match this option
    /// </summary>
    public List<ItemType>? MatchingItemTypes { get; set; }
}
