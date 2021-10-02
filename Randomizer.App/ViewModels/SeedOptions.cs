using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

using Randomizer.SMZ3;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// Represents user-configurable options for influencing seed generation.
    /// </summary>
    public class SeedOptions
    {
        public Sprite LinkSprite { get; set; }

        public Sprite SamusSprite { get; set; }

        [JsonIgnore]
        public string Seed { get; set; }

        public ItemPlacement SwordLocation { get; set; }

        public ItemPlacement MorphLocation { get; set; }

        public ItemPlacement MorphBombsLocation { get; set; }

        public ItemPool ShaktoolItem { get; set; }

        public bool Keysanity { get; set; }

        public bool Race { get; set; }

        public string Msu1Path { get; set; }
    }
}
