using System.Collections.Generic;

namespace Randomizer.Data.Options;

public class RandomizerPreset
{
    public required string PresetName { get; set; }
    public required Config? Config { get; set; }
    public string? FilePath { get; set; }

    public static List<RandomizerPreset> GetDefaultPresets()
    {
        return
        [
            new RandomizerPreset()
            {
                PresetName = "Default",
                Config = new Config()
            }
        ];
    }
}
