using System.Collections.Generic;
using System.ComponentModel;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Config file for misc settings that can be updated via config update or
/// don't fit with other configs
/// </summary>
[Description("Config file for misc settings that can be updated via config update or " +
             "don't fit with other configs")]
public class MetadataConfig : IMergeable<MetadataConfig>, IConfigFile<MetadataConfig>
{
    public Dictionary<string, string> PySpeechRecognitionReplacements { get; set; } = [];

    /// <summary>
    /// Returns default config
    /// </summary>
    /// <returns></returns>
    public static MetadataConfig Default()
    {
        return new MetadataConfig();
    }

    public static object Example()
    {
        return new MetadataConfig()
        {
            PySpeechRecognitionReplacements = new Dictionary<string, string> { { "brin star", "brinstar" } }
        };
    }
}
