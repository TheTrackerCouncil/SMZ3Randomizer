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
using Randomizer.Shared.Enums;

namespace Randomizer.SMZ3.Generation
{
    /// <summary>
    /// Fills an SMZ3 world with items according to pre-planned configuration.
    /// </summary>
    public class MultiplayerFiller : IFiller
    {
        private readonly ILogger<MultiplayerFiller> _logger;
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlandoFiller"/> class
        /// with the specified configuration.
        /// </summary>
        /// <param name="plandoConfig">
        /// The plando configuration that determines how items are placed.
        /// </param>
        /// <param name="logger">Used to write logging information.</param>
        public MultiplayerFiller(ILogger<MultiplayerFiller> logger)
        {
            _logger = logger;
            _random = new Random();
        }

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
            foreach (var world in worlds)
            {
                FillItems(world, worlds);
                FillDungeonData(world);
            }
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

        private void FillDungeonData(World world)
        {
            var generatedDungeonData = world.Config.MultiplayerPlayerGenerationData!.Dungeons;
            foreach (var dungeon in world.Dungeons)
            {
                var generatedData = generatedDungeonData.Single(x => x.Name == dungeon.DungeonName);
                if (generatedData.Medallion != ItemType.Nothing && dungeon is INeedsMedallion medallionRegion)
                {
                    medallionRegion.Medallion = generatedData.Medallion;
                    _logger.LogDebug("Marked {Dungeon} as requiring {Medallion}", generatedData.Name, generatedData.Medallion);
                }
                if (generatedData.Reward != null && dungeon is IHasReward rewardRegion)
                {
                    rewardRegion.Reward = new Reward(generatedData.Reward.Value, world, rewardRegion);
                    _logger.LogDebug("Marked {Dungeon} as having {Medallion}", generatedData.Name, generatedData.Reward);
                }
            }
            EnsureDungeonsHaveMedallions(world);
            EnsureDungeonsHaveRewards(world);
        }

        private void FillItems(World world, List<World> worlds)
        {
            var generatedLocationData = world.Config.MultiplayerPlayerGenerationData!.Locations;
            foreach (var location in world.Locations)
            {
                var generatedData = generatedLocationData.Single(x => x.Id == location.Id);
                var itemType = generatedData.Item;
                var itemWorld = worlds.Single(x => x.Id == generatedData.ItemWorldId);
                location.Item = new Item(itemType, itemWorld, isProgression: itemType.IsPossibleProgression(itemWorld.Config.ZeldaKeysanity, itemWorld.Config.MetroidKeysanity));
                _logger.LogDebug("Fast-filled {Item} at {Location}", generatedData.Item, location.Name);
            }
            EnsureLocationsHaveItems(world);
        }
    }
}
