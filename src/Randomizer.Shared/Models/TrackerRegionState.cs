using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models {

    public class TrackerRegionState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TrackerStateId { get; set; }
        public string TypeName { get; set; }
        public int? Reward { get; set; }
        public byte? Medallion { get; set; }
    }

}
