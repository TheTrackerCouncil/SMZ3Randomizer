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
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Tracking.Services;

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
        /// <param name="bossStates">Boss states.</param>
        /// <param name="secondsElapsed">Seconds elapsed.</param>
        /// <param name="seedConfig">Seed config.</param>
        public TrackerState(IReadOnlyCollection<ItemState> itemStates,
            IReadOnlyCollection<LocationState> locationStates,
            IReadOnlyCollection<RegionState> regionStates,
            IReadOnlyCollection<DungeonState> dungeonStates,
            IReadOnlyCollection<MarkedLocation> markedLocations,
            IReadOnlyCollection<BossState> bossStates,
            IReadOnlyCollection<TrackerHistoryEvent> trackerHistoryEvents,
            double secondsElapsed,
            Config seedConfig)
        {
            ItemStates = itemStates;
            LocationStates = locationStates;
            RegionStates = regionStates;
            DungeonStates = dungeonStates;
            MarkedLocations = markedLocations;
            BossStates = bossStates;
            HistoryEvents = trackerHistoryEvents;
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
        /// Gets a collection of marked locations.
        /// </summary>
        public IReadOnlyCollection<BossState> BossStates { get; }

        /// <summary>
        /// Gets a collection of marked locations.
        /// </summary>
        public IReadOnlyCollection<TrackerHistoryEvent> HistoryEvents { get; }

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
        public static TrackerState TakeSnapshot(Tracker tracker, IItemService itemService)
        {
            var itemStates = itemService.AllItems()
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
            var dungeonStates = tracker.WorldInfo.Dungeons
                .Select(x => new DungeonState(x.Name[0], x.Cleared, x.TreasureRemaining, x.Reward, x.Requirement))
                .ToImmutableList();
            var markedLocations = tracker.MarkedLocations
                .Select(x => new MarkedLocation(x.Key, x.Value.Name[0]))
                .ToImmutableList();
            var bossStates = tracker.WorldInfo.Bosses
                .Select(x => new BossState(x.Name[0], x.Defeated))
                .ToImmutableList();
            var historyEvents = tracker.History.GetHistory();
            var seedConfig = tracker.World.Config;
            var secondsElapsed = tracker.TotalElapsedTime.TotalSeconds;
            return new TrackerState(
                itemStates,
                locationStates,
                regionStates,
                dungeonStates,
                markedLocations,
                bossStates,
                historyEvents,
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
            dbContext.Entry(trackerState).Collection(x => x.BossStates).Load();
            dbContext.Entry(trackerState).Collection(x => x.History).Load();

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

            var bossStates = trackerState.BossStates
                .Select(x => new BossState(x.BossName, x.Defeated))
                .ToImmutableList();

            var historyEvents = trackerState.History
                .ToImmutableList();

            var secondsElapsed = trackerState.SecondsElapsed;

            var config = GeneratedRom.IsValid(generatedRom) ? Config.FromConfigString(generatedRom.Settings) : new Config();

            return new TrackerState(
                itemStates,
                locationStates,
                regionStates,
                dungeonStates,
                markedLocations,
                bossStates,
                historyEvents,
                secondsElapsed,
                config ?? new Config());
        }

        /// <summary>
        /// Loads the config from the generated rom's Settings string
        /// </summary>
        /// <param name="generatedRom">The generated rom</param>
        /// <returns>The deserialized config</returns>
        public static Config LoadConfig(GeneratedRom generatedRom)
        {
            return GeneratedRom.IsValid(generatedRom) ? Config.FromConfigString(generatedRom.Settings) ?? new Config() : new Config(); ;
        }

        /// <summary>
        /// Restores the saved state to the specified tracker instance.
        /// </summary>
        /// <param name="tracker">
        /// The tracker instance to apply the state to.
        /// </param>
        /// <param name="worldAccessor">Used to set the loaded world.</param>
        public void Apply(Tracker tracker, IWorldAccessor worldAccessor, IItemService itemService)
        {
            var world = new World(SeedConfig, "", 0, "");

            foreach (var itemState in ItemStates)
            {
                var item = itemService.FindOrDefault(itemState.Name)
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
                var dungeon = tracker.WorldInfo.Dungeons.SingleOrDefault(x => x.Name.Contains(dungeonState.Name, StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentException($"Could not find dungeon with name '{dungeonState.Name}'.", nameof(tracker));

                dungeon.Cleared = dungeonState.Cleared;
                dungeon.TreasureRemaining = dungeonState.RemainingTreasure;
                dungeon.Reward = dungeonState.Reward;
                dungeon.Requirement = dungeonState.Requirement;
            }

            tracker.MarkedLocations.Clear();
            foreach (var markedLocation in MarkedLocations)
            {
                var item = itemService.FindOrDefault(markedLocation.ItemName)
                    ?? throw new ArgumentException($"Could not find loaded item data for '{markedLocation.ItemName}'.", nameof(tracker));

                tracker.MarkedLocations[markedLocation.LocationId] = item;
            }

            foreach (var bossState in BossStates)
            {
                var boss = tracker.WorldInfo.Bosses.SingleOrDefault(x => x.Name[0] == bossState.Name)
                    ?? throw new ArgumentException($"Could not find boss with name '{bossState.Name}.", nameof(tracker));

                boss.Defeated = bossState.Defeated;
            }
            tracker.History.LoadHistory(tracker, this);
            worldAccessor.World = world;
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
            var totalLocations = LocationStates.Count;
            var clearedLocations = LocationStates
                .Where(x => x.Cleared)
                .Count();
            var percCleared = (int)Math.Floor((double)clearedLocations / totalLocations * 100);

            if (rom.TrackerState == null)
            {
                var trackerState = new Shared.Models.TrackerState()
                {
                    StartDateTime = DateTimeOffset.Now,
                    UpdatedDateTime = DateTimeOffset.Now,
                    SecondsElapsed = SecondsElapsed,
                    PercentageCleared = percCleared
                };

                if (rom != null && string.IsNullOrEmpty(rom.Settings))
                {
                    rom.Settings = Config.ToConfigString(SeedConfig, true);
                }

                if (rom != null)
                {
                    rom.TrackerState = trackerState;
                }

                dbContext.TrackerStates.Add(trackerState);
            }
            else
            {
                var trackerState = rom.TrackerState;
                trackerState.UpdatedDateTime = DateTimeOffset.Now;
                trackerState.SecondsElapsed = SecondsElapsed;
                trackerState.PercentageCleared = percCleared;
            }

            CopyItemStates(rom.TrackerState, ItemStates);
            CopyLocationStates(rom.TrackerState, LocationStates);
            CopyRegionStates(rom.TrackerState, RegionStates);
            CopyDungeonStates(rom.TrackerState, DungeonStates);
            CopyBossStates(rom.TrackerState, BossStates);
            CopyHistoryEvents(rom.TrackerState, HistoryEvents);
            CopyMarkedLocations(rom.TrackerState, MarkedLocations);

            return dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Represents the tracking state of an item.
        /// </summary>
        /// <param name="Name">The primary name of the item.</param>
        /// <param name="TrackingState">The tracking state of the item.</param>
        public record ItemState(string Name, int TrackingState);

        private static void CopyItemStates(Shared.Models.TrackerState trackerState, IReadOnlyCollection<ItemState> states)
        {
            if (trackerState.ItemStates?.Count == 0)
            {
                trackerState.ItemStates = states
                    .Select(x => new TrackerItemState()
                    {
                        ItemName = x.Name,
                        TrackingState = x.TrackingState
                    })
                    .ToList();
            }
            else
            {
                trackerState.ItemStates
                    .Select(x => new { dbEntry = x, trackerEntry = states.First(y => y.Name == x.ItemName) })
                    .ToList()
                    .ForEach(x =>
                    {
                        x.dbEntry.TrackingState = x.trackerEntry.TrackingState;
                    });
            }
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

        private static void CopyLocationStates(Shared.Models.TrackerState trackerState, IReadOnlyCollection<LocationState> states)
        {
            if (trackerState.LocationStates?.Count == 0)
            {
                trackerState.LocationStates = states
                    .Select(x => new TrackerLocationState()
                    {
                        LocationId = x.Id,
                        Item = x.Item,
                        Cleared = x.Cleared
                    })
                    .ToList();
            }
            else
            {
                trackerState.LocationStates
                    .Select(x => new { dbEntry = x, trackerEntry = states.First(y => y.Id == x.LocationId) })
                    .ToList()
                    .ForEach(x =>
                    {
                        x.dbEntry.Cleared = x.trackerEntry.Cleared;
                    });
            }
        }

        /// <summary>
        /// Represents the tracking state of a region.
        /// </summary>
        /// <param name="TypeName">The class name of the region.</param>
        /// <param name="Reward">The type of reward for the region.</param>
        /// <param name="Medallion">
        /// The type of medallion required for the region.
        /// </param>
        public record RegionState(string TypeName, RewardType? Reward, ItemType? Medallion);

        private static void CopyRegionStates(Shared.Models.TrackerState trackerState, IReadOnlyCollection<RegionState> states)
        {
            if (trackerState.RegionStates?.Count == 0)
            {
                trackerState.RegionStates = states
                    .Select(x => new TrackerRegionState()
                    {
                        TypeName = x.TypeName,
                        Reward = x.Reward,
                        Medallion = x.Medallion
                    })
                    .ToList();
            }
            else
            {
                trackerState.RegionStates
                    .Select(x => new { dbEntry = x, trackerEntry = states.First(y => y.TypeName == x.TypeName) })
                    .ToList()
                    .ForEach(x =>
                    {
                        x.dbEntry.Reward = x.trackerEntry.Reward;
                        x.dbEntry.Medallion = x.trackerEntry.Medallion;
                    });
            }
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

        private static void CopyDungeonStates(Shared.Models.TrackerState trackerState, IReadOnlyCollection<DungeonState> states)
        {
            if (trackerState.DungeonStates?.Count == 0)
            {
                trackerState.DungeonStates = states
                    .Select(x => new TrackerDungeonState()
                    {
                        Name = x.Name,
                        Cleared = x.Cleared,
                        RemainingTreasure = x.RemainingTreasure,
                        Reward = x.Reward,
                        RequiredMedallion = x.Requirement
                    })
                    .ToList();
            }
            else
            {
                trackerState.DungeonStates
                    .Select(x => new { dbEntry = x, trackerEntry = states.First(y => y.Name == x.Name) })
                    .ToList()
                    .ForEach(x =>
                    {
                        x.dbEntry.Cleared = x.trackerEntry.Cleared;
                        x.dbEntry.RemainingTreasure = x.trackerEntry.RemainingTreasure;
                        x.dbEntry.Reward = x.trackerEntry.Reward;
                        x.dbEntry.RequiredMedallion = x.trackerEntry.Requirement;
                    });
            }
        }

        /// <summary>
        /// Represents a marked location.
        /// </summary>
        /// <param name="LocationId">The ID of the location.</param>
        /// <param name="ItemName">
        /// The name of the item that was marked at the location.
        /// </param>
        public record MarkedLocation(int LocationId, string ItemName);

        private static void CopyMarkedLocations(Shared.Models.TrackerState trackerState, IReadOnlyCollection<MarkedLocation> markedLocations)
        {
            trackerState.MarkedLocations = markedLocations
                .Select(x => new TrackerMarkedLocation()
                {
                    LocationId = x.LocationId,
                    ItemName = x.ItemName
                })
                .ToList();
        }

        /// <summary>
        /// Represents the tracking state of a boss
        /// </summary>
        /// <param name="Name">The name of the boss</param>
        /// <param name="Defeated">
        /// Indicates whether the boss has been defeated or not.
        /// </param>
        public record BossState(string Name, bool Defeated);

        private static void CopyBossStates(Shared.Models.TrackerState trackerState, IReadOnlyCollection<BossState> states)
        {
            if (trackerState.BossStates?.Count == 0)
            {
                trackerState.BossStates = states
                    .Select(x => new TrackerBossState()
                    {
                        BossName = x.Name,
                        Defeated = x.Defeated
                    })
                    .ToList();
            }
            else
            {
                trackerState.BossStates
                    .Select(x => new { dbEntry = x, trackerEntry = states.First(y => y.Name == x.BossName) })
                    .ToList()
                    .ForEach(x =>
                    {
                        x.dbEntry.Defeated = x.trackerEntry.Defeated;
                    });
            }
        }

        public static void CopyHistoryEvents(Shared.Models.TrackerState trackerState, IReadOnlyCollection<TrackerHistoryEvent> history)
        {
            trackerState.History = history.ToList();
        }
    }
}
