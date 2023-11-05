﻿using System.Collections.Generic;
using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.FileData.Patches;

/// <summary>
/// Patch for setting the starting equipment in both games
/// </summary>
public class StartingEquipmentPatch : RomPatch
{
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

}
