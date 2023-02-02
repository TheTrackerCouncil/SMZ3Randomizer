using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.Extensions.Logging;
using Randomizer.Data.Logic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.SMZ3.Contracts;
using Randomizer.Data.Options;
using Randomizer.Shared.Enums;

namespace Randomizer.SMZ3.Generation
{
    /// <summary>
    /// Fills items in one or more SMZ3 worlds according to the vanilla SMZ3
    /// randomizer algorithm.
    /// </summary>
    public class StandardFiller : IFiller
    {
        private readonly ILogger<StandardFiller> _logger;

        public StandardFiller(ILogger<StandardFiller> logger)
        {
            _logger = logger;
            Random = new Random();
        }

        protected Random Random { get; set; }

        public void SetRandom(Random random)
            => Random = random ?? throw new ArgumentNullException(nameof(random));

        /// <summary>
        /// Randomly distributes items across locations in the specified worlds.
        /// </summary>
        /// <param name="worlds">The world or worlds to initialize.</param>
        /// <param name="config">The configuration to use.</param>
        /// <param name="cancellationToken">
        /// A token to monitor for cancellation requests.
        /// </param>
        public void Fill(List<World> worlds, Config config, CancellationToken cancellationToken)
        {
            if (Random == null)
            {
                throw new InvalidOperationException("No random number generator " +
                    "was set. " + nameof(SetRandom) + " must be called with a " +
                    "valid instance prior this calling this method.");
            }

            Random.Sanitize();

            // Setup the assumed inventory with the starting items
            var startingInventory = new List<Item>();
            foreach (var world in worlds)
            {
                world.Setup(Random);
                startingInventory.AddRange(ItemSettingOptions.GetStartingItemTypes(world.Config)
                    .Select(x => new Item(x, world)));
            }

            var progressionItems = new List<Item>();
            var assumedInventory = new List<Item>();
            assumedInventory.AddRange(startingInventory);
            var niceItems = worlds.SelectMany(x => x.ItemPools.Nice).Shuffle(Random);
            var junkItems = worlds.SelectMany(x => x.ItemPools.Junk).Shuffle(Random);

            foreach (var world in worlds)
            {
                var worldConfig = world.Config;

                /* The dungeon pool order is significant, don't shuffle */
                var dungeon = world.ItemPools.Dungeon.ToList();
                var progression = world.ItemPools.Progression.ToList();

                var preferenceItems = ApplyItemPoolPreferences(startingInventory, progression, niceItems, junkItems, world);

                InitialFillInOwnWorld(dungeon, progression, world, config, startingInventory);

                if (worldConfig.ZeldaKeysanity == false)
                {
                    _logger.LogDebug("Distributing dungeon items according to logic");
                    var worldLocations = world.Locations.Empty().Shuffle(Random);
                    var keyCards = world.ItemPools.Keycards;
                    AssumedFill(dungeon, progression.Concat(keyCards).Concat(assumedInventory).Concat(preferenceItems).ToList(), worldLocations, new[] { world }, cancellationToken);
                }

                if (worldConfig.MetroidKeysanity)
                {
                    progressionItems.AddRange(world.ItemPools.Keycards);
                }
                else
                {
                    assumedInventory = assumedInventory.Concat(world.ItemPools.Keycards).ToList();
                }

                progressionItems.AddRange(dungeon);
                progressionItems.AddRange(progression);
            }

            progressionItems = progressionItems.Shuffle(Random);

            var locations = worlds.SelectMany(x => x.Locations).Empty().Shuffle(Random);
            if (config.SingleWorld)
                locations = ApplyLocationWeighting(locations).ToList();

            if (config.MultiWorld)
            {
                /* Place moonpearls and morphs in last 40%/20% of the pool so that
                 * they will tend to place in earlier locations.
                 */
                ApplyItemBias(progressionItems, new[] {
                    (ItemType.MoonPearl, .40),
                    (ItemType.Morph, .20),
                });
            }

            _logger.LogDebug("Filling GT with junk");
            GanonTowerFill(worlds, junkItems, 2);

            _logger.LogDebug("Distributing progression items according to logic");
            AssumedFill(progressionItems, assumedInventory, locations, worlds, cancellationToken);

            _logger.LogDebug("Distributing nice-to-have items");
            FastFill(niceItems, locations);

            _logger.LogDebug("Distributing junk items");
            FastFill(junkItems, locations);
        }

