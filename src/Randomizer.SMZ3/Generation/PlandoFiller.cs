using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.Extensions.Logging;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.SMZ3.Contracts;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.Generation
{
    /// <summary>
    /// Fills an SMZ3 world with items according to pre-planned configuration.
    /// </summary>
    public class PlandoFiller : IFiller
    {
        private readonly ILogger<PlandoFiller> _logger;
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlandoFiller"/> class
        /// with the specified configuration.
        /// </summary>
        /// <param name="plandoConfig">
        /// The plando configuration that determines how items are placed.
        /// </param>
        /// <param name="logger">Used to write logging information.</param>
        public PlandoFiller(PlandoConfig plandoConfig, ILogger<PlandoFiller> logger)
        {
            PlandoConfig = plandoConfig;
            _logger = logger;
            _random = new Random();
        }

        /// <summary>
        /// Gets the plando configuration for the current filler.
        /// </summary>
        public PlandoConfig PlandoConfig { get; }

        /// <summary>
        /// Randomly distributes items across locations in the specified worlds.
        /// </summary>
        /// <param name="worlds">The world or worlds to initialize.</param>
        /// <param name="config">The configuration to use.</param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        /// <exception cref="PlandoConfigurationException">
        /// The plando configuration contains one or more errors.
        /// </exception>
        public void Fill(List<World> worlds, Config config, CancellationToken cancellationToken)
        {
            if (worlds.Count == 0) return;
            if (worlds.Count > 1) throw new NotSupportedException(nameof(PlandoFiller) + " does not suport multi-world generation.");

            var world = worlds.First();

            FillItems(world);
            AssignRewards(world);
            AssignMedallions(world);
        }

        public void SetRandom(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        private static void EnsureDungeonsHaveMedallions(World world)
        {
            var emptyMedallions = world.Regions.Where(x => x is INeedsMedallion { Medallion: ItemType.Nothing });
            if (emptyMedallions.Any())
            {
                throw new PlandoConfigurationException($"Not all dungeons have had their medallions set. Missing:\n"
                    + string.Join('\n', emptyMedallions.Select(x => x.Name)));
            }
        }

        private static void EnsureDungeonsHaveRewards(World world)
        {
            var emptyDungeons = world.Regions.Where(x => x is IHasReward { RewardType: RewardType.None });
            if (emptyDungeons.Any())
            {
                throw new PlandoConfigurationException($"Not all dungeons have had their rewards set. Missing:\n"
                    + string.Join('\n', emptyDungeons.Select(x => x.Name)));
            }
        }

        private static void EnsureLocationsHaveItems(World world)
        {
            var emptyLocations = world.Locations.Where(x => x.Item.Type == ItemType.Nothing);
            if (emptyLocations.Any())
            {
                throw new PlandoConfigurationException($"Not all locations have been filled. Missing:\n"
                    + string.Join('\n', emptyLocations.Select(x => x.Name)));
            }
        }

        private void AssignMedallions(World world)
        {
            foreach (var regionName in PlandoConfig.Medallions.Keys)
            {
                var medallion = PlandoConfig.Medallions[regionName];
                var region = world.Regions.FirstOrDefault(x => x.Name.Equals(regionName, StringComparison.OrdinalIgnoreCase));
                if (region == null)
                    throw new PlandoConfigurationException($"Could not find a region with the specified name.\nName: '{regionName}'");

                if (region is not INeedsMedallion dungeon)
                    throw new PlandoConfigurationException($"{region.Name} is configured with a medallion but that region cannot be configured with medallions.");

                dungeon.Medallion = medallion switch
                {
                    ItemType.Bombos => ItemType.Bombos,
                    ItemType.Ether => ItemType.Ether,
                    ItemType.Quake => ItemType.Quake,
                    _ => throw new PlandoConfigurationException($"{medallion} is not a valid type of medallion.")
                };
            }

            EnsureDungeonsHaveMedallions(world);
        }

        private void AssignRewards(World world)
        {
            foreach (var regionName in PlandoConfig.Rewards.Keys)
            {
                var rewardType = PlandoConfig.Rewards[regionName];
                var region = world.Regions.FirstOrDefault(x => x.Name.Equals(regionName, StringComparison.OrdinalIgnoreCase));
                if (region == null)
                    throw new PlandoConfigurationException($"Could not find a region with the specified name.\nName: '{regionName}'");

                if (region is not IHasReward dungeon)
                    throw new PlandoConfigurationException($"{region.Name} is configured with a reward but that region cannot be configured with rewards.");

                dungeon.Reward = new Reward(rewardType, world, dungeon);
            }

            EnsureDungeonsHaveRewards(world);
        }

        private void FillItems(World world)
        {
            foreach (var locationName in PlandoConfig.Items.Keys)
            {
                var itemType = PlandoConfig.Items[locationName];
                var location = world.FindLocation(locationName, StringComparison.OrdinalIgnoreCase);
                if (location == null)
                    throw new PlandoConfigurationException($"Could not find a location with the specified name.\nName: '{locationName}'");

                location.Item = new Item(itemType, world);
            }

            EnsureLocationsHaveItems(world);
        }
    }
}
