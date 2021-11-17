using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models
{
    public class Seed
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Date { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public string Settings { get; set; }
        public int GeneratorVersion { get; set; }
        public string RomPath { get; set; }
        public string SpoilerPath { get; set; }
    }

}
