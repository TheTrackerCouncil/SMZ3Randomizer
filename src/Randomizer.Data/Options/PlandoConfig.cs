using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;

using YamlDotNet.Serialization;
using Randomizer.Data.Logic;

namespace Randomizer.Data.Options
{
    /// <summary>
    /// Represents the configuration for a plandomizer world.
    /// </summary>
    public class PlandoConfig // TODO: Consider using this instead of SeedData?
    {
        /// <summary>
        /// Initializes a new empty instance of the <see cref="PlandoConfig"/>
        /// class.
        /// </summary>
        public PlandoConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlandoConfig"/> based
        /// on the specified world.
        /// </summary>
        /// <param name="world">The world instance to export to a config.</param>
        public PlandoConfig(World world)
        {
            KeysanityMode = world.Config.KeysanityMode;
            GanonsTowerCrystalCount = world.Config.GanonCrystalCount;
            GanonCrystalCount = world.Config.GanonCrystalCount;
            OpenPyramid = world.Config.OpenPyramid;
            TourianBossCount = world.Config.TourianBossCount;
            Items = world.Locations
                .ToDictionary(x => x.ToString(), x => x.Item.Type);
            Rewards = world.Regions.Where(x => x is IHasReward)
                .ToDictionary(x => x.ToString(), x => ((IHasReward)x).RewardType);
            Medallions = world.Regions.Where(x => x is INeedsMedallion)
                .ToDictionary(x => x.ToString(), x => ((INeedsMedallion)x).Medallion);
            Logic = world.Config.LogicConfig.Clone();
            StartingInventory = world.Config.ItemOptions;
            var prizes = DropPrizes.GetPool(world.Config.ZeldaDrops);
            ZeldaPrizes.EnemyDrops = prizes.Take(56).ToList();
            ZeldaPrizes.TreePulls = prizes.Skip(56).Take(3).ToList();
            ZeldaPrizes.CrabBaseDrop = prizes.Skip(59).First();
            ZeldaPrizes.CrabEightDrop = prizes.Skip(60).First();
            ZeldaPrizes.StunPrize = prizes.Skip(61).First();
            ZeldaPrizes.FishPrize = prizes.Skip(62).First();
        }

        /// <summary>
        /// Gets or sets the name of the file from which the plando config was
        /// deserialized.
        /// </summary>
        [YamlIgnore]
        public string FileName { get; set; } = "";

        /// <summary>
        /// Gets or sets a value indicating whether Keysanity should be enabled.
        /// </summary>
        public KeysanityMode KeysanityMode { get; set; }

        /// <summary>
        /// Gets or sets the number of crystals for entering GT
        /// </summary>
        public int GanonsTowerCrystalCount { get; set; } = 7;

        /// <summary>
        /// Gets or sets the number of crystals needed to hurt Ganon
        /// </summary>
        public int GanonCrystalCount { get; set; } = 7;

        /// <summary>
        /// Gets or sets if the pyramid should be open by default
        /// </summary>
        public bool OpenPyramid { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of SM Golden Bosses need to be defeated to enter Tourian
        /// </summary>
        public int TourianBossCount { get; set; } = 4;

        /// <summary>
        /// Gets or sets the logic options that apply to the plando.
        /// </summary>
        public LogicConfig Logic { get; set; } = new();

        /// <summary>
        /// Gets or sets a dictionary that contains the names of locations and
        /// the types of items they should be filled with.
        /// </summary>
        public Dictionary<string, ItemType> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets a dictionary that contains the names of regions and the
        /// boss rewards they should be filled with.
        /// </summary>
        public Dictionary<string, RewardType> Rewards { get; set; } = new();

        /// <summary>
        /// Gets or sets a dictionary that contains the names of regions and the
        /// medallions they require.
        /// </summary>
        public Dictionary<string, ItemType> Medallions { get; set; } = new();

        /// <summary>
        /// Text overrides
        /// </summary>
        public PlandoTextConfig Text { get; set; } = new();

        /// <summary>
        /// Various Zelda enemy drops and other prizes
        /// </summary>
        public PlandoZeldaPrizeConfig ZeldaPrizes { get; set; } = new();

        /// <summary>
        /// Item Options for the starting inventory
        /// </summary>
        public IDictionary<string, int> StartingInventory { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Lines for tracker to say when tracking a location
        /// </summary>
        public IDictionary<string, string> TrackerLocationLines { get; set; } = new Dictionary<string, string>();
    }
}
