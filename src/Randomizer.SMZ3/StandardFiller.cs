﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3
{
    public class StandardFiller : IFiller
    {
        private readonly ILogger<StandardFiller> _logger;

        public StandardFiller(ILogger<StandardFiller> logger)
        {
            _logger = logger;
        }

        protected Random Random { get; set; }
        
        public void SetRandom(Random random)
            => Random = random ?? throw new ArgumentNullException(nameof(random));

        public void Fill(List<World> worlds, Config config, CancellationToken cancellationToken)
        {
            if (Random == null)
            {
                throw new InvalidOperationException("No random number generator " +
                    "was set. " + nameof(SetRandom) + " must be called with a " +
                    "valid instance prior this calling this method.");
            }

            foreach (var world in worlds)
            {
                world.Setup(Random);
            }

            var progressionItems = new List<Item>();
            var baseItems = new List<Item>();

            foreach (var world in worlds)
            {
                /* The dungeon pool order is significant, don't shuffle */
                var dungeon = Item.CreateDungeonPool(world);
                var progression = Item.CreateProgressionPool(world);

                InitialFillInOwnWorld(dungeon, progression, world, config);

                if (config.Keysanity == false)
                {
                    var worldLocations = world.Locations.Empty().Shuffle(Random);
                    var keyCards = Item.CreateKeycards(world);
                    AssumedFill(dungeon, progression.Concat(keyCards).ToList(), worldLocations, new[] { world }, cancellationToken);
                    baseItems = baseItems.Concat(keyCards).ToList();
                }
                else
                {
                    progressionItems.AddRange(Item.CreateKeycards(world));
                }

                progressionItems.AddRange(dungeon);
                progressionItems.AddRange(progression);
            }

            progressionItems = progressionItems.Shuffle(Random);
            var niceItems = worlds.SelectMany(world => Item.CreateNicePool(world)).Shuffle(Random);
            var junkItems = worlds.SelectMany(world => Item.CreateJunkPool(world)).Shuffle(Random);

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

            ApplyItemPoolPreferences(junkItems, locations, worlds, config);
            GanonTowerFill(worlds, junkItems, 2);
            AssumedFill(progressionItems, baseItems, locations, worlds, cancellationToken);
            FastFill(niceItems, locations);
            FastFill(junkItems, locations);
        }

        private void ApplyItemPoolPreferences(List<Item> junkItems, List<Location> locations, List<World> worlds, Config config)
        {
            ApplyPreference(config.ShaktoolItemPool, world => world.InnerMaridia.ShaktoolItem);
            ApplyPreference(config.PegWorldItemPool, world => world.DarkWorldNorthWest.PegWorld);

            void ApplyPreference(ItemPool setting, Func<World, Location> selectLocation)
            {
                switch (setting)
                {
                    case ItemPool.Progression:
                        // If we always want to have a progression item, move it
                        // to the top of the list so it gets filled early while
                        // we still have all progression items in the pool. We
                        // also add a filter to prevent it from filling it with
                        // the wrong items.
                        var locationId = selectLocation(worlds[0]).Id;
                        var location = locations
                            .MoveToTop(x => x.Id == locationId)
                            .Allow((item, items) => !item.Type.IsInCategory(ItemCategory.Scam));
                        break;

                    case ItemPool.Junk:
                        FastFill(junkItems, worlds.Select(selectLocation));
                        break;
                }
            }
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

        private void InitialFillInOwnWorld(List<Item> dungeonItems, List<Item> progressionItems, World world, Config config)
        {
            FillItemAtLocation(dungeonItems, ItemType.KeySW, world.SkullWoods.PinballRoom);

            switch (config.SwordLocation)
            {
                case ItemPlacement.Original: FillItemAtLocation(progressionItems, ItemType.ProgressiveSword, world.HyruleCastle.LinksUncle); break;
                case ItemPlacement.Early: FrontFillItemInOwnWorld(progressionItems, ItemType.ProgressiveSword, world); break;
            }

            switch (config.MorphLocation)
            {
                case ItemPlacement.Original: FillItemAtLocation(progressionItems, ItemType.Morph, world.BlueBrinstar.MorphBall); break;
                case ItemPlacement.Early: FrontFillItemInOwnWorld(progressionItems, ItemType.Morph, world); break;
            }

            switch (config.MorphBombsLocation)
            {
                case ItemPlacement.Original: FillItemAtLocation(progressionItems, ItemType.Bombs, world.CentralCrateria.BombTorizo); break;
                case ItemPlacement.Early: FrontFillItemInOwnWorld(progressionItems, ItemType.Bombs, world); break;
            }

            /* We place a PB and Super in Sphere 1 to make sure the filler
             * doesn't start locking items behind this when there are a
             * high chance of the trash fill actually making them available */
            FrontFillItemInOwnWorld(progressionItems, ItemType.Super, world);
            FrontFillItemInOwnWorld(progressionItems, ItemType.PowerBomb, world);
        }

        private void AssumedFill(List<Item> itemPool, List<Item> baseItems,
            IEnumerable<Location> locations, IEnumerable<World> worlds,
            CancellationToken cancellationToken)
        {
            var assumedItems = new List<Item>(itemPool);
            while (assumedItems.Count > 0)
            {
                /* Try placing next item */
                var item = assumedItems.First();
                assumedItems.Remove(item);

                var inventory = CollectItems(assumedItems.Concat(baseItems), worlds);
                var location = locations.Empty().CanFillWithinWorld(item, inventory).FirstOrDefault();
                if (location == null)
                {
                    _logger.LogDebug("Could not find anywhere to place {item}", item);
                    assumedItems.Add(item);
                    continue;
                }

                location.Item = item;
                itemPool.Remove(item);
                _logger.LogDebug("Placed {item} at {location}", item, location);

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private IEnumerable<Item> CollectItems(IEnumerable<Item> items, IEnumerable<World> worlds)
        {
            var assumedItems = new List<Item>(items);
            var remainingLocations = worlds.SelectMany(l => l.Locations).Filled().ToList();

            while (true)
            {
                var availableLocations = remainingLocations.AvailableWithinWorld(assumedItems).ToList();
                remainingLocations = remainingLocations.Except(availableLocations).ToList();
                var foundItems = availableLocations.Select(x => x.Item).ToList();
                if (foundItems.Count == 0)
                    break;

                assumedItems.AddRange(foundItems);
            }

            return assumedItems;
        }

        private void FrontFillItemInOwnWorld(List<Item> itemPool, ItemType itemType, World world)
        {
            var item = itemPool.Get(itemType);
            var location = world.Locations.Empty().Available(world.Items).Random(Random);
            if (location == null)
            {
                throw new InvalidOperationException($"Tried to front fill {item.Name} in, but no location was available");
            }

            location.Item = item;
            itemPool.Remove(item);
            _logger.LogDebug("Front-filled {item} at {location}", item, location);
        }

        private void GanonTowerFill(List<World> worlds, List<Item> itemPool, double factor)
        {
            var locations = worlds
                .SelectMany(x => x.Locations)
                .Where(x => x.Region is Regions.Zelda.GanonsTower)
                .Empty().Shuffle(Random);
            FastFill(itemPool, locations.Take((int)(locations.Count / factor)));
        }

        private void FastFill(List<Item> itemPool, IEnumerable<Location> locations)
        {
            foreach (var (location, item) in locations.Empty().Zip(itemPool, (l, i) => (l, i)).ToList())
            {
                location.Item = item;
                itemPool.Remove(item);
                _logger.LogDebug("Fast-filled {item} at {location}", item, location);
            }
        }

        private void FillItemAtLocation(List<Item> itemPool, ItemType itemType, Location location)
        {
            var itemToPlace = itemPool.Get(itemType);
            location.Item = itemToPlace ?? throw new InvalidOperationException($"Tried to place item {itemType} at {location.Name}, but there is no such item in the item pool");
            itemPool.Remove(itemToPlace);

            _logger.LogDebug("Manually placed {item} at {location}", itemToPlace, location);
        }
    }
}