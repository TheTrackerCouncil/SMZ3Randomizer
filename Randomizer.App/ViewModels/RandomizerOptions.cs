using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

using Randomizer.SMZ3;

namespace Randomizer.App.ViewModels
{
    public class RandomizerOptions : INotifyPropertyChanged
    {
        private const string SnesRomFilter = "SNES ROMs (*.sfc, *.smc)|*.sfc;*.smc|All files (*.*)|*.*";

        private readonly Window _owner;

        public event PropertyChangedEventHandler PropertyChanged;

        public RandomizerOptions()
        {
        }

        public RandomizerOptions(Window owner)
        {
            _owner = owner;
        }

        public BrowseFileCommand BrowseRomCommand => new BrowseFileCommand(_owner, SnesRomFilter);

        public string Z3RomPath { get; set; }

        public string SMRomPath { get; set; }

        public string Seed { get; set; }

        public SwordLocation SwordLocation { get; set; }

        public MorphLocation MorphLocation { get; set; }

        public bool Keysanity { get; set; }

        public bool Race { get; set; }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }
    }
}