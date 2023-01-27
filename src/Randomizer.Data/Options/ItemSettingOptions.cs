using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Randomizer.Shared;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Randomizer.Data.Options;

/// <summary>
/// Class that represents a list of options for items, including items that should show up
/// early and items that the player should have in their starting inventory
/// </summary>
public class ItemSettingOptions
{
    private static readonly IDeserializer s_deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

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

    /// <summary>
    /// Retrieves what item types the user has requested to start with in their inventory
    /// </summary>
    /// <param name="config">The config of the player</param>
    /// <returns>The item types that the user will start with</returns>
    public static IEnumerable<ItemType> GetStartingItemTypes(Config config)
    {
        var itemOptions = config.ItemOptions;
        var itemTypes = GetOptions()
            .Where(x => itemOptions.ContainsKey(x.Item)
                        && x.Options[itemOptions[x.Item]].MemoryValues != null
                        && x.Options[itemOptions[x.Item]].MatchingItemTypes != null)
            .SelectMany(x => x.Options[itemOptions[x.Item]].MatchingItemTypes!)
            .ToList();
        return itemTypes;
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
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Randomizer.Data.Options.ItemSettingOptions.yml") ?? throw new FileNotFoundException("Unable to find ItemSettingOptions.yml file");
        using var reader = new StreamReader(stream);
        var ymlText = reader.ReadToEnd();
        return s_deserializer.Deserialize<List<ItemSettingOptions>>(ymlText);
    }
}
