using System.Collections.Generic;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Options;

/// <summary>
/// Represents user-configurable options for influencing seed generation.
/// </summary>
public class SeedOptions
{
    [YamlIgnore]
    public string Seed { get; set; } = "";

    public KeysanityMode KeysanityMode { get; set; } = KeysanityMode.None;

    public ItemPlacementRule ItemPlacementRule { get; set; } = ItemPlacementRule.Anywhere;

    public bool Race { get; set; }

    public bool DisableSpoilerLog { get; set; }

    public bool DisableTrackerHints { get; set; }

    public bool DisableTrackerSpoilers { get; set; }

    public bool DisableCheats { get; set; }

    public int? UniqueHintCount { get; set; }

    public int GanonsTowerCrystalCount { get; set; } = 7;
    public int GanonCrystalCount { get; set; } = 7;
    public bool OpenPyramid { get; set; } = false;
    public int TourianBossCount { get; set; } = 4;

    public IDictionary<LocationId, int> LocationItems { get; set; } = new Dictionary<LocationId, int>();

    public IDictionary<string, int> ItemOptions { get; set; } = new Dictionary<string, int>();
}