        private List<Item> ApplyItemPoolPreferences(List<Item> startingInventory, List<Item> progressionItems, List<Item> niceItems, List<Item> junkItems, World world)
        {
            var placedItems = new List<Item>();
            var config = world.Config;

            // Populate location preferences
            var configLocations = config.LocationItems.Shuffle(Random);
            foreach (var (locationId, value) in configLocations)
            {
                var location = world.Locations.FirstOrDefault(x => x.Id == locationId && x.Item.Type == ItemType.Nothing);

                if (location == null)
                {
                    _logger.LogDebug($"Location could not be found or already has an item");
                    continue;
                }

                // First if a location is requested to be either progression or junk item pools
                if (value < Enum.GetValues(typeof(ItemPool)).Length)
                {
                    var itemPool = (ItemPool)value;

                    if (itemPool == ItemPool.Progression && progressionItems.Any())
                    {
                        // Some locations (AKA Shaktool) get pretty tough to tell if an item is needed there, so a workaround is to
                        // grab an item from the opposite game to minimize chances of situations where an item required to access a
                        // location is picked to go there
                        var item = progressionItems.FirstOrDefault(x => x.Type.IsInCategory(ItemCategory.Metroid) && location.Region is Z3Region || x.Type.IsInCategory(ItemCategory.Zelda) && location.Region is SMRegion);

                        if (item != null)
                        {
                            FillItemAtLocation(progressionItems, item.Type, location);
                            placedItems.Add(item);
                        }
                        else
                        {
                            _logger.LogDebug("Could not find item to place at {Location}", location.Name);
                        }
                    }
                    else if (itemPool == ItemPool.Junk && junkItems.Any())
                    {
                        placedItems.AddRange(FastFill(junkItems, world.Locations.Where(x => x.Id == locationId && x.World == world)));

                    }
                }
                // If a specific item is requested
                else
                {
                    var itemType = (ItemType)value;

                    if (progressionItems.Any(x => x.Type == itemType))
                    {
                        //var location = worlds[0].Locations.First(x => x.Id == locationId);
                        var itemsRequired = Logic.GetMissingRequiredItems(location, new Progression(), out _);

                        // If no items required or at least one combination of items required does not contain this item
                        if (!itemsRequired.Any() || itemsRequired.Any(x => !x.Contains(itemType)))
                        {
                            placedItems.Add(FillItemAtLocation(progressionItems, itemType, location));
                        }
                        else
                        {
                            throw new RandomizerGenerationException($"{itemType} was selected as the item for {location}, but it is required to get there.");
                        }
                    }
                    else if (niceItems.Any(x => x.Type == itemType))
                    {
                        placedItems.Add(FillItemAtLocation(niceItems, itemType, location));
                    }
                    else if (junkItems.Any(x => x.Type == itemType))
                    {
                        placedItems.Add(FillItemAtLocation(junkItems, itemType, location));
                    }
                }
            }

            // Push requested progression items to the top
            var configItems = ItemSettingOptions.GetEarlyItemTypes(config).Shuffle(Random);
            var addedItems = new List<ItemType>();
            addedItems.AddRange(startingInventory.Where(x => x.World == world).Select(x => x.Type));
            foreach (var itemType in configItems)
            {
                if (progressionItems.Concat(niceItems).Concat(junkItems).All(x => x.Type != itemType)) continue;
                var accessibleLocations = world.Locations.Where(x => x.Item.Type == ItemType.Nothing && x.IsAvailable(new Progression(addedItems, new List<RewardType>(), new List<BossType>()))).Shuffle(Random);
                var location = accessibleLocations.First();
                if (progressionItems.Any(x => x.Type == itemType))
                    placedItems.Add(FillItemAtLocation(progressionItems, itemType, location));
                else if(niceItems.Any(x => x.Type == itemType))
                    placedItems.Add(FillItemAtLocation(niceItems, itemType, location));
                else
                    placedItems.Add(FillItemAtLocation(junkItems, itemType, location));
                addedItems.Add(itemType);
            }

            return placedItems;
        }

