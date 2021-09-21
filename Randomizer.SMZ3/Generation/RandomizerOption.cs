using System.Collections.Generic;

using Randomizer.Shared.Contracts;

namespace Randomizer.SMZ3.Generation
{
    public class RandomizerOption : IRandomizerOption
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public RandomizerOptionType Type { get; set; }
        public Dictionary<string, string> Values { get; set; }
        public string Default { get; set; }
    }

}
