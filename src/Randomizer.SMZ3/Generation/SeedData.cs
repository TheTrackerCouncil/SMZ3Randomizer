using System.Collections.Generic;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Generation
{
    public class SeedData
    {
        public string Guid { get; set; }
        public string Seed { get; set; }
        public string Game { get; set; }
        public string Mode { get; set; }
        public List<(World World, Dictionary<int, byte[]> Patches)> Worlds { get; set; }
        public List<(World World, List<string>)> Hints { get; set; }
        public Config PrimaryConfig { get; set; }
        public IEnumerable<Config> Configs { get; set; }
        public Playthrough Playthrough { get; set; }
    }

}
