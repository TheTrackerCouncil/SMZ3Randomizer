using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows.Media;

using Randomizer.SMZ3.Tracking;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// Represents user-configurable options for the general working of the
    /// randomizer itself.
    /// </summary>
    public class GeneralOptions
    {
        public string Z3RomPath { get; set; }

        public string SMRomPath { get; set; }

        public string RomOutputPath { get; set; }
            = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SMZ3CasRandomizer", "Seeds");

        [Range(0.0, 1.0)]
        public float TrackerRecognitionThreshold { get; set; } = 0.75f;

        [Range(0.0, 1.0)]
        public float TrackerConfidenceThreshold { get; set; } = 0.85f;

        [Range(0.0, 1.0)]
        public float TrackerConfidenceSassThreshold { get; set; } = 0.92f;

        public Color TrackerBackgroundColor { get; set; } = Color.FromRgb(0x21, 0x21, 0x21);

        public bool TrackerShadows { get; set; } = true;

        public bool Validate()
        {
            return File.Exists(Z3RomPath)
                && File.Exists(SMRomPath)
                && (Directory.Exists(RomOutputPath) || RomOutputPath == null);
        }

        public TrackerOptions GetTrackerOptions() => new()
        {
            MinimumRecognitionConfidence = TrackerRecognitionThreshold,
            MinimumExecutionConfidence = TrackerConfidenceThreshold,
            MinimumSassConfidence = TrackerConfidenceSassThreshold
        };
    }
}
