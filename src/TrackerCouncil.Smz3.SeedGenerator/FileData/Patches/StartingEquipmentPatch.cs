using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

/// <summary>
/// Patch for setting the starting equipment in both games
/// </summary>
[SkipForParsedRoms]
public class StartingEquipmentPatch : RomPatch
{
    private static Dictionary<ItemType, List<ItemAddress>> s_classicItemAddresses = new()
    {
        {
            ItemType.Bow, [
                new ItemAddress { Address = 0x403000 },
                new ItemAddress { Address = 0x40304E, Value = 0b1000000, Bitflag = true }
            ]
        },
        { ItemType.SilverArrows, [new ItemAddress { Address = 0x40304E, Value = 0b01000000, Bitflag = true }] },
        {
            ItemType.BlueBoomerang, [
                new ItemAddress { Address = 0x403001 },
                new ItemAddress { Address = 0x40304C, Value = 0b1000000, Bitflag = true }
            ]
        },
        {
            ItemType.RedBoomerang, [
                new ItemAddress { Address = 0x403001, Value = 0x02 },
                new ItemAddress { Address = 0x40304C, Value = 0b0100000, Bitflag = true }
            ]
        },
        { ItemType.Hookshot, [new ItemAddress(address: 0x403002)] },
        {
            ItemType.ThreeBombs, [
                new ItemAddress { Address = 0x403003, Value = 0x03, Additive = true },
                new ItemAddress { Address = 0x40304D, Value = 0b00000010, Bitflag = true }
            ]
        },
        {
            ItemType.Mushroom, [
                new ItemAddress { Address = 0x403004, Value = 0x01 },
                new ItemAddress { Address = 0x40304C, Value = 0b00101000, Bitflag = true }
            ]
        },
        {
            ItemType.Powder, [
                new ItemAddress { Address = 0x403004, Value = 0x02 },
                new ItemAddress { Address = 0x40304C, Value = 0b00010000, Bitflag = true }
            ]
        },
        { ItemType.Firerod, [new ItemAddress { Address = 0x403005 }] },
        { ItemType.Icerod, [new ItemAddress { Address = 0x403006 }] },
        { ItemType.Bombos, [new ItemAddress { Address = 0x403007 }] },
        { ItemType.Ether, [new ItemAddress { Address = 0x403008 }] },
        { ItemType.Quake, [new ItemAddress { Address = 0x403009 }] },
        { ItemType.Lamp, [new ItemAddress { Address = 0x40300A }] },
        { ItemType.Hammer, [new ItemAddress { Address = 0x40300B }] },
        {
            ItemType.Shovel, [
                new ItemAddress { Address = 0x40300C },
                new ItemAddress { Address = 0x40304C, Value = 0b00000100, Bitflag = true }
            ]
        },
        {
            ItemType.Flute, [
                new ItemAddress { Address = 0x40300C, Value = 0x03 },
                new ItemAddress { Address = 0x40304C, Value = 0b00000001, Bitflag = true }
            ]
        },
        { ItemType.Bugnet, [new ItemAddress { Address = 0x40300D }] },
        { ItemType.Book, [new ItemAddress { Address = 0x40300E }] },
        { ItemType.Somaria, [new ItemAddress { Address = 0x403010 }] },
        { ItemType.Byrna, [new ItemAddress { Address = 0x403011 }] },
        { ItemType.Cape, [new ItemAddress { Address = 0x403012 }] },
        { ItemType.Mirror, [new ItemAddress { Address = 0x403013, Value = 0x02 }] },
        { ItemType.ProgressiveGlove, [new ItemAddress { Address = 0x403014, Value = 0x01, Additive = true }] },
        {
            ItemType.Boots, [
                new ItemAddress { Address = 0x403015 },
                new ItemAddress { Address = 0x403039, Value = 0b00000100, Bitflag = true }
            ]
        },
        {
            ItemType.Flippers, [
                new ItemAddress { Address = 0x403016 },
                new ItemAddress { Address = 0x403039, Value = 0b00000010, Bitflag = true }
            ]
        },
        { ItemType.MoonPearl, [new ItemAddress { Address = 0x403017 }] },
        { ItemType.ProgressiveSword, [new ItemAddress { Address = 0x400043, Value = 0x01, Additive = true }] },
        { ItemType.ProgressiveShield, [new ItemAddress { Address = 0x40301A, Value = 0x01, Additive = true }] },
        { ItemType.ProgressiveTunic, [new ItemAddress { Address = 0x40301B, Value = 0x01, Additive = true }] },
        {
            ItemType.Bottle,
            [new ItemAddress { Address = 0x40301C, Value = 0x02 }, new ItemAddress { Address = 0x40300F, Value = 0x01 }]
        },
        { ItemType.OneRupee, [new ItemAddress { Address = 0x403020, Value = 0x01, Additive = true }] },
        { ItemType.FiveRupees, [new ItemAddress { Address = 0x403020, Value = 0x05, Additive = true }] },
        { ItemType.TwentyRupees, [new ItemAddress { Address = 0x403020, Value = 0x14, Additive = true }] },
        { ItemType.TwentyRupees2, [new ItemAddress { Address = 0x403020, Value = 0x14, Additive = true }] },
        { ItemType.FiftyRupees, [new ItemAddress { Address = 0x403020, Value = 0x32, Additive = true }] },
        { ItemType.OneHundredRupees, [new ItemAddress { Address = 0x403020, Value = 0x64, Additive = true }] },
        { ItemType.ThreeHundredRupees, [new ItemAddress { Address = 0x403020, Value = 0x12C, Additive = true }] },
        { ItemType.Missile, [new ItemAddress { Address = 0xF26106, Value = 0x05, Additive = true }] },
        { ItemType.Super, [new ItemAddress { Address = 0xF26108, Value = 0x05, Additive = true }] },
        { ItemType.PowerBomb, [new ItemAddress { Address = 0xF2610A, Value = 0x05, Additive = true }] },
        { ItemType.Varia, [new ItemAddress { Address = 0xF26100, Value = 0x1, Bitflag = true }] },
        { ItemType.SpringBall, [new ItemAddress { Address = 0xF26100, Value = 0x2, Bitflag = true }] },
        { ItemType.Morph, [new ItemAddress { Address = 0xF26100, Value = 0x4, Bitflag = true }] },
        { ItemType.ScrewAttack, [new ItemAddress { Address = 0xF26100, Value = 0x8, Bitflag = true }] },
        { ItemType.Gravity, [new ItemAddress { Address = 0xF26100, Value = 0x20, Bitflag = true }] },
        { ItemType.HiJump, [new ItemAddress { Address = 0xF26100, Value = 0x100, Bitflag = true }] },
        { ItemType.SpaceJump, [new ItemAddress { Address = 0xF26100, Value = 0x200, Bitflag = true }] },
        { ItemType.Bombs, [new ItemAddress { Address = 0xF26100, Value = 0x1000, Bitflag = true }] },
        { ItemType.SpeedBooster, [new ItemAddress { Address = 0xF26100, Value = 0x2000, Bitflag = true }] },
        { ItemType.Grapple, [new ItemAddress { Address = 0xF26100, Value = 0x4000, Bitflag = true }] },
        { ItemType.XRay, [new ItemAddress { Address = 0xF26100, Value = 0x8000, Bitflag = true }] },
        { ItemType.Wave, [new ItemAddress { Address = 0xF26102, Value = 0x1, Bitflag = true }] },
        { ItemType.Ice, [new ItemAddress { Address = 0xF26102, Value = 0x2, Bitflag = true }] },
        { ItemType.Spazer, [new ItemAddress { Address = 0xF26102, Value = 0x4, Bitflag = true }] },
        { ItemType.Plasma, [new ItemAddress { Address = 0xF26102, Value = 0x8, Bitflag = true }] },
        { ItemType.Charge, [new ItemAddress { Address = 0xF26102, Value = 0x1000, Bitflag = true }] },
        { ItemType.ETank, [new ItemAddress { Address = 0xF26104, Value = 0x64, Additive = true }] },
    };

