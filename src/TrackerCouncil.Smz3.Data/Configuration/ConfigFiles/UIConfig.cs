using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Represents a config file that houses different layouts for the Tracker UI
/// </summary>
[Description("Config file for the tracker window UI layouts showing items, dungeons, and bosses")]
public class UIConfig : List<UILayout>, IConfigFile<UIConfig>, IMergeable<UILayout>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public UIConfig() { }

    /// <summary>
    /// Builds the default Tracker UI layouts
    /// </summary>
    /// <returns>A collection of layouts</returns>
    public static UIConfig Default()
    {
        return new UIConfig();
    }

    public static object Example()
    {
        return new UIConfig
        {
            new("Basic Example", new List<UIGridLocation>()
            {
                new()
                {
                    Type = UIGridLocationType.Items,
                    Row = 1,
                    Column = 1,
                    Identifiers = new List<string>() { "Hookshot" }
                },
                new()
                {
                    Type = UIGridLocationType.Items,
                    Row = 1,
                    Column = 2,
                    Identifiers = new List<string>() { "Bow", "Silver Arrows" }
                },
                new()
                {
                    Type = UIGridLocationType.Dungeon,
                    Row = 2,
                    Column = 1,
                    Identifiers = new List<string>() { "Desert Palace" }
                },
                new()
                {
                    Type = UIGridLocationType.SMBoss,
                    Row = 2,
                    Column = 2,
                    Identifiers = new List<string>() { "Kraid" }
                },
            }),
            new("Basic Keysanity Example",
                new List<UIGridLocation>()
                {
                    new()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 1,
                        Identifiers =
                            new List<string>()
                            {
                                "Desert Palace Key", "Desert Palace Big Key", "Desert Palace Map"
                            }
                    },
                    new()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 2,
                        Image = "CR.png",
                        Identifiers = new List<string>()
                        {
                            "Crateria Level 1 Keycard", "Crateria Level 2 Keycard", "Crateria Boss Keycard"
                        }
                    },
                })
        };
    }
}
