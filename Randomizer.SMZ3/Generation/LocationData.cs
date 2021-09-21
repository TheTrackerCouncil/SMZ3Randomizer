
using Randomizer.Shared.Contracts;

namespace Randomizer.SMZ3.Generation
{
    public class LocationData : ILocationData
    {
        public int LocationId { get; set; }
        public int ItemId { get; set; }
        public int ItemWorldId { get; set; }
    }

}
