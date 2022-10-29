using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Randomizer.Shared.Enums;

namespace Randomizer.Shared.Models
{

    public class TrackerBossState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public TrackerState TrackerState { get; init; } = new();
        public string BossName { get; init; } = string.Empty;
        public bool Defeated { get; set; }
        public BossType Type { get; init; }
        public int WorldId { get; set; }
    }

}
