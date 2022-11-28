using System.Collections.Generic;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Generation
{
    public class SeedData
    {
        public SeedData(string guid, string seed, string game, string mode, WorldGenerationDataCollection worldGenerationData, Config primaryConfig, IEnumerable<Config> configs, Playthrough playthrough)
        {
            Guid = guid;
            Seed = seed;
            Game = game;
            Mode = mode;
            WorldGenerationData = worldGenerationData;
            PrimaryConfig = primaryConfig;
            Configs = configs;
            Playthrough = playthrough;
        }

        public string Guid { get; set; }
        public string Seed { get; set; }
        public string Game { get; set; }
        public string Mode { get; set; }
        public WorldGenerationDataCollection WorldGenerationData { get; set; }
        public Config PrimaryConfig { get; set; }
        public IEnumerable<Config> Configs { get; set; }
        public Playthrough Playthrough { get; set; }
    }

}
