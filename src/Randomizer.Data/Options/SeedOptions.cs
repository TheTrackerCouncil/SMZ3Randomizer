using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Randomizer.Shared;
using Randomizer.Shared.Enums;

namespace Randomizer.Data.Options
{
    /// <summary>
    /// Represents user-configurable options for influencing seed generation.
    /// </summary>
    public class SeedOptions
    {
        [JsonIgnore]
        public string Seed { get; set; } = "";

        [JsonIgnore]
        public string ConfigString { get; set; } = "";

        public KeysanityMode KeysanityMode { get; set; } = KeysanityMode.None;

        public ItemPlacementRule ItemPlacementRule { get; set; } = ItemPlacementRule.Anywhere;

        public bool Race { get; set; }

        public bool DisableSpoilerLog { get; set; }

        public bool DisableTrackerHints { get; set; }

        public bool DisableTrackerSpoilers { get; set; }

        public bool DisableCheats { get; set; }

        public int UniqueHintCount { get; set; } = 8;

        public int GanonsTowerCrystalCount { get; set; } = 7;
        public int GanonCrystalCount { get; set; } = 7;
        public bool OpenPyramid { get; set; } = false;
        public int TourianBossCount { get; set; } = 4;

        [JsonIgnore]
        public int MaxHints => 15;

        [JsonIgnore]
        public bool CopySeedAndRaceSettings { get; set; }

        public IDictionary<int, int> LocationItems { get; set; } = new Dictionary<int, int>();

        public IDictionary<string, int> ItemOptions { get; set; } = new Dictionary<string, int>();
    }
}
