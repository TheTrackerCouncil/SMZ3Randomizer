using System;
using System.Linq;
using System.Reflection;

namespace TrackerCouncil.Smz3.Shared.Enums;

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
    /// <param name="isLocalPlayerItem"></param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="itemType"/> matches any of
    /// could possibly be progression; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsPossibleProgression(this ItemType itemType, bool isZeldaKeysanity, bool isMetroidKeysanity, bool isLocalPlayerItem)
    {
        if (itemType.IsInAnyCategory(ItemCategory.SmallKey, ItemCategory.BigKey))
            return isZeldaKeysanity || !isLocalPlayerItem;

        if (itemType.IsInCategory(ItemCategory.Keycard))
            return isMetroidKeysanity || !isLocalPlayerItem;

        return itemType.IsInAnyCategory(ItemCategory.PossibleProgression);
    }

    /// <summary>
    /// Returns if the two item types are the same or have the same category (Eastern Palace Big Key & Big Key)
    /// </summary>
    /// <param name="itemType">The type of item</param>
    /// <param name="other">The type to compare to</param>
    /// <returns>If the two items are equivalent</returns>
    public static bool IsEquivalentTo(this ItemType itemType, ItemType? other)
    {
        if (itemType == other)
        {
            return true;
        }
        else if (other == null)
        {
            return false;
        }
        else if (itemType.IsInCategory(ItemCategory.Bottle) && other.Value.IsInCategory(ItemCategory.Bottle))
        {
            return true;
        }
        else if (itemType.IsInCategory(ItemCategory.SmallKey) && other.Value.IsInCategory(ItemCategory.SmallKey))
        {
            return true;
        }
        else if (itemType.IsInCategory(ItemCategory.BigKey) && other.Value.IsInCategory(ItemCategory.BigKey))
        {
            return true;
        }
        else if (itemType.IsInCategory(ItemCategory.Map) && other.Value.IsInCategory(ItemCategory.Map))
        {
            return true;
        }
        else if (itemType.IsInCategory(ItemCategory.Compass) && other.Value.IsInCategory(ItemCategory.Compass))
        {
            return true;
        }
        else if (itemType.IsInCategory(ItemCategory.KeycardL1) && other.Value.IsInCategory(ItemCategory.KeycardL1))
        {
            return true;
        }
        else if (itemType.IsInCategory(ItemCategory.KeycardL2) && other.Value.IsInCategory(ItemCategory.KeycardL2))
        {
            return true;
        }
        else if (itemType.IsInCategory(ItemCategory.KeycardBoss) && other.Value.IsInCategory(ItemCategory.KeycardBoss))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the generic form of the item type
    /// </summary>
    /// /// &lt;param name="itemType"&gt;The type of item&lt;/param&gt;
    public static ItemType GetGenericType(this ItemType itemType)
    {
        if (!itemType.IsInAnyCategory(ItemCategory.Bottle, ItemCategory.Compass, ItemCategory.Map,
                ItemCategory.SmallKey, ItemCategory.BigKey, ItemCategory.KeycardL1, ItemCategory.KeycardL2, ItemCategory.KeycardBoss))
        {
            return itemType;
        }
        else if (itemType.IsInCategory(ItemCategory.Bottle))
        {
            return ItemType.Bottle;
        }
        else if (itemType.IsInCategory(ItemCategory.Map))
        {
            return ItemType.Map;
        }
        else if (itemType.IsInCategory(ItemCategory.Compass))
        {
            return ItemType.Compass;
        }
        else if (itemType.IsInCategory(ItemCategory.SmallKey))
        {
            return ItemType.Key;
        }
        else if (itemType.IsInCategory(ItemCategory.KeycardL1))
        {
            return ItemType.CardL1;
        }
        else if (itemType.IsInCategory(ItemCategory.KeycardL2))
        {
            return ItemType.CardL2;
        }
        else if (itemType.IsInCategory(ItemCategory.KeycardBoss))
        {
            return ItemType.CardBoss;
        }
        else
        {
            return ItemType.BigKey;
        }
    }
}
