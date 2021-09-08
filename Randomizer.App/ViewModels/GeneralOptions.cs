using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// Represents user-configurable options for the general working of the randomizer itself.
    /// </summary>
    public class GeneralOptions
    {
        private const string SnesRomFilter = "SNES ROMs (*.sfc, *.smc)|*.sfc;*.smc|All files (*.*)|*.*";

        private Window _owner;

        public void SetOwner(Window owner)
        {
            _owner = owner;
        }

        [JsonIgnore]
        public BrowseFileCommand BrowseRomCommand => new BrowseFileCommand(_owner, SnesRomFilter);

        public string Z3RomPath { get; set; }

        public string SMRomPath { get; set; }

        public bool Validate()
        {
            return File.Exists(Z3RomPath)
                && File.Exists(SMRomPath);
        }
    }
}