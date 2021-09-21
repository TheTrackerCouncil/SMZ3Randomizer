using System.Collections.Generic;

using Randomizer.Shared.Contracts;

namespace Randomizer.SMZ3.Generation
{
    public class SeedData : ISeedData
    {
        public string Guid { get; set; }
        public string Seed { get; set; }
        public string Game { get; set; }
        public string Logic { get; set; }
        public string Mode { get; set; }
        public List<IWorldData> Worlds { get; set; }
        public List<Dictionary<string, string>> Playthrough { get; set; }
    }

}
