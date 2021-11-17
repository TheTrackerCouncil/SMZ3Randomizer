using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Randomizer.Shared;
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
        /// <param name="seedConfig">Seed config.</param>
        public TrackerState(IReadOnlyCollection<ItemState> itemStates,
            IReadOnlyCollection<LocationState> locationStates,
            IReadOnlyCollection<RegionState> regionStates,
            IReadOnlyCollection<DungeonState> dungeonStates,
            IReadOnlyCollection<MarkedLocation> markedLocations,
            Config seedConfig)
        {
            ItemStates = itemStates;
            LocationStates = locationStates;
            RegionStates = regionStates;
            DungeonStates = dungeonStates;
            MarkedLocations = markedLocations;
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
            return new TrackerState(
                itemStates,
                locationStates,
                regionStates,
                dungeonStates,
                markedLocations,
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
        /// Represents the tracking state of an item.
        /// </summary>
        /// <param name="Name">The primary name of the item.</param>
        /// <param name="TrackingState">The tracking state of the item.</param>
        public record ItemState(string Name, int TrackingState);

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
        /// Represents the tracking state of a region.
        /// </summary>
        /// <param name="TypeName">The class name of the region.</param>
        /// <param name="Reward">The type of reward for the region.</param>
        /// <param name="Medallion">
        /// The type of medallion required for the region.
        /// </param>
        public record RegionState(string TypeName, Reward? Reward, ItemType? Medallion);

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
        /// Represents a marked location.
        /// </summary>
        /// <param name="LocationId">The ID of the location.</param>
        /// <param name="ItemName">
        /// The name of the item that was marked at the location.
        /// </param>
        public record MarkedLocation(int LocationId, string ItemName);
    }
}