    /// <summary>
    /// Returns the changes to be applied to an SMZ3 ROM file.
    /// </summary>
    /// <param name="data">Patcher Data with the world and config information</param>
    /// <returns>
    /// A collection of changes, represented by the data to overwrite at the
    /// specified ROM offset.
    /// </returns>
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var itemSettingOptions = ItemSettingOptions.GetOptions();

        var zeldaData = Enumerable.Repeat((byte)0, 78).ToList();
        var metroidData = new Dictionary<int, List<int>>();

        var options = data.PlandoConfig.Items.Any()
            ? data.PlandoConfig.StartingInventory
            : data.Config.ItemOptions;

        foreach (var item in options)
        {
            var itemOptions = itemSettingOptions.FirstOrDefault(x => x.Item == item.Key);
            if (itemOptions == null || item.Value <= 0 || item.Value >= itemOptions.Options.Count) continue;
            var selectedOption = itemOptions.Options[item.Value];
            if (selectedOption.MemoryValues == null) continue;

            // For Metroid items we need to group items based on memory location because
            // equipment and beams are bit flags that need to be combined
            if (itemOptions.IsMetroid)
            {
                foreach (var patchData in selectedOption.MemoryValues)
                {
                    if (metroidData.ContainsKey(patchData.Key))
                    {
                        metroidData[patchData.Key].Add(patchData.Value);
                    }
                    else
                    {
                        metroidData[patchData.Key] = new List<int>() { patchData.Value };
                    }
                }
            }
            // For Zelda items, simply set the memory patches specified in the item option
            // However, we do them in one big chunk rather than individually
            else
            {
                foreach (var patchData in selectedOption.MemoryValues)
                {
                    zeldaData[patchData.Key] = (byte)patchData.Value;
                }

            }
        }

