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

        public SwordLocation SwordLocation { get; set; }

        public MorphLocation MorphLocation { get; set; }

        public bool Keysanity { get; set; }

        public bool Race { get; set; }

        public string Msu1Path { get; set; }

        public Dictionary<string, string> ToDictionary() => new()
        {
            ["gamemode"] = "normal", // Multiworld not supported in this fork
            ["players"] = "1",
            ["z3logic"] = "normal", // Not used anywhere
            ["smlogic"] = "normal", // Irrelevant if you're playing without IBJ, may support later?
            ["swordlocation"] = SwordLocation.ToString().ToLowerInvariant(),
            ["morphlocation"] = MorphLocation.ToString().ToLowerInvariant(),
            ["goal"] = "defeatboth", // Only one value
            ["keyshuffle"] = Keysanity ? "keysanity" : "none",
            ["race"] = Race ? "true" : "false",
            ["ganoninvincible"] = "beforecrystals", // Does anyone even change this?
        };
    }
}