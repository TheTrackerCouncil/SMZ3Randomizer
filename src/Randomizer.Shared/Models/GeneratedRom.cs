using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models
{
    public class GeneratedRom
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; } = 0;
        public DateTimeOffset Date { get; set; }
        public string Label { get; set; } = "";
        public string Seed { get; init;  } = "";
        public string Settings { get; init;  } = "";
        public int GeneratorVersion { get; init; }
        public string RomPath { get; init; } = "";
        public string SpoilerPath { get; init; } = "";
        public TrackerState? TrackerState { get; set; }

        public static bool IsValid(GeneratedRom? rom)
        {
            return rom is { Id: > 0 };
        }
    }

}