        private void ApplyItemBias(List<Item> itemPool, IEnumerable<(ItemType type, double weight)> reorder)
        {
            var n = itemPool.Count;

            /* Gather all items that are being biased */
            var items = reorder.ToDictionary(x => x.type, x => itemPool.FindAll(item => item.Type == x.type));
            itemPool.RemoveAll(item => reorder.Any(x => x.type == item.Type));

            /* Insert items from each biased type such that their lowest index
             * is based on their weight on the original pool size
             */
            foreach (var (type, weight) in reorder.OrderByDescending(x => x.weight))
            {
                var i = (int)(n * (1 - weight));
                if (i >= itemPool.Count)
                    throw new InvalidOperationException($"Too many items are being biased which makes the tail portion for {type} too big");
                foreach (var item in items[type])
                {
                    var k = Random.Next(i, itemPool.Count);
                    itemPool.Insert(k, item);
                }
            }
        }

        private IEnumerable<Location> ApplyLocationWeighting(IEnumerable<Location> locations)
        {
            return from location in locations.Select((x, i) => (x, i: i - x.Weight))
                   orderby location.i
                   select location.x;
        }

        private void InitialFillInOwnWorld(List<Item> dungeonItems, List<Item> progressionItems, World world, Config config, List<Item> startingInventory)
        {
            FillItemAtLocation(dungeonItems, ItemType.KeySW, world.SkullWoods.PinballRoom);

            /* We place a PB and Super in Sphere 1 to make sure the filler
             * doesn't start locking items behind this when there are a
             * high chance of the trash fill actually making them available */
            if (!startingInventory.Any(x => x.World == world && x.Type == ItemType.Super))
                FrontFillItemInOwnWorld(progressionItems, ItemType.Super, world);
            if (!startingInventory.Any(x => x.World == world && x.Type == ItemType.PowerBomb))
                FrontFillItemInOwnWorld(progressionItems, ItemType.PowerBomb, world);
        }

