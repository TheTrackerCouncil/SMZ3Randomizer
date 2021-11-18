using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.App
{
    /// <summary>
    /// This is a shared class that is meant to act as an intermediary between the tracker and the location/map windows
    /// to keep everything in sync as well as clean up some of the logic in view models
    /// </summary>
    public class TrackerLocationSyncer
    {
        private bool _isDesign;
        private Region _stickyRegion = null;
        private Tracker _tracker = null;
        private bool _showOutOfLogicLocations;

        public TrackerLocationSyncer()
        {
            _isDesign = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }

        /// <summary>
        /// Creates a new instance of the TrackerLocationSyncer that will be synced with a given tracker
        /// </summary>
        /// <param name="tracker">The tracker to keep things in sync with</param>
        public TrackerLocationSyncer(Tracker tracker)
        {
            _tracker = tracker;

            // Set all events from the tracker to point to the two in this class
            _tracker.MarkedLocationsUpdated += (_, _) => MarkedLocationUpdated.Invoke(this, new(""));
            _tracker.LocationCleared += (_, e) =>
            {
                _stickyRegion = e.Location.Region;
                TrackedLocationUpdated.Invoke(this, new(e.Location.Name));
                MarkedLocationUpdated.Invoke(this, new(e.Location.Name));
            };
            _tracker.DungeonUpdated += (_, _) =>
            {
                TrackedLocationUpdated.Invoke(this, new(""));
                MarkedLocationUpdated.Invoke(this, new(""));
            };
            _tracker.ItemTracked += (_, _) =>
            {
                TrackedLocationUpdated.Invoke(this, new(""));
                MarkedLocationUpdated.Invoke(this, new(""));
            };
            _tracker.ActionUndone += (_, _) =>
            {
                TrackedLocationUpdated.Invoke(this, new(""));
                MarkedLocationUpdated.Invoke(this, new(""));
            };
            _tracker.StateLoaded += (_, _) =>
            {
                TrackedLocationUpdated.Invoke(this, new(""));
                MarkedLocationUpdated.Invoke(this, new(""));
            };
        }

        public event PropertyChangedEventHandler TrackedLocationUpdated;

        public event PropertyChangedEventHandler MarkedLocationUpdated;

        /// <summary>
        /// Calls the event handlers when a location has been updated somehow
        /// </summary>
        /// <param name="location">The name of the location that was updated</param>
        /// <param name="updateToTrackedLocation">Whether a tracked location has been potentially updated</param>
        /// <param name="updateToMarkedLocation">Whether a marked location has been potentially updated</param>
        public void OnLocationUpdated(string location = null , bool updateToTrackedLocation = true, bool updateToMarkedLocation = true)
        {
            if (updateToTrackedLocation) TrackedLocationUpdated.Invoke(this, new(location));
            if (updateToMarkedLocation) MarkedLocationUpdated.Invoke(this, new(location));
        }

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

        public World World => _tracker?.World ?? new World(new Config(), "", 0, "");

        public Progression Progression => _tracker?.GetProgression() ?? new();

        public Progression ProgressionWithKeys => _tracker?.GetProgression(true) ?? new();

        public IEnumerable<Location> AllLocations => World.Locations.ToImmutableList();

        public IEnumerable<Location> AllClearableLocations => World.Locations.Where(x => IsLocationClearable(x)).ToImmutableList();

        /// <summary>
        /// Returns the regions sorted by the ones with highest number of accessible items, stickying the one that the user recently
        /// </summary>
        /// <param name="filter">LocationFilter for SM or Z3</param>
        /// <param name="applyStickyRegion">If the region the player updated last should be stickied at the top</param>
        /// <returns>A list of all regions sorted by the regions with the most items</returns>
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
        /// Returns the locations sorted by regions with the highest number of accessible items
        /// </summary>
        /// <param name="filter">LocationFilter for SM or Z3</param>
        /// <param name="applyStickyRegion">If the region the player updated last should be sticked at the top</param>
        /// <returns>A list of all locations sorted by the regions with the most items</returns>
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
        /// <param name="allowOutOfLogic">If out of logic checks should be returned, assuming the option is enabled</param>
        /// <param name="requireKeys">If we should check for required keys for this location or not</param>
        /// <returns>True if a location is accessible given settings, false otherwise</returns>
        public bool IsLocationClearable(Location location, bool allowOutOfLogic = true, bool requireKeys = false)
        {
            return !location.Cleared
                && (location.IsAvailable(Progression)
                || (!requireKeys && location.IsAvailable(ProgressionWithKeys))
                || (allowOutOfLogic && ShowOutOfLogicLocations));
        }

        /// <summary>
        /// Clears a location and fires the updated events
        /// </summary>
        /// <param name="location">The SMZ3 location to update</param>
        public void ClearLocation(Location location)
        {
            _tracker.Clear(location);
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
                .ForEach(x => _tracker.Clear(x));
            if (locations.Select(x => x.Region).Count() == 1) _stickyRegion = locations.First().Region;
        }

        /// <summary>
        /// Clears an entire region/area
        /// </summary>
        /// <param name="region">The region to clear</param>
        /// <param name="trackItems">If items in the region should be tracked or not</param>
        /// <param name="assumeKeys">Set to true if keys should be ignored in the logic for clearing locations</param>
        public void ClearRegion(Region region, bool trackItems = false, bool assumeKeys = false)
        {
            if (region.Name == "Hyrule Castle")
            {
                assumeKeys = false;
            }

            _tracker.ClearArea(region, trackItems, false, null, assumeKeys);
        }

        public Dictionary<int, ItemData> MarkedLocations =>  _tracker?.MarkedLocations;

        /// <summary>
        /// Returns if a given region matches the LocationFilter
        /// </summary>
        /// <param name="filter">The filter to apply</param>
        /// <param name="region">The SMZ3 region to check</param>
        /// <returns>True if the region matches, false otherwise</returns>
        private bool RegionMatchesFilter(LocationFilter filter, Region region)
        {
            switch (filter)
            {
                case LocationFilter.None: return true;
                case LocationFilter.ZeldaOnly: return region is Z3Region;
                case LocationFilter.MetroidOnly: return region is SMRegion;
                default: throw new InvalidEnumArgumentException(nameof(filter), (int)filter, typeof(LocationFilter));
            }
        }
    }
}
