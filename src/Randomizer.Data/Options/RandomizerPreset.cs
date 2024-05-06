using System;
using System.Collections.Generic;
using Randomizer.Data.Logic;
using Randomizer.Shared;

namespace Randomizer.Data.Options;

public class RandomizerPreset
{
    public required string PresetName { get; set; }
    public required Config? Config { get; set; }
    public string? FilePath { get; set; }
    public int Order { get; set; } = 1000;

    public static List<RandomizerPreset> GetDefaultPresets()
    {
        return
        [
            new RandomizerPreset()
            {
                PresetName = "Default - No Cas' logic or tricks are enabled.",
                Order = 0,
                Config = new Config()
            },
            new RandomizerPreset()
            {
                PresetName = "Beginner - All Cas' logic & patches, no required wall jumps, plentiful Zelda item drops.",
                Order = 1,
                Config = new Config()
                {
                    CasPatches = new CasPatches()
                    {
                        ZeldaDrops = ZeldaDrops.Easy
                    },
                    LogicConfig = new LogicConfig(true, false, WallJumpDifficulty.None)
                }
            },
            new RandomizerPreset()
            {
                PresetName = "Extra Cas' - All Cas' logic & patches, starting sword, pegasus boots, spazer, and hi-jump boots.",
                Order = 1,
                Config = new Config()
                {
                    CasPatches = new CasPatches(),
                    LogicConfig = new LogicConfig(true, false, WallJumpDifficulty.Medium),
                    ItemOptions = new Dictionary<string, int>()
                    {
                        { "Pegasus Boots", 2 },
                        { "Sword", 2 },
                        { "Spazer Beam", 2 },
                        { "Hi Jump", 2 },
                    }
                }
            },
            new RandomizerPreset()
            {
                PresetName = "Speedy - Ganon & Tourian open by default. Find needed resources and defeat both bosses.",
                Order = 1,
                Config = new Config()
                {
                    GanonCrystalCount = 0,
                    GanonsTowerCrystalCount = 0,
                    TourianBossCount = 0,
                    OpenPyramid = true
                }
            },
            new RandomizerPreset()
            {
                PresetName = "Challenging - No Cas' logic enabled, but all tricks are enabled. Hard wall jumps enabled.",
                Order = 1,
                Config = new Config()
                {
                    LogicConfig = new LogicConfig(false, true, WallJumpDifficulty.Hard)
                }
            }
        ];
    }
}
