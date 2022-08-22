using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Windows;

using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.App
{
    /// <summary>
    /// This is a shared class that is meant to act as an intermediary between
    /// the tracker and the location/map windows to keep everything in sync as
    /// well as clean up some of the logic in view models
    /// </summary>
    public class TrackerLocationSyncer
    {
        private bool _isDesign;
        private Region _stickyRegion = null;
        private readonly ILogger<TrackerLocationSyncer> _logger;
        private bool _showOutOfLogicLocations;

        public TrackerLocationSyncer()
        {
            _isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }

        /// <summary>
        /// Creates a new instance of the TrackerLocationSyncer that will be
        /// synced with a given tracker
        /// </summary>
        /// <param name="tracker">The tracker to keep things in sync with</param>
        public TrackerLocationSyncer(Tracker tracker, ILogger<TrackerLocationSyncer> logger)
        {
            Tracker = tracker;
            _logger = logger;

            // Set all events from the tracker to point to the two in this class
            Tracker.MarkedLocationsUpdated += (_, _) => MarkedLocationUpdated.Invoke(this, new(""));
            Tracker.LocationCleared += (_, e) =>
            {
                _stickyRegion = e.Location.Region;
                TrackedLocationUpdated.Invoke(this, new(e.Location.Name));
                MarkedLocationUpdated.Invoke(this, new(e.Location.Name));
            };
            Tracker.DungeonUpdated += (_, _) =>
            {
                TrackedLocationUpdated.Invoke(this, new(""));
                MarkedLocationUpdated.Invoke(this, new(""));
            };
            Tracker.ItemTracked += (_, _) =>
            {
                TrackedLocationUpdated.Invoke(this, new(""));
                MarkedLocationUpdated.Invoke(this, new(""));
            };
            Tracker.ActionUndone += (_, _) =>
            {
                TrackedLocationUpdated.Invoke(this, new(""));
                MarkedLocationUpdated.Invoke(this, new(""));
            };
            Tracker.StateLoaded += (_, _) =>
            {
                TrackedLocationUpdated.Invoke(this, new(""));
                MarkedLocationUpdated.Invoke(this, new(""));
            };
            Tracker.BossUpdated += (_, _) =>
            {
                TrackedLocationUpdated.Invoke(this, new(""));
                MarkedLocationUpdated.Invoke(this, new(""));
            };
        }

        public event PropertyChangedEventHandler TrackedLocationUpdated;

        public event PropertyChangedEventHandler MarkedLocationUpdated;

        /// <summary>
        /// If out of logic locations should be displayed on the tracker
        /// </summary>
        public bool ShowOutOfLogicLocations
        {
            get => _showOutOfLogicLocations;
            set
            {
                _showOutOfLogicLocations = value;
                OnLocationUpdated();
            }
        }

        public Tracker Tracker { get; private set; }

        public World World => Tracker?.World ?? new World(new Config(), "", 0, "");

        public Progression Progression => Tracker?.GetProgression(false) ?? new();

        public Progression ProgressionWithKeys => Tracker?.GetProgression(true) ?? new();

        public IEnumerable<Location> AllLocations => World.Locations.ToImmutableList();

        public IEnumerable<Location> AllClearableLocations => World.Locations.Where(x => IsLocationClearable(x)).ToImmutableList();

        public Dictionary<int, ItemData> MarkedLocations => Tracker?.MarkedLocations;

        /// <summary>
        /// Calls the event handlers when a location has been updated somehow
        /// </summary>
        /// <param name="location">
        /// The name of the location that was updated
        /// </param>
        /// <param name="updateToTrackedLocation">
        /// Whether a tracked location has been potentially updated
        /// </param>
        /// <param name="updateToMarkedLocation">
        /// Whether a marked location has been potentially updated
        /// </param>
        public void OnLocationUpdated(string location = null, bool updateToTrackedLocation = true, bool updateToMarkedLocation = true)
        {
            if (updateToTrackedLocation) TrackedLocationUpdated.Invoke(this, new(location));
            if (updateToMarkedLocation) MarkedLocationUpdated.Invoke(this, new(location));
        }

        /// <summary>
        /// Returns the regions sorted by the ones with highest number of
        /// accessible items, stickying the one that the user recently
        /// </summary>
        /// <param name="filter">LocationFilter for SM or Z3</param>
        /// <param name="applyStickyRegion">
        /// If the region the player updated last should be stickied at the top
        /// </param>
        /// <returns>
        /// A list of all regions sorted by the regions with the most items
        /// </returns>
        public IEnumerable<Region> GetTopRegions(LocationFilter filter = LocationFilter.None, bool applyStickyRegion = true)
        {
            var regions = World.Regions
                .Where(x => RegionMatchesFilter(filter, x))
                .OrderByDescending(x => x.Locations.Count(x => !x.Cleared && x.IsAvailable(Progression)))
                .ToList();

            if (applyStickyRegion && _stickyRegion != null && regions.Contains(_stickyRegion))
                regions.MoveToTop(x => x == _stickyRegion);

            return regions;
        }

        /// <summary>
        /// Returns the locations sorted by regions with the highest number of
        /// accessible items
        /// </summary>
        /// <param name="filter">LocationFilter for SM or Z3</param>
        /// <param name="applyStickyRegion">
        /// If the region the player updated last should be sticked at the top
        /// </param>
        /// <returns>
        /// A list of all locations sorted by the regions with the most items
        /// </returns>
        public IEnumerable<Location> GetTopLocations(LocationFilter filter = LocationFilter.None, bool applyStickyRegion = true)
        {
            return GetTopRegions(filter, applyStickyRegion).SelectMany(x => x.Locations)
                .Where(x => IsLocationClearable(x))
                .ToImmutableList();
        }

        /// <summary>
        /// Returns if a SMZ3 location is currently accessible
        /// </summary>
        /// <param name="location">The location to check</param>
        /// <param name="allowOutOfLogic">
        /// If out of logic checks should be returned, assuming the option is
        /// enabled
        /// </param>
        /// <param name="requireKeys">
        /// If we should check for required keys for this location or not
        /// </param>
        /// <returns>
        /// True if a location is accessible given settings, false otherwise
        /// </returns>
        public bool IsLocationClearable(Location location, bool allowOutOfLogic = true, bool requireKeys = false)
        {
            return !location.Cleared
                && ((location.IsAvailable(requireKeys ? Progression : ProgressionWithKeys) && SpecialLocationLogic(location))
                || (allowOutOfLogic && ShowOutOfLogicLocations));
        }

        /// <summary>
        /// Special logic for hiding particular locations from the tracker.
        /// Needs to be done because of how rewards work with the SMZ3 logic and because
        /// players can have medallions before seeing the dungeon
        /// </summary>
        /// <param name="location">The location to check</param>
        /// <returns>True if the location should be shown. False otherwise</returns>
        public bool SpecialLocationLogic(Location location)
        {
            // Don't show MM or TR unless we're sure we have the identified medallion or have all medallions
            if (location.Region is MiseryMire or TurtleRock)
            {
                var dungeonInfo = Tracker.WorldInfo.Dungeons.First(x => x.GetRegion(Tracker.World) == location.Region);
                return (dungeonInfo.Requirement == Medallion.Bombos && Progression.Bombos) ||
                    (dungeonInfo.Requirement == Medallion.Ether && Progression.Ether) ||
                    (dungeonInfo.Requirement == Medallion.Quake && Progression.Quake) ||
                    (Progression.Bombos && Progression.Ether && Progression.Quake);
            }
            // Don't show Mimic/Mirror cave unless TR is accessible or have all medallions
            else if (location == Tracker.World.LightWorldDeathMountainEast.MirrorCave)
            {
                var dungeonInfo = Tracker.WorldInfo.Dungeons.First(x => x.GetRegion(Tracker.World) == Tracker.World.TurtleRock);
                return (dungeonInfo.Requirement == Medallion.Bombos && Progression.Bombos) ||
                    (dungeonInfo.Requirement == Medallion.Ether && Progression.Ether) ||
                    (dungeonInfo.Requirement == Medallion.Quake && Progression.Quake) ||
                    (Progression.Bombos && Progression.Ether && Progression.Quake);
            }
            else return true;
        }

        /// <summary>
        /// Clears a location and fires the updated events
        /// </summary>
        /// <param name="location">The SMZ3 location to update</param>
        public void ClearLocation(Location location)
        {
            Tracker.Clear(location);
            _stickyRegion = location.Region;
        }

        /// <summary>
        /// Clears a list of locations and fires the updated events
        /// </summary>
        /// <param name="locations">A list of SMZ3 locations to update</param>
        public void ClearLocations(List<Location> locations)
        {
            locations.Where(x => !x.Cleared)
                .ToList()
                .ForEach(x => Tracker.Clear(x));
            if (locations.Select(x => x.Region).Count() == 1) _stickyRegion = locations.First().Region;
        }

        /// <summary>
        /// Clears an entire region/area
        /// </summary>
        /// <param name="region">The region to clear</param>
        /// <param name="trackItems">
        /// If items in the region should be tracked or not
        /// </param>
        /// <param name="assumeKeys">
        /// Set to true if keys should be ignored in the logic for clearing
        /// locations
        /// </param>
        public void ClearRegion(Region region, bool trackItems = false, bool assumeKeys = false)
        {
            if (region.Name == "Hyrule Castle")
            {
                assumeKeys = false;
            }

            Tracker.ClearArea(region, trackItems, false, null, assumeKeys);
        }

        /// <summary>
        /// Retrieve the appropriate progression object for the particular region
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public Progression ProgressionForRegion(Region region) =>
            region.World.Config.KeysanityForRegion(region) ? Progression : ProgressionWithKeys;

        /// <summary>
        /// Gets the primary name for the specified location.
        /// </summary>
        /// <param name="location">
        /// The location whose configured name to find.
        /// </param>
        /// <returns>
        /// The first name configured for <paramref name="location"/>.
        /// </returns>
        public string GetName(Location location) => Tracker.WorldInfo.Location(location).Name[0];

        /// <summary>
        /// Gets the primary name for the specified room.
        /// </summary>
        /// <param name="room">The room whose configured name to find.</param>
        /// <returns>
        /// The first name configured for <paramref name="room"/>.
        /// </returns>
        public string GetName(Room room) => Tracker.WorldInfo.Room(room).Name[0];

        /// <summary>
        /// Gets the primary name for the specified region.
        /// </summary>
        /// <param name="region">
        /// The region whose configured name to find.
        /// </param>
        /// <returns>
        /// The first name configured for <paramref name="region"/>.
        /// </returns>
        public string GetName(Region region) => Tracker.WorldInfo.Region(region).Name[0];

        /// <summary>
        /// Gets the primary name for the specified map location.
        /// </summary>
        /// <param name="mapLocation">The map location whose configured name to find.</param>
        /// <returns>The first name configured for <paramref name="mapLocation"/>.</returns>
        public string GetName(TrackerMapLocation mapLocation)
        {
            // TODO: Make this method unnecessary (e.g. instead of map locations, use the locations.json stuff I guess)
            var room = World.Rooms.SingleOrDefault(x => x.Name.Equals(mapLocation.Name, StringComparison.OrdinalIgnoreCase));
            if (room != null)
                return GetName(room);

            var location = World.Locations.SingleOrDefault(x => x.Name.Equals(mapLocation.Name, StringComparison.OrdinalIgnoreCase));
            if (location != null)
                return GetName(location);

            var dungeon = Tracker.WorldInfo.Dungeons.SingleOrDefault(x => x.Name.Contains(mapLocation.Name, StringComparison.OrdinalIgnoreCase));
            if (dungeon != null)
                return dungeon.Name[0];

            _logger?.LogWarning("Could not find matching room or location for map location {MapLocation} in {Region}", mapLocation.Name, mapLocation.Region);
            return mapLocation.Name;
        }

        /// <summary>
        /// Returns if a given region matches the LocationFilter
        /// </summary>
        /// <param name="filter">The filter to apply</param>
        /// <param name="region">The SMZ3 region to check</param>
        /// <returns>True if the region matches, false otherwise</returns>
        private static bool RegionMatchesFilter(LocationFilter filter, Region region) => filter switch
        {
            LocationFilter.None => true,
            LocationFilter.ZeldaOnly => region is Z3Region,
            LocationFilter.MetroidOnly => region is SMRegion,
            _ => throw new InvalidEnumArgumentException(nameof(filter), (int)filter, typeof(LocationFilter)),
        };
    }
}
