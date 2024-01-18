using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Infrastructure;

public class PlaythroughService
{
    private ILogger<PlaythroughService> _logger;

    public PlaythroughService(ILogger<PlaythroughService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simulates a rough playthrough and returns a value indicating whether
    /// the playthrough is likely possible.
    /// </summary>
    /// <param name="worlds">A list of the worlds to play through.</param>
    /// <param name="config">The config used to generate the worlds.</param>
    /// <param name="playthrough">
    /// If this method returns <see langword="true"/>, the generated
    /// playthrough; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the playthrough is possible; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGenerate(IReadOnlyCollection<World> worlds, Config config, [MaybeNullWhen(true)] out Playthrough? playthrough)
    {
        try
        {
            playthrough = Generate(worlds, config);
            return true;
        }
        catch (RandomizerGenerationException)
        {
            playthrough = null;
            return false;
        }
    }

    /// <summary>
    /// Simulates a rough playthrough of the specified worlds.
    /// </summary>
    /// <param name="worlds">A list of the worlds to play through.</param>
    /// <param name="config">The config used to generate the worlds.</param>
    /// <returns>A new <see cref="Playthrough"/>.</returns>
    /// <exception cref="RandomizerGenerationException">
    /// The seed is likely impossible.
    /// </exception>
    public Playthrough Generate(IReadOnlyCollection<World> worlds, Config config)
    {
        var spheres = GenerateSpheres(worlds.SelectMany(x => x.Locations));
        return new Playthrough(config, spheres);
    }

    /// <summary>
    /// Returns a list of all of the spheres for a given set of locations
    /// </summary>
    /// <param name="allLocations">List of locations to iterate through</param>
    /// <param name="defaultRewards"></param>
    /// <returns>A list of the spheres</returns>
    /// <exception cref="RandomizerGenerationException">When not all locations are </exception>
    public IEnumerable<Playthrough.Sphere> GenerateSpheres(IEnumerable<Location> allLocations, Reward? defaultReward = null)
    {
        var worlds = allLocations.Select(x => x.Region.World).Distinct();

        var spheres = new List<Playthrough.Sphere>();
        var locations = new List<Location>();
        var items = new List<Item>();
        var defaultRewards = new List<Reward>();
        if (defaultReward != null)
        {
            defaultRewards.Add(defaultReward);
        }

        var allRewards = worlds.SelectMany(w => w.Regions).OfType<IHasReward>().Select(x => x.Reward);
        var regions = new List<Region>();
        var rewards = defaultRewards;

        var allBosses = worlds.SelectMany(w => w.GoldenBosses);
        var bossRegions = worlds.SelectMany(w => w.GoldenBosses).Select(x => x.Region).Cast<IHasBoss>();
        var bosses = new List<Boss>();

        foreach (var world in worlds)
        {
            if (!world.Config.MetroidKeysanity)
            {
                var keycards = world.ItemPools.Keycards;
                items.AddRange(keycards);
            }

            items.AddRange(ItemSettingOptions.GetStartingItemTypes(world.Config).Select(x => new Item(x, world)));
        }

        var initInventoryCount = items.Count;
        var totalItemCount = allLocations.Select(x => x.Item).Count();
        var prevRewardCount = 0;
        while (items.Count - initInventoryCount < totalItemCount)
        {
            var sphere = new Playthrough.Sphere();

            var tempProgression = new Progression(items, new List<Reward>(), new List<Boss>());
            rewards = allRewards.Where(x => x.Region.CanComplete(tempProgression)).Concat(defaultRewards).ToList();
            bosses = bossRegions.Where(x => x.CanBeatBoss(tempProgression)).Select(x => x.Boss).ToList();

            var accessibleLocations = allLocations.Where(l => l.IsAvailable(new Progression(items.Where(i => i.World == l.World), rewards.Where(r => r.World == l.World), bosses.Where(b => b.World == l.World))));
            var newLocations = accessibleLocations.Except(locations).ToList();
            var newItems = newLocations.Select(l => l.Item).ToList();

            locations.AddRange(newLocations);
            items.AddRange(newItems);

            _logger.LogDebug("Sphere {Number}: {ItemCount} new items | {RewardCount} new rewards", spheres.Count + 1, newItems.Count, rewards.Count - prevRewardCount);

            if (!newItems.Any() && prevRewardCount == rewards.Count)
            {
                /* With no new items added we might have a problem, so list inaccessable items */
                var inaccessibleLocations = allLocations.Where(l => !locations.Contains(l)).ToList();

                _logger.LogDebug("Inaccesible locations: {}", string.Join(", ", inaccessibleLocations.Select(x => x.Id.ToString())));

                // If there are a large number of inaccessible locations, throw an error if we can't beat the game
                // We determine this on if all players can beat all 4 golden bosses, access the
                if (inaccessibleLocations.Select(l => l.Item).Count() >= (15 * worlds.Count()))
                {
                    var vitalLocations = allLocations.Where(x => x.Id is LocationId.GanonsTowerMoldormChest or LocationId.KraidsLairVariaSuit or LocationId.WreckedShipEastSuper or LocationId.InnerMaridiaSpaceJump or LocationId.LowerNorfairRidleyTank).ToList();
                    var crateriaBossKeys = items.Count(x => x.Type == ItemType.CardCrateriaBoss);
                    if (accessibleLocations.Count(x => vitalLocations.Contains(x)) != vitalLocations.Count() || crateriaBossKeys != worlds.Count())
                    {
                        throw new RandomizerGenerationException("Too many inaccessible items, seed likely impossible.");
                    }
                }

                sphere.InaccessibleLocations.AddRange(inaccessibleLocations);
                break;
            }

            sphere.Locations.AddRange(newLocations);
            sphere.Items.AddRange(newItems);
            spheres.Add(sphere);
            prevRewardCount = rewards.Count;

            if (spheres.Count > 100)
                throw new RandomizerGenerationException("Too many spheres, seed likely impossible.");
        }

        return spheres;
    }
}
