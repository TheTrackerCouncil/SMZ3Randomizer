using System.Collections.Generic;

namespace Randomizer.SMZ3.Generation
{
    public class SeedData
    {
        public string Guid { get; set; }
        public string Seed { get; set; }
        public string Game { get; set; }
        public string Mode { get; set; }
        public List<(World World, Dictionary<int, byte[]> Patches)> Worlds { get; set; }

        public Playthrough Playthrough { get; set; }
    }

}
