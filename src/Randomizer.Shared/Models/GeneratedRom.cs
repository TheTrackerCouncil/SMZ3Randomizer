using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models
{
    public class GeneratedRom
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public string Settings { get; set; }
        public int GeneratorVersion { get; set; }
        public string RomPath { get; set; }
        public string SpoilerPath { get; set; }
        public TrackerState TrackerState { get; set; }

        public static bool IsValid(GeneratedRom rom)
        {
            return rom != null && rom.Id > 0;
        }
    }

}
