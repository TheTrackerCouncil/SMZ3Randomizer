using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.GeneratedData;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.Infrastructure;

public class PlaythroughService(ILogger<PlaythroughService> logger)
{
    private readonly HashSet<LocationId> _verifyLocationIds =
    [
        LocationId.GanonsTowerMoldormChest,
        LocationId.KraidsLairVariaSuit,
        LocationId.WreckedShipEastSuper,
        LocationId.InnerMaridiaSpaceJump,
        LocationId.LowerNorfairRidleyTank
    ];

    private readonly HashSet<ItemType> _verifyItemTypes =
    [
        ItemType.SilverArrows,
        ItemType.CardCrateriaBoss
    ];

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
    /// <param name="defaultReward"></param>
    /// <param name="defaultItems"></param>
    /// <returns>A list of the spheres</returns>
    /// <exception cref="RandomizerGenerationException">When not all locations are </exception>
    public IEnumerable<Playthrough.Sphere> GenerateSpheres(IEnumerable<Location> allLocations, Reward? defaultReward = null, List<Item>? defaultItems = null)
    {
        var allLocationsList = allLocations.ToList();
        var worlds = allLocationsList.Select(x => x.Region.World).Distinct().ToList();

        var spheres = new List<Playthrough.Sphere>();
        var locations = new List<Location>();
        var items = defaultItems ?? [];
        var defaultRewards = new List<Reward>();
        if (defaultReward != null)
        {
            defaultRewards.Add(defaultReward);
        }

        var rewardRegions = worlds.SelectMany(x => x.RewardRegions).ToList();
        List<Reward> rewards;

        var bossRegions = worlds.SelectMany(x => x.BossRegions).ToList();
        List<Boss> bosses;

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
        var totalItemCount = allLocationsList.Select(x => x.Item).Count();
        var prevRewardCount = 0;
        while (items.Count - initInventoryCount < totalItemCount)
        {
            var sphere = new Playthrough.Sphere();

            if (spheres.Count == 0)
            {
                sphere.StartingItems.AddRange(items);
            }

            var tempProgression = new Progression(items, new List<Reward>(), new List<Boss>());
            rewards = rewardRegions.Where(x => x.CanRetrieveReward(tempProgression, false))
                .Select(x => x.Reward)
                .Concat(defaultRewards).ToList();
            bosses = bossRegions.Where(x => x.CanBeatBoss(tempProgression, false))
                .Select(x => x.Boss).ToList();

            var accessibleLocations = allLocationsList.Where(l =>
                l.IsAvailable(new Progression(items.Where(i => i.World == l.World),
                    rewards.Where(r => r.World == l.World), bosses.Where(b => b.World == l.World)))).ToList();
            var newLocations = accessibleLocations.Except(locations).ToList();
            var newItems = newLocations.Select(l => l.Item).ToList();

            locations.AddRange(newLocations);
            items.AddRange(newItems);

            logger.LogDebug("Sphere {Number}: {ItemCount} new items | {RewardCount} new rewards", spheres.Count + 1, newItems.Count, rewards.Count - prevRewardCount);
            logger.LogDebug("Sphere {Number} Items:{Items}", spheres.Count + 1, string.Join("", newLocations.Select(x => $"\r\n\t{x.RandomName} - {x.Item.Name}")));
            logger.LogDebug("Sphere {Number} Rewards:{Rewards}", spheres.Count + 1, string.Join("", rewards.Select(x => $"\r\n\t{x.Type}")));

            if (!newItems.Any() && prevRewardCount == rewards.Count)
            {
                /* With no new items added we might have a problem, so list inaccessable items */
                var inaccessibleLocations = allLocationsList.Where(l => !locations.Contains(l)).ToList();

                logger.LogDebug("Inaccesible locations: {}", string.Join(", ", inaccessibleLocations.Select(x => x.Id.ToString())));

                // If there are a large number of inaccessible locations, throw an error if we can't beat the game
                // We determine this on if all players can beat all 4 golden bosses, access the
                if (inaccessibleLocations.Select(l => l.Item).Count() >= (15 * worlds.Count()))
                {
                    var vitalLocations = allLocationsList.Where(x => x.Id is LocationId.GanonsTowerMoldormChest or LocationId.KraidsLairVariaSuit or LocationId.WreckedShipEastSuper or LocationId.InnerMaridiaSpaceJump or LocationId.LowerNorfairRidleyTank).ToList();
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

    public bool ValidatePlaythrough(Playthrough playthrough, List<World> worlds)
    {
        List<Location> allLocations = [];
        List<Item> allItems = [];
        foreach (var sphere in playthrough.Spheres)
        {
            allLocations.AddRange(sphere.Locations.Where(x => _verifyLocationIds.Contains(x.Id)));
            allItems.AddRange(sphere.Items.Concat(sphere.StartingItems).Where(x => _verifyItemTypes.Contains(x.Type)));
        }

        if (allLocations.Count < worlds.Count * 5)
        {
            var inaccessibleLocations = worlds.SelectMany(w => w.Locations)
                .Where(l => _verifyLocationIds.Contains(l.Id) && !allLocations.Contains(l))
                .Select(x => $"Player {x.World.Id} {x}");
            logger.LogError("The following critical location(s) cannot be accessed: {Locations}", string.Join(",", inaccessibleLocations));
            return false;
        }

        var expectedKeycards = worlds.Count(x => x.Config.MetroidKeysanity);
        if (allItems.Count < worlds.Count + expectedKeycards)
        {
            logger.LogError("At least one player cannot obtain the silver arrows or get to mother brain");
            return false;
        }

        return true;
    }
}
