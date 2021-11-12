
using Randomizer.Shared.Contracts;

namespace Randomizer.SMZ3.Generation
{
    public class LocationTypeData : ILocationTypeData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Region { get; set; }
        public string Area { get; set; }
    }

}