        private void AssumedFill(List<Item> itemPool, List<Item> initialInventory,
            IEnumerable<Location> locations, IEnumerable<World> worlds,
            CancellationToken cancellationToken)
        {
            var allRewards = worlds.SelectMany(Reward.CreatePool);
            var allBosses = worlds.SelectMany(w => w.GoldenBosses);
            var itemsToAdd = new List<Item>(itemPool);
            var failedAttempts = new Dictionary<Item, int>();
            while (itemsToAdd.Count > 0)
            {
                /* Try placing next item */
                var item = itemsToAdd.First();
                itemsToAdd.Remove(item);

                var inventory = CollectItems(itemsToAdd.Concat(initialInventory), worlds);
                var currentRewards = CollectRewards(inventory, allRewards, worlds);
                var currentBosses = CollectBosses(inventory, currentRewards, allBosses, worlds);
                var location = locations.Empty().CanFillWithinWorld(item, inventory, currentRewards, currentBosses).FirstOrDefault();
                if (location == null)
                {
                    _logger.LogDebug("Could not find anywhere to place {Item}", item);
                    itemsToAdd.Add(item);

                    if (!failedAttempts.ContainsKey(item))
                        failedAttempts[item] = 0;
                    failedAttempts[item]++;

                    if (failedAttempts[item] > 500)
                        throw new RandomizerGenerationException("Infinite loop in generation found. Specified item location combinations may not be possible.");
                    continue;
                }

                location.Item = item;
                itemPool.Remove(item);
                _logger.LogDebug("Placed {Item} at {Location}", item, location);

                if (item.IsBigKey && !itemsToAdd.Any(x => x.IsBigKey))
                {
                    locations = locations.Shuffle(Random);
                    _logger.LogDebug("Reshuffling locations after final big key placement");
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private IEnumerable<Item> CollectItems(IEnumerable<Item> items, IEnumerable<World> worlds)
        {
            var assumedItems = new List<Item>(items);
            var remainingLocations = worlds.SelectMany(l => l.Locations).Filled().ToList();

            while (true)
            {
                var availableLocations = remainingLocations.AvailableWithinWorld(assumedItems, new List<Reward>(), new List<Boss>()).ToList();
                remainingLocations = remainingLocations.Except(availableLocations).ToList();
                var foundItems = availableLocations.Select(x => x.Item).ToList();
                if (foundItems.Count == 0)
                    break;

                assumedItems.AddRange(foundItems);
            }

            return assumedItems;
        }

        private IEnumerable<Reward> CollectRewards(IEnumerable<Item> items, IEnumerable<Reward> rewardPool, IEnumerable<World> worlds)
        {
            var progressions = worlds.ToDictionary(w => w, w => new Progression(items.Where(i => i.World == w), new List<Reward>(), new List<Boss>()));

            return worlds
                .SelectMany(w => w.Regions)
                .Where(r => r is IHasReward reward && reward.CanComplete(progressions[r.World]))
                .SelectMany(r => rewardPool.Where(p => p.Type == ((IHasReward)r).RewardType && p.Region == r));
        }

        private IEnumerable<Boss> CollectBosses(IEnumerable<Item> items, IEnumerable<Reward> rewards, IEnumerable<Boss> bossPool, IEnumerable<World> worlds)
        {
            var progressions = worlds.ToDictionary(w => w, w => new Progression(items.Where(i => i.World == w), rewards.Where(r => r.World == w), new List<Boss>()));

            return worlds
                .SelectMany(w => w.Regions)
                .Where(r => r is IHasBoss boss && boss.CanBeatBoss(progressions[r.World]))
                .SelectMany(r => bossPool.Where(p => p.Type == ((IHasBoss)r).BossType && p.Region == r));
        }

        private void FrontFillItemInOwnWorld(List<Item> itemPool, ItemType itemType, World world)
        {
            var item = itemPool.Get(itemType);
            var location = world.Locations.Empty()
                .Available(world.LocationItems, new List<Reward>(), new List<Boss>())
                .Where(x => !world.Config.LocationItems.ContainsKey(x.Id))
                .Random(Random);
            if (location == null)
                throw new InvalidOperationException($"Tried to front fill {item.Name} in, but no location was available");

            location.Item = item;
            itemPool.Remove(item);
            _logger.LogDebug("Front-filled {Item} at {Location}", item, location);
        }

        private void GanonTowerFill(List<World> worlds, List<Item> itemPool, double factor)
        {
            foreach (var world in worlds)
            {
                var locations = world.Locations
                    .Where(x => x.Region is Data.WorldData.Regions.Zelda.GanonsTower)
                    .Empty()
                    .Shuffle(Random);
                var numLocations = (int)Math.Floor(15.0 * world.Config.GanonsTowerCrystalCount / 7);
                FastFill(itemPool, locations.Take(numLocations));
            }
        }

        private List<Item> FastFill(List<Item> itemPool, IEnumerable<Location> locations)
        {
            var placedItems = new List<Item>();
            foreach (var (location, item) in locations.Empty().Zip(itemPool, (l, i) => (l, i)).ToList())
            {
                location.Item = item;
                itemPool.Remove(item);
                _logger.LogDebug("Fast-filled {Item} at {Location}", item, location);
                placedItems.Add(item);
            }

            return placedItems;
        }

        private Item FillItemAtLocation(List<Item> itemPool, ItemType itemType, Location location)
        {
            var itemToPlace = itemPool.Get(itemType, location.World);
            location.Item = itemToPlace ?? throw new InvalidOperationException($"Tried to place item {itemType} at {location.Name}, but there is no such item in the item pool");
            itemPool.Remove(itemToPlace);
            _logger.LogDebug("Manually placed {Item} at {Location}", itemToPlace, location);
            return itemToPlace;
        }
    }
}
