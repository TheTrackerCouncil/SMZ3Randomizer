using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Randomizer.Shared;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Regions;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Represents the tracker state in a form that can be saved or restored.
    /// </summary>
    public class TrackerState
    {
        private static readonly JsonSerializerOptions s_options = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerState"/> class
        /// with the specified state.
        /// </summary>
        /// <param name="itemStates">Item states.</param>
        /// <param name="locationStates">Location states.</param>
        /// <param name="regionStates">Region states.</param>
        /// <param name="dungeonStates">Dungeon states.</param>
        /// <param name="markedLocations">Marked locations.</param>
        /// <param name="secondsElapsed">Seconds elapsed.</param>
        /// <param name="seedConfig">Seed config.</param>
        public TrackerState(IReadOnlyCollection<ItemState> itemStates,
            IReadOnlyCollection<LocationState> locationStates,
            IReadOnlyCollection<RegionState> regionStates,
            IReadOnlyCollection<DungeonState> dungeonStates,
            IReadOnlyCollection<MarkedLocation> markedLocations,
            double secondsElapsed,
            Config seedConfig)
        {
            ItemStates = itemStates;
            LocationStates = locationStates;
            RegionStates = regionStates;
            DungeonStates = dungeonStates;
            MarkedLocations = markedLocations;
            SecondsElapsed = secondsElapsed;
            SeedConfig = seedConfig;
        }

        /// <summary>
        /// Gets a collection containing the tracking state of items.
        /// </summary>
        public IReadOnlyCollection<ItemState> ItemStates { get; }

        /// <summary>
        /// Gets a collection containing the tracking state of locations.
        /// </summary>
        public IReadOnlyCollection<LocationState> LocationStates { get; }

        /// <summary>
        /// Gets a collection containing the tracking state of regions.
        /// </summary>
        public IReadOnlyCollection<RegionState> RegionStates { get; }

        /// <summary>
        /// Gets a collection containing the tracking state of dungeons.
        /// </summary>
        public IReadOnlyCollection<DungeonState> DungeonStates { get; }

        /// <summary>
        /// Gets a collection of marked locations.
        /// </summary>
        public IReadOnlyCollection<MarkedLocation> MarkedLocations { get; }

        /// <summary>
        /// Gets the <see cref="Config"/> used to generate the seed being
        /// tracked.
        /// </summary>
        public Config SeedConfig { get; }

        /// <summary>
        /// The seconds that have elapsed
        /// </summary>
        public double SecondsElapsed { get; }

        /// <summary>
        /// Takes a copy of the state from the specified tracker instance.
        /// </summary>
        /// <param name="tracker">
        /// The tracker instance whose state to copy.
        /// </param>
        /// <returns>
        /// A new <see cref="TrackerState"/> object representing the state of
        /// <paramref name="tracker"/>.
        /// </returns>
        public static TrackerState TakeSnapshot(Tracker tracker)
        {
            var itemStates = tracker.Items
                .Select(x => new ItemState(x.Name[0], x.TrackingState))
                .ToImmutableList();
            var locationStates = tracker.World.Locations
                .Select(x => new LocationState(x.Id, x.Item?.Type, x.Cleared))
                .ToImmutableList();
            var regionStates = tracker.World.Regions
                .Select(x => new RegionState(x.GetType().Name,
                    x is IHasReward rewardRegion ? rewardRegion.Reward : null,
                    x is INeedsMedallion medallionRegion ? medallionRegion.Medallion : null))
                .ToImmutableList();
            var dungeonStates = tracker.Dungeons
                .Select(x => new DungeonState(x.Name[0], x.Cleared, x.TreasureRemaining, x.Reward, x.Requirement))
                .ToImmutableList();
            var markedLocations = tracker.MarkedLocations
                .Select(x => new MarkedLocation(x.Key, x.Value.Name[0]))
                .ToImmutableList();
            var seedConfig = tracker.World.Config;
            var secondsElapsed = tracker.TotalElapsedTime.TotalSeconds;
            return new TrackerState(
                itemStates,
                locationStates,
                regionStates,
                dungeonStates,
                markedLocations,
                secondsElapsed,
                seedConfig);
        }

        /// <summary>
        /// Loads the tracker state from serialized data.
        /// </summary>
        /// <param name="stream">A stream containing the saved state.</param>
        /// <returns>
        /// A task returning a new <see cref="TrackerState"/> from <paramref
        /// name="stream"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="stream"/> does not contain a valid Tracker saved
        /// state.
        /// </exception>
        public static async Task<TrackerState> LoadAsync(Stream stream)
        {
            try
            {
                var state = await JsonSerializer.DeserializeAsync<TrackerState>(stream, s_options);
                return state ?? throw new JsonException("Failed to deserialize tracker state (Deserialize returned null).");
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("The stream does not contain a valid Tracker saved state.", nameof(stream), ex);
            }
        }

        /// <summary>
        /// Loads a the tracker state from the database
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="generatedRom"></param>
        /// <returns>The loaded tracker state</returns>
        public static TrackerState? Load(RandomizerContext dbContext, GeneratedRom generatedRom)
        {
            var trackerState = generatedRom.TrackerState;

            if (trackerState == null)
            {
                return null;
            }

            dbContext.Entry(trackerState).Collection(x => x.ItemStates).Load();
            dbContext.Entry(trackerState).Collection(x => x.LocationStates).Load();
            dbContext.Entry(trackerState).Collection(x => x.RegionStates).Load();
            dbContext.Entry(trackerState).Collection(x => x.DungeonStates).Load();
            dbContext.Entry(trackerState).Collection(x => x.MarkedLocations).Load();

            var itemStates = trackerState.ItemStates
                .Select(x => new ItemState(x.ItemName, x.TrackingState))
                .ToImmutableList();

            var locationStates = trackerState.LocationStates
                .Select(x => new LocationState(x.LocationId, x.Item, x.Cleared))
                .ToImmutableList();

            var regionStates = trackerState.RegionStates
                .Select(x => new RegionState(x.TypeName, x.Reward, x.Medallion))
                .ToImmutableList();

            var dungeonStates = trackerState.DungeonStates
                .Select(x => new DungeonState(x.Name, x.Cleared, x.RemainingTreasure, x.Reward, x.RequiredMedallion))
                .ToImmutableList();

            var markedLocations = trackerState.MarkedLocations
                .Select(x => new MarkedLocation(x.LocationId, x.ItemName))
                .ToImmutableList();

            var secondsElapsed = trackerState.SecondsElapsed;

            var config = generatedRom == null ? new Config() : JsonSerializer.Deserialize<Config>(generatedRom.Settings, s_options);

            return new TrackerState(
                itemStates,
                locationStates,
                regionStates,
                dungeonStates,
                markedLocations,
                secondsElapsed,
                config ?? new Config());
        }

        /// <summary>
        /// Restores the saved state to the specified tracker instance.
        /// </summary>
        /// <param name="tracker">
        /// The tracker instance to apply the state to.
        /// </param>
        public void Apply(Tracker tracker)
        {
            var world = new World(SeedConfig, "", 0, "");

            foreach (var itemState in ItemStates)
            {
                var item = tracker.FindItemByName(itemState.Name)
                    ?? throw new ArgumentException($"Could not find loaded item data for '{itemState.Name}'.", nameof(tracker));

                item.TrackingState = itemState.TrackingState;
            }

            foreach (var locationState in LocationStates)
            {
                var location = world.Locations.SingleOrDefault(x => x.Id == locationState.Id)
                    ?? throw new ArgumentException($"Could not find location with ID {locationState.Id}.", nameof(tracker));

                location.Item = locationState.Item != null ? new SMZ3.Item(locationState.Item.Value, world) : null;
                location.Cleared = locationState.Cleared;
            }

            foreach (var regionState in RegionStates)
            {
                var region = world.Regions.SingleOrDefault(x => x.GetType().Name == regionState.TypeName)
                    ?? throw new ArgumentException($"Could not find region with type name '{regionState.TypeName}'.", nameof(tracker));

                if (region is IHasReward rewardRegion && regionState.Reward != null)
                    rewardRegion.Reward = regionState.Reward.Value;
                if (region is INeedsMedallion medallionRegion && regionState.Medallion != null)
                    medallionRegion.Medallion = regionState.Medallion.Value;
            }

            foreach (var dungeonState in DungeonStates)
            {
                var dungeon = tracker.Dungeons.SingleOrDefault(x => x.Name.Contains(dungeonState.Name, StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentException($"Could not find dungeon with name '{dungeonState.Name}'.", nameof(tracker));

                dungeon.Cleared = dungeonState.Cleared;
                dungeon.TreasureRemaining = dungeonState.RemainingTreasure;
                dungeon.Reward = dungeonState.Reward;
                dungeon.Requirement = dungeonState.Requirement;
            }

            tracker.MarkedLocations.Clear();
            foreach (var markedLocation in MarkedLocations)
            {
                var item = tracker.FindItemByName(markedLocation.ItemName)
                    ?? throw new ArgumentException($"Could not find loaded item data for '{markedLocation.ItemName}'.", nameof(tracker));

                tracker.MarkedLocations[markedLocation.LocationId] = item;
            }

            tracker.World = world;
            tracker.SavedElapsedTime = TimeSpan.FromSeconds(SecondsElapsed);
        }

        /// <summary>
        /// Saves the tracker state.
        /// </summary>
        /// <param name="destination">The stream to save the state to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SaveAsync(Stream destination)
        {
            return JsonSerializer.SerializeAsync(destination, this, s_options);
        }

        /// <summary>
        /// Saves the tracker state to the database
        /// </summary>
        /// <param name="dbContext">The dbcontext to save to</param>
        /// <param name="rom">The GeneratedRom to save</param>
        /// <returns></returns>
        public Task SaveAsync(RandomizerContext dbContext, GeneratedRom rom)
        {
            if (rom.TrackerState == null)
            {
                var trackerState = new Randomizer.Shared.Models.TrackerState()
                {
                    Date = DateTimeOffset.Now,
                    SecondsElapsed = SecondsElapsed
                };

                trackerState.ItemStates = ItemStates
                    .Select(x => new TrackerItemState()
                    {
                        ItemName = x.Name,
                        TrackingState = x.TrackingState
                    })
                    .ToList();

                trackerState.LocationStates = LocationStates
                    .Select(x => new TrackerLocationState()
                    {
                        LocationId = x.Id,
                        Item = x.Item,
                        Cleared = x.Cleared
                    })
                    .ToList();

                trackerState.RegionStates = RegionStates
                    .Select(x => new TrackerRegionState()
                    {
                        TypeName = x.TypeName,
                        Reward = x.Reward,
                        Medallion = x.Medallion
                    })
                    .ToList();

                trackerState.DungeonStates = DungeonStates
                    .Select(x => new TrackerDungeonState()
                    {
                        Name = x.Name,
                        Cleared = x.Cleared,
                        RemainingTreasure = x.RemainingTreasure,
                        Reward = x.Reward,
                        RequiredMedallion = x.Requirement
                    })
                    .ToList();

                trackerState.MarkedLocations = MarkedLocations
                    .Select(x => new TrackerMarkedLocation()
                    {
                        LocationId = x.LocationId,
                        ItemName = x.ItemName
                    })
                    .ToList();

                if (rom != null)
                {
                    rom.Settings = JsonSerializer.Serialize(SeedConfig, s_options);
                }

                rom.TrackerState = trackerState;
                dbContext.TrackerStates.Add(trackerState);
            }
            else
            {
                var trackerState = rom.TrackerState;

                trackerState.ItemStates.ToList().ForEach(x => CopyItemState(x, ItemStates.First(y => y.Name == x.ItemName)));
                trackerState.LocationStates.ToList().ForEach(x => CopyLocationState(x, LocationStates.First(y => y.Id == x.LocationId)));
                trackerState.RegionStates.ToList().ForEach(x => CopyRegionState(x, RegionStates.First(y => y.TypeName == x.TypeName)));
                trackerState.DungeonStates.ToList().ForEach(x => CopyDungeonState(x, DungeonStates.First(y => y.Name == x.Name)));

                trackerState.MarkedLocations = MarkedLocations
                    .Select(x => new TrackerMarkedLocation()
                    {
                        LocationId = x.LocationId,
                        ItemName = x.ItemName
                    })
                    .ToList();

                trackerState.Date = DateTimeOffset.Now;
                trackerState.SecondsElapsed = SecondsElapsed;
            }

            return dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Represents the tracking state of an item.
        /// </summary>
        /// <param name="Name">The primary name of the item.</param>
        /// <param name="TrackingState">The tracking state of the item.</param>
        public record ItemState(string Name, int TrackingState);

        /// <summary>
        /// Copies the item state values to the db item state
        /// </summary>
        /// <param name="trackerItemState">The db item state</param>
        /// <param name="itemState">The tracker item state</param>
        private static void CopyItemState(TrackerItemState trackerItemState, ItemState itemState)
        {
            trackerItemState.TrackingState = itemState.TrackingState;
        }

        /// <summary>
        /// Represents the tracking state of a location.
        /// </summary>
        /// <param name="Id">The ID of the location.</param>
        /// <param name="Item">
        /// The type of item at the location. May be <c>null</c> if Tracker was
        /// started without a generated seed.
        /// </param>
        /// <param name="Cleared">
        /// Indicates whether the location has been cleared or not.
        /// </param>
        public record LocationState(int Id, ItemType? Item, bool Cleared);

        /// <summary>
        /// Copies the location state values to the db location state
        /// </summary>
        /// <param name="trackerLocationState">The db location state</param>
        /// <param name="locationState">The tracker location state</param>
        private static void CopyLocationState(TrackerLocationState trackerLocationState, LocationState locationState)
        {
            trackerLocationState.LocationId = locationState.Id;
            trackerLocationState.Item = locationState.Item;
            trackerLocationState.Cleared = locationState.Cleared;
        }

        /// <summary>
        /// Represents the tracking state of a region.
        /// </summary>
        /// <param name="TypeName">The class name of the region.</param>
        /// <param name="Reward">The type of reward for the region.</param>
        /// <param name="Medallion">
        /// The type of medallion required for the region.
        /// </param>
        public record RegionState(string TypeName, Reward? Reward, ItemType? Medallion);

        /// <summary>
        /// Copies the region state values to the db region state
        /// </summary>
        /// <param name="trackerRegionState">The db region state</param>
        /// <param name="regionState">The tracker region state</param>
        private static void CopyRegionState(TrackerRegionState trackerRegionState, RegionState regionState)
        {
            trackerRegionState.TypeName = regionState.TypeName;
            trackerRegionState.Reward = regionState.Reward;
            trackerRegionState.Medallion = regionState.Medallion;
        }

        /// <summary>
        /// Represents the tracking state of a dungeon.
        /// </summary>
        /// <param name="Name">The name of the dungeon.</param>
        /// <param name="Cleared">
        /// Indicates whether the dungeon has been fully cleared.
        /// </param>
        /// <param name="RemainingTreasure">
        /// The number of treasure chests remaining.
        /// </param>
        /// <param name="Reward">The type of pendant/crystal reward.</param>
        /// <param name="Requirement">The type of medallion required.</param>
        public record DungeonState(string Name, bool Cleared, int RemainingTreasure, RewardItem Reward, Medallion Requirement);

        /// <summary>
        /// Copies the dungeon state values to the db dungeon state
        /// </summary>
        /// <param name="trackerDungeonState">The db dungeon state</param>
        /// <param name="dungeonState">The tracker dungeon state</param>
        private static void CopyDungeonState(TrackerDungeonState trackerDungeonState, DungeonState dungeonState)
        {
            trackerDungeonState.Name = dungeonState.Name;
            trackerDungeonState.Cleared = dungeonState.Cleared;
            trackerDungeonState.RemainingTreasure = dungeonState.RemainingTreasure;
            trackerDungeonState.Reward = dungeonState.Reward;
            trackerDungeonState.RequiredMedallion = dungeonState.Requirement;
        }

        /// <summary>
        /// Represents a marked location.
        /// </summary>
        /// <param name="LocationId">The ID of the location.</param>
        /// <param name="ItemName">
        /// The name of the item that was marked at the location.
        /// </param>
        public record MarkedLocation(int LocationId, string ItemName);
    }
}
