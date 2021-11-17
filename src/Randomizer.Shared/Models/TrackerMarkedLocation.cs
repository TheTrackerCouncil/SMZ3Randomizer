using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models {

    public class TrackerMarkedLocation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TrackerStateId { get; set; }
        public int LocationId { get; set; }
        public string ItemName { get; set; }
    }

}
