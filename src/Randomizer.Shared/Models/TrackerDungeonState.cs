using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models {

    public class TrackerDungeonState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TrackerStateId { get; set; }
        public string Name { get; set; }
        public bool Cleared { get; set; }
        public int RemainingTreasure { get; set; }
        public int Reward { get; set; }
        public int RequiredMedallion { get; set; }
    }

}
