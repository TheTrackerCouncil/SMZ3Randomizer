using System.Collections.Generic;
using System.IO;

namespace TrackerCouncil.Smz3.Data.Configuration;

public class ConfigProfileDetails
{
    public ConfigProfileDetails(string configFolderPath, HashSet<string>? moods)
    {
        Name = new DirectoryInfo(configFolderPath).Name;
        ConfigFolderPath = configFolderPath;
        Moods = moods;

        if (Directory.Exists(Path.Combine(configFolderPath, "Sprites")))
        {
            SpritePath = Path.Combine(configFolderPath, "Sprites");
            HasSprites = Directory.GetFiles(SpritePath).Length > 0;
        }
    }

    public string Name { get; }
    public string ConfigFolderPath { get; set; }
    public HashSet<string>? Moods { get; }
    public string? SpritePath { get; }
    public bool HasSprites { get; }
}
