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
using Randomizer.SMZ3;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// Represents user-configurable options for influencing seed generation.
    /// </summary>
    public class SeedOptions
    {
        [JsonIgnore]
        public string Seed { get; set; }

        [JsonIgnore]
        public string ConfigString { get; set; }

        public ItemPlacement SwordLocation { get; set; }

        public ItemPlacement MorphLocation { get; set; }

        public ItemPlacement MorphBombsLocation { get; set; }

        public ItemPlacement PegasusBootsLocation { get; set; }

        public ItemPlacement SpaceJumpLocation { get; set; }

        public ItemPool ShaktoolItem { get; set; }

        public ItemPool PegWorldItem { get; set; }

        public bool Keysanity { get; set; }

        public KeysanityMode KeysanityMode { get; set; } = KeysanityMode.None;

        public ItemPlacementRule ItemPlacementRule { get; set; } = ItemPlacementRule.Anywhere;

        public bool Race { get; set; }

        public bool DisableSpoilerLog { get; set; }

        public bool DisableTrackerHints { get; set; }

        public bool DisableTrackerSpoilers { get; set; }

        public bool DisableCheats { get; set; }

        [JsonIgnore]
        public bool CopySeedAndRaceSettings { get; set; }

        public ISet<ItemType> EarlyItems { get; set; } = new HashSet<ItemType>();

        public IDictionary<int, int> LocationItems { get; set; } = new Dictionary<int, int>();
    }
}
