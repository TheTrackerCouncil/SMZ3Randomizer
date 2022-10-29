using System.Collections.Generic;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Generation
{
    public class SeedData
    {
        public SeedData(string guid, string seed, string game, string mode, List<(World World, Dictionary<int, byte[]> Patches)> worlds, List<(World World, List<string>)> hints, Config primaryConfig, IEnumerable<Config> configs, Playthrough playthrough)
        {
            Guid = guid;
            Seed = seed;
            Game = game;
            Mode = mode;
            Worlds = worlds;
            Hints = hints;
            PrimaryConfig = primaryConfig;
            Configs = configs;
            Playthrough = playthrough;
        }

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
