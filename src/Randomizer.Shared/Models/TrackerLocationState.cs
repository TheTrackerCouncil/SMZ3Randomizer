using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Randomizer.Shared.Models
{

    public class TrackerLocationState
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public TrackerState TrackerState { get; set; }
        public int LocationId { get; set; }
        public ItemType? Item { get; set; }
        public bool Cleared { get; set; }
    }

}
