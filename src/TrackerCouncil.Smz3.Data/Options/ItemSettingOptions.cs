using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TrackerCouncil.Smz3.Shared.Enums;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TrackerCouncil.Smz3.Data.Options;

/// <summary>
/// Class that represents a list of options for items, including items that should show up
/// early and items that the player should have in their starting inventory
/// </summary>
public class ItemSettingOptions
{
    private static List<ItemSettingOptions>? s_itemSettingsOptions;

    /// <summary>
    /// The UI name of the item/category
    /// </summary>
    public string Item { get; set; } = null!;

    /// <summary>
    /// If this is a Metroid item or not
    /// </summary>
    public bool IsMetroid { get; set; }

    /// <summary>
    /// The list of selectable options for this category
    /// </summary>
    public List<ItemSettingOption> Options { get; set; } = null!;

    public ItemSettingOption SelectedOption { get; set; } = new();

    /// <summary>
    /// Retrieves what item types the user has requested to start with in their inventory
    /// </summary>
    /// <param name="config">The config of the player</param>
    /// <returns>The item types that the user will start with</returns>
    public static IEnumerable<ItemType> GetStartingItemTypes(Config config)
    {
        var selectedOptions = config.ItemOptions;
        var options = GetOptions().ToList();

        var toReturn = new List<ItemType>();

        foreach (var option in selectedOptions)
        {
            if (option.Key.StartsWith("ItemType:") && Enum.TryParse(option.Key[9..], out ItemType itemType))
            {
                toReturn.AddRange(Enumerable.Repeat(itemType, option.Value));
            }
            else
            {
                var settings =  options.FirstOrDefault(x => x.Item == option.Key && x.Options[option.Value].MemoryValues != null && x.Options[option.Value].MatchingItemTypes?.Count > 0);
                if (settings != null)
                {
                    toReturn.AddRange(settings.Options[option.Value].MatchingItemTypes!);
                }
            }
        }

        return toReturn;
    }

    /// <summary>
    /// Retrieves what items the user has requested to show early
    /// </summary>
    /// <param name="config">The config of the player</param>
    /// <returns>The item types that should show up early</returns>
    public static IEnumerable<ItemType> GetEarlyItemTypes(Config config)
    {
        var itemOptions = config.ItemOptions;
        var itemTypes = GetOptions()
            .Where(x => itemOptions.ContainsKey(x.Item)
                        && x.Options[itemOptions[x.Item]].MemoryValues == null
                        && x.Options[itemOptions[x.Item]].MatchingItemTypes != null)
            .SelectMany(x => x.Options[itemOptions[x.Item]].MatchingItemTypes!)
            .ToList();
        return itemTypes;
    }

    /// <summary>
    /// Retrieves a list of different items/categories and what options are available for them
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<ItemSettingOptions> GetOptions()
    {
        if (s_itemSettingsOptions != null)
        {
            return s_itemSettingsOptions;
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();

        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("TrackerCouncil.Smz3.Data.Options.ItemSettingOptions.yml") ?? throw new FileNotFoundException("Unable to find ItemSettingOptions.yml file");
        using var reader = new StreamReader(stream);
        var ymlText = reader.ReadToEnd();
        s_itemSettingsOptions = deserializer.Deserialize<List<ItemSettingOptions>>(ymlText);
        return s_itemSettingsOptions;
    }
}
