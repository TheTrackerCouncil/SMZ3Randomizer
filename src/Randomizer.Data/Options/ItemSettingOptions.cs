using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;

namespace Randomizer.Data.Options;

/// <summary>
/// Class that represents a list of options for items, including items that should show up
/// early and items that the player should have in their starting inventory
/// </summary>
public class ItemSettingOptions
{
    /// <summary>
    /// The UI name of the item/category
    /// </summary>
    public string Item { get; set; }

    /// <summary>
    /// If this is a Metroid item or not
    /// </summary>
    public bool IsMetroid { get; set; }

    /// <summary>
    /// The list of selectable options for this category
    /// </summary>
    public List<ItemSettingOption> Options { get; set; }

    /// <summary>
    /// Constructor for the ItemSettingsOptions
    /// </summary>
    /// <param name="item">The UI name for the item/category</param>
    /// <param name="isMetroid">If this a metroid item or not</param>
    /// <param name="options">The list of dropdown options for this item/category</param>
    public ItemSettingOptions(string item, bool isMetroid, params ItemSettingOption[] options)
    {
        Item = item;
        IsMetroid = isMetroid;
        Options = options.ToList();
    }

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
        return new List<ItemSettingOptions>
        {
            new(item: "Bow", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Bow")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bow }
                },
                new ItemSettingOption("Early Bow and Silver Arrows")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bow, ItemType.SilverArrows }
                },
                new ItemSettingOption("Start with Bow")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x340 - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bow }
                },
                new ItemSettingOption("Start with Bow and Silver Arrows")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x340 - 0x340, 0x04) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bow, ItemType.SilverArrows }
                }
            ),
            new(item: "Boomerang", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Blue Boomerang")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.BlueBoomerang }
                },
                new ItemSettingOption("Early Red Boomerang")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.RedBoomerang }
                },
                new ItemSettingOption("Start with Blue Boomerang")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x341 - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.BlueBoomerang }
                },
                new ItemSettingOption("Start with Red Boomerang")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x341 - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.RedBoomerang }
                }
            ),
            new(item: "Hookshot", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Hookshot")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Hookshot }
                },
                new ItemSettingOption("Start with Hookshot")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x342 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Hookshot }
                }
            ),
            new(item: "Fire Rods", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Fire Rod")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Firerod }
                },
                new ItemSettingOption("Start with Fire Rod")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x345 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Firerod }
                }
            ),
            new(item: "Ice Rods", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Ice Rod")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Icerod }
                },
                new ItemSettingOption("Start with Ice Rod")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x346 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Icerod }
                }
            ),
            new(item: "Mushroom & Powder", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Mushroom")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Mushroom }
                },
                new ItemSettingOption("Early Magic Powder")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Powder }
                },
                new ItemSettingOption("Start with Magic Powder")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x344 - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Powder }
                }
            ),
            new(item: "Medallions", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Bombos")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bombos }
                },
                new ItemSettingOption("Early Ether")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Ether }
                },
                new ItemSettingOption("Early Quake")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Quake }
                },
                new ItemSettingOption("Early Medallions")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bombos, ItemType.Ether, ItemType.Quake }
                },
                new ItemSettingOption("Start with Bombos")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x347 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bombos }
                },
                new ItemSettingOption("Start with Ether")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x348 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Ether }
                },
                new ItemSettingOption("Start with Quake")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x349 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Quake }
                },
                new ItemSettingOption("Start with all Medallions")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x347 - 0x340, 0x01), (0x348 - 0x340, 0x01), (0x349 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bombos, ItemType.Ether, ItemType.Quake }
                }
            ),
            new(item: "Lamp", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Lamp")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Lamp }
                },
                new ItemSettingOption("Start with Lamp")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x34A - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Lamp }
                }
            ),
            new(item: "Hammer", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Hammer")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Hammer }
                },
                new ItemSettingOption("Start with Hammer")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x34B - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Hammer }
                }
            ),
            new(item: "Flute", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Flute")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Flute }
                },
                new ItemSettingOption("Start with Flute")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x34C - 0x340, 0x03) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Flute }
                }
            ),
            new(item: "Bug Net", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Bug Net")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bugnet }
                },
                new ItemSettingOption("Start with Bug Net")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x34D - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bugnet }
                }
            ),
            new(item: "Book of Mudora", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Book of Mudora")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Book }
                },
                new ItemSettingOption("Start with Book of Mudora")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x34E - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Book }
                }
            ),
            new(item: "Bottles", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Bottle")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bottle }
                },
                new ItemSettingOption("Start with 1 Bottle")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x34F - 0x340, 0x01), (0x35C - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bottle }
                },
                new ItemSettingOption("Start with 4 Bottles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x34F - 0x340, 0x04), (0x35C - 0x340, 0x02), (0x35D - 0x340, 0x02), (0x35E - 0x340, 0x02), (0x35F - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bottle, ItemType.Bottle, ItemType.Bottle, ItemType.Bottle }
                }
            ),
            new(item: "Cane of Somaria", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Cane of Somaria")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Somaria }
                },
                new ItemSettingOption("Start with Cane of Somaria")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x350 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Somaria }
                }
            ),
            new(item: "Cane of Byrna", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Cane of Byrna")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Somaria }
                },
                new ItemSettingOption("Start with Cane of Byrna")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x351 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Somaria }
                }
            ),
            new(item: "Magic Cape", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Magic Cape")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Cape }
                },
                new ItemSettingOption("Start with Magic Cape")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x352 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Cape }
                }
            ),
            new(item: "Mirror", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Mirror")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Mirror }
                },
                new ItemSettingOption("Start with Mirror")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x353 - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Mirror }
                }
            ),
            new(item: "Gloves", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Power Glove")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveGlove }
                },
                new ItemSettingOption("Early Tita's Mitts")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveGlove, ItemType.ProgressiveGlove }
                },
                new ItemSettingOption("Start with Power Glove")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x354 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveGlove }
                },
                new ItemSettingOption("Start with Titan's Mitt")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x354 - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveGlove, ItemType.ProgressiveGlove }
                }
            ),
            new(item: "Pegasus Boots", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Pegasus Boots")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Boots }
                },
                new ItemSettingOption("Start with Pegasus Boots")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x355 - 0x340, 0x01) , (0x379 - 0x340, 0xFC) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Boots }
                }
            ),
            new(item: "Flippers", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Flippers")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Flippers }
                },
                new ItemSettingOption("Start with Flippers")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x356 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Flippers }
                }
            ),
            new(item: "Moon Pearl", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Moon Pearl")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.MoonPearl }
                },
                new ItemSettingOption("Start with Moon Pearl")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x357 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.MoonPearl }
                }
            ),
            new(item: "Sword", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Sword")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveSword }
                },
                new ItemSettingOption("Start with Fighter's Sword")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x359 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveSword }
                },
                new ItemSettingOption("Start with Master Sword")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x359 - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveSword, ItemType.ProgressiveSword }
                },
                new ItemSettingOption("Start with Tempered Sword")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x359 - 0x340, 0x03) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveSword, ItemType.ProgressiveSword, ItemType.ProgressiveSword }
                },
                new ItemSettingOption("Start with Golden Sword")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x359 - 0x340, 0x04) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveSword, ItemType.ProgressiveSword, ItemType.ProgressiveSword, ItemType.ProgressiveSword }
                }
            ),
            new(item: "Shield", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Shield")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveShield }
                },
                new ItemSettingOption("Start with Blue Shield")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x35A - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveShield }
                },
                new ItemSettingOption("Start with Hero's Shield")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x35A - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveShield, ItemType.ProgressiveShield }
                },
                new ItemSettingOption("Start with Mirror Shield")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x35A - 0x340, 0x03) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveShield, ItemType.ProgressiveShield, ItemType.ProgressiveShield }
                }
            ),
            new(item: "Mail", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Mail")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveTunic }
                },
                new ItemSettingOption("Start with Blue Mail")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x35B - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveTunic }
                },
                new ItemSettingOption("Start with Red Mail")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x35B - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ProgressiveTunic, ItemType.ProgressiveTunic }
                }
            ),
            new(item: "Rupees", isMetroid: false,
                new ItemSettingOption("Start with 0 Rupees"),
                new ItemSettingOption("Start with 100 Rupees")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x360 - 0x340, 0x64), (0x362 - 0x340, 0x64) }
                },
                new ItemSettingOption("Start with 500 Rupees")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x360 - 0x340, 0xF4), (0x361 - 0x340, 0x01), (0x362 - 0x340, 0xF4), (0x363 - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ThreeHundredRupees }
                },
                new ItemSettingOption("Start with 1000 Rupees")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x360 - 0x340, 0xE8), (0x361 - 0x340, 0x03), (0x362 - 0x340, 0xE8), (0x363 - 0x340, 0x03) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ThreeHundredRupees, ItemType.ThreeHundredRupees }
                }
            ),
            new(item: "Hearts", isMetroid: false,
                new ItemSettingOption("Start with 3 Hearts"),
                new ItemSettingOption("Start with 5 Hearts")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x36C - 0x340, 0x40), (0x36D - 0x340, 0x40) }
                },
                new ItemSettingOption("Start with 10 Hearts")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x36C - 0x340, 0x80), (0x36D - 0x340, 0x80) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.HeartContainer, 6).Concat(Enumerable.Repeat(ItemType.HeartPiece, 16)).ToList()
                },
                new ItemSettingOption("Start with 20 Hearts")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x36C - 0x340, 0xA0), (0x36D - 0x340, 0xA0) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.HeartContainer, 10).Concat(Enumerable.Repeat(ItemType.HeartPiece, 24)).Concat(new List<ItemType>() { ItemType.HeartContainerRefill}).ToList()
                }
            ),
            new(item: "Zelda Ammo", isMetroid: false,
                new ItemSettingOption("Start with No Ammo"),
                new ItemSettingOption("Start with 10 Bombs")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x375 - 0x340, 0x0A) }
                },
                new ItemSettingOption("Start with 10 bombs, 30 arrows, and full magic")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x375 - 0x340, 0x0A), (0x376 - 0x340, 0x1E), (0x373 - 0x340, 0x80) }
                }
            ),
            new(item: "Half Magic", isMetroid: false,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Half Magic")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.HalfMagic }
                },
                new ItemSettingOption("Start with Half Magic")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x37B - 0x340, 0x01) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.HalfMagic }
                },
                new ItemSettingOption("Start with Quarter Magic")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0x37B - 0x340, 0x02) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.HalfMagic, ItemType.HalfMagic }
                }
            ),
            new(item: "Wave Beam", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Wave Beam")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Wave }
                },
                new ItemSettingOption("Start with Wave Beam")
                {
                    MemoryValues = new List<(int offset, int value)>{ (2, 0x0001) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Wave }
                }
            ),
            new(item: "Ice Beam", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Ice Beam")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Ice }
                },
                new ItemSettingOption("Start with Ice Beam")
                {
                    MemoryValues = new List<(int offset, int value)>{ (2, 0x0002) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Ice }
                }
            ),
            new(item: "Spazer Beam", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Spazer Beam")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Spazer }
                },
                new ItemSettingOption("Start with Spazer Beam")
                {
                    MemoryValues = new List<(int offset, int value)>{ (2, 0x0004) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Spazer }
                }
            ),
            new(item: "Plasma Beam", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Plasma Beam")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Plasma }
                },
                new ItemSettingOption("Start with Plasma Beam")
                {
                    MemoryValues = new List<(int offset, int value)>{ (2, 0x0008) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Plasma }
                }
            ),
            new(item: "Charge Beam", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Charge Beam")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Charge }
                },
                new ItemSettingOption("Start Charge Beam")
                {
                    MemoryValues = new List<(int offset, int value)>{ (2, 0x1000) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Charge }
                }
            ),
            new(item: "Varia Suit", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Varia Suit")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Varia }
                },
                new ItemSettingOption("Start with Varia Suit")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x0001) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Varia }
                }
            ),
            new(item: "Spring Ball", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Spring Ball")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.SpringBall }
                },
                new ItemSettingOption("Start with Spring Ball")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x0002) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.SpringBall }
                }
            ),
            new(item: "Morph Ball", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Morph Ball")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Morph }
                },
                new ItemSettingOption("Start with Morph Ball")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x0004) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Morph }
                }
            ),
            new(item: "Screw Attack", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Screw Attack")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.ScrewAttack }
                },
                new ItemSettingOption("Start with Screw Attack")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x0008) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.ScrewAttack }
                }
            ),
            new(item: "Gravity Suit", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Gravity Suit")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Gravity }
                },
                new ItemSettingOption("Start with Gravity Suit")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x0020) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Gravity }
                }
            ),
            new(item: "Hi Jump", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Hi Jump")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.HiJump }
                },
                new ItemSettingOption("Start with Hi Jump")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x0100) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.HiJump }
                }
            ),
            new(item: "Space Jump", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Space Jump")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.SpaceJump }
                },
                new ItemSettingOption("Start with Space Jump")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x0200) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.SpaceJump }
                }
            ),
            new(item: "Morph Bombs", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Morph Bombs")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bombs }
                },
                new ItemSettingOption("Start with Morph Bombs")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x1000) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Bombs }
                }
            ),
            new(item: "Speed Booster", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Speed Booster")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.SpeedBooster }
                },
                new ItemSettingOption("Start with Speed Booster")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x2000) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.SpeedBooster }
                }
            ),
            new(item: "Grapple Beam", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early Grapple Beam")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.Grapple }
                },
                new ItemSettingOption("Start with Grapple Beam")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x4000) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.Grapple }
                }
            ),
            new(item: "X-Ray Scope", isMetroid: true,
                new ItemSettingOption(ItemSettingOption.Randomized),
                new ItemSettingOption("Early X-Ray Scope")
                {
                    MatchingItemTypes = new List<ItemType>() { ItemType.XRay }
                },
                new ItemSettingOption("Start with X-Ray Scope")
                {
                    MemoryValues = new List<(int offset, int value)>{ (0, 0x8000) },
                    MatchingItemTypes = new List<ItemType>() { ItemType.XRay }
                }
            ),
            new(item: "Energy Tanks", isMetroid: true,
                new ItemSettingOption("Start with No Energy Tanks"),
                new ItemSettingOption("Start with 3 Energy Tanks")
                {
                    MemoryValues = new List<(int offset, int value)>{ (4, 399) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.ETank, 3).ToList()
                },
                new ItemSettingOption("Start with 7 Energy Tanks")
                {
                    MemoryValues = new List<(int offset, int value)>{ (4, 799) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.ETank, 7).ToList()
                },
                new ItemSettingOption("Start with 14 Energy Tanks")
                {
                    MemoryValues = new List<(int offset, int value)>{ (4, 1499) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.ETank, 14).ToList()
                }
            ),
            new(item: "Missiles", isMetroid: true,
                new ItemSettingOption("Start with No Missiles"),
                new ItemSettingOption("Start with 5 Missiles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (6, 5) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.Missile, 1).ToList()
                },
                new ItemSettingOption("Start with 20 Missiles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (6, 20) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.Missile, 4).ToList()
                },
                new ItemSettingOption("Start with 40 Missiles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (6, 40) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.Missile, 8).ToList()
                },
                new ItemSettingOption("Start with 100 Missiles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (6, 100) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.Missile, 20).ToList()
                },
                new ItemSettingOption("Start with 200 Missiles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (6, 200) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.Missile, 40).ToList()
                }
            ),
            new(item: "Super Missiles", isMetroid: true,
                new ItemSettingOption("Start with No Super Missiles"),
                new ItemSettingOption("Start with 5 Super Missiles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (8, 5) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.Super, 1).ToList()
                },
                new ItemSettingOption("Start with 20 Super Missiles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (8, 20) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.Super, 4).ToList()
                },
                new ItemSettingOption("Start with 40 Super Missiles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (8, 40) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.Super, 8).ToList()
                },
                new ItemSettingOption("Start with 80 Super Missiles")
                {
                    MemoryValues = new List<(int offset, int value)>{ (8, 80) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.Super, 16).ToList()
                }
            ),
            new(item: "Power Bombs", isMetroid: true,
                new ItemSettingOption("Start with No Power Bombs"),
                new ItemSettingOption("Start with 5 Power Bombs")
                {
                    MemoryValues = new List<(int offset, int value)>{ (10, 5) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.PowerBomb, 1).ToList()
                },
                new ItemSettingOption("Start with 10 Power Bombs")
                {
                    MemoryValues = new List<(int offset, int value)>{ (10, 10) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.PowerBomb, 2).ToList()
                },
                new ItemSettingOption("Start with 20 Power Bombs")
                {
                    MemoryValues = new List<(int offset, int value)>{ (10, 20) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.PowerBomb, 4).ToList()
                },
                new ItemSettingOption("Start with 50 Power Bombs")
                {
                    MemoryValues = new List<(int offset, int value)>{ (10, 50) },
                    MatchingItemTypes = Enumerable.Repeat(ItemType.PowerBomb, 10).ToList()
                }
            ),
        };
    }
}

/// <summary>
/// Class that represents a single option in a dropdown for an item/category
/// </summary>
public class ItemSettingOption
{
    public const string Randomized = "Randomized";

    /// <summary>
    /// The text to display in the dropdown
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// The list of memory offsets/values to modify
    /// </summary>
    public List<(int offset, int value)>? MemoryValues { get; set; }

    /// <summary>
    /// The list of item types that match this option
    /// </summary>
    public List<ItemType>? MatchingItemTypes { get; set; }

    public ItemSettingOption(string display)
    {
        Display = display;
    }
}
