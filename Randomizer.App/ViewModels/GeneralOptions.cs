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
        public string Z3RomPath { get; set; }

        public string SMRomPath { get; set; }

        public string RomOutputPath { get; set; }
            = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "Seeds");

        public bool Validate()
        {
            return File.Exists(Z3RomPath)
                && File.Exists(SMRomPath)
                && (Directory.Exists(RomOutputPath) || RomOutputPath == null);
        }
    }
}
