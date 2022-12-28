using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Shared.Models
{
    public class GeneratedRom
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; } = 0;
        public DateTimeOffset Date { get; set; }
        public string Label { get; set; } = "";
        public string Seed { get; init;  } = "";
        public string Settings { get; init;  } = "";
        public int GeneratorVersion { get; init; }
        public string RomPath { get; init; } = "";
        public string SpoilerPath { get; init; } = "";
        [ForeignKey("MultiplayerGameDetails")]
        public long? MultiplayerGameDetailsId { get; set; }
        public virtual MultiplayerGameDetails? MultiplayerGameDetails { get; set; }
        public TrackerState? TrackerState { get; set; }

        public static bool IsValid([NotNullWhen(true)] GeneratedRom? rom)
        {
            return rom is { Id: > 0 };
        }
    }

}
