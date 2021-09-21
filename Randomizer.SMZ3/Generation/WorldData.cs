using System.Collections.Generic;

using Randomizer.Shared.Contracts;

namespace Randomizer.SMZ3.Generation
{
    public class WorldData : IWorldData
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public string Player { get; set; }
        public Dictionary<int, byte[]> Patches { get; set; }
        public List<ILocationData> Locations { get; set; }
    }

}