        foreach (var patchData in metroidData)
        {
            var valueTotal = patchData.Value.Aggregate(0, (current, value) => current | value);
            yield return new GeneratedPatch(Snes(0x81EF90 + patchData.Key), UshortBytes(valueTotal));
        }

        yield return new GeneratedPatch(Snes(0x30B000), zeldaData.ToArray());
    }

    public static Dictionary<ItemType, int> GetStartingItemList(byte[] rom, bool isCas)
    {
        if (isCas)
        {
            List<ItemType> items = [];

            foreach (var itemOptions in ItemSettingOptions.GetOptions())
            {
                if (itemOptions.IsMetroid)
                {
                    foreach (var itemSettingOption in itemOptions.Options.Where(x => x.MemoryValues?.Count > 0 && x.MatchingItemTypes?.Count > 0))
                    {
                        var memoryData = itemSettingOption.MemoryValues!.First();
                        var value = BitConverter.ToInt16(rom.Skip(Snes(0x81EF90 + memoryData.Key)).Take(2).ToArray());
                        if (itemSettingOption.MatchingItemTypes!.Count > 1 || memoryData.Value % 5 == 0)
                        {
                            if (value == memoryData.Value)
                            {
                                items.AddRange(itemSettingOption.MatchingItemTypes);
                                break;
                            }
                        }
                        else
                        {
                            if ((value & (short)memoryData.Value) == memoryData.Value)
                            {
                                items.AddRange(itemSettingOption.MatchingItemTypes);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var itemSettingOption in itemOptions.Options.Where(x => x.MemoryValues?.Count > 0 && x.MatchingItemTypes?.Count > 0))
                    {
                        var data = itemSettingOption.MemoryValues!.Select(x => ((byte)x.Value, rom[Snes(0x30B000) + x.Key])).ToList();
                        if (data.All(x => x.Item1 == x.Item2))
                        {
                            items.AddRange(itemSettingOption.MatchingItemTypes!);
                            break;
                        }
                    }
                }
            }

            return items.Distinct().ToDictionary(x => x, x => items.Count(i => i == x));
        }
        else
        {
            var toReturn = new Dictionary<ItemType, int>();

            foreach (var itemData in s_classicItemAddresses)
            {
                var itemType = itemData.Key;
                var addresses = itemData.Value;

                if (addresses.Any(x => x.Additive))
                {
                    var address = addresses.First(x => x.Additive);
                    var num = rom[Snes(address.Address)] / address.Value;
                    if (num > 0)
                    {
                        toReturn[itemType] = num;
                    }
                }
                else
                {
                    var allTrue = true;

                    foreach (var address in addresses)
                    {
                        if (address.Bitflag && itemType.IsInCategory(ItemCategory.Metroid))
                        {
                            var value = BitConverter.ToInt16(rom.Skip(Snes(address.Address)).Take(2).ToArray());
                            if ((value & address.Value) != address.Value)
                            {
                                allTrue = false;
                                break;
                            }
                        }
                        else if (address.Bitflag && itemType.IsInCategory(ItemCategory.Zelda))
                        {
                            var value = rom[Snes(address.Address)];
                            if ((value & address.Value) != address.Value)
                            {
                                allTrue = false;
                                break;
                            }
                        }
                        else if (rom[Snes(address.Address)] != address.Value)
                        {
                            allTrue = false;
                            break;
                        }
                    }

                    if (allTrue)
                    {
                        toReturn[itemType] = 1;
                    }
                }
            }

            return toReturn;
        }
    }
}

public record ItemAddress
{
    public ItemAddress()
    {
    }

    public ItemAddress(int address)
    {
        Address = address;
    }

    public int Address { get; init; }
    public int Value { get; init; } = 0x01;
    public bool Bitflag { get; init; }
    public bool Additive { get; init; }
}
