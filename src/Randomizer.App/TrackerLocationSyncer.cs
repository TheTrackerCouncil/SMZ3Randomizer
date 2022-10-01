using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Windows;

using Microsoft.Extensions.Logging;
using Randomizer.Data;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Shared.Enums;
using Randomizer.SMZ3.Tracking.Services;

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
        public TrackerLocationSyncer(Tracker tracker, IItemService itemService, IWorldService worldService, ILogger<TrackerLocationSyncer> logger)
        {
            Tracker = tracker;
            ItemService = itemService;
            WorldService = worldService;
            _logger = logger;

            // Set all events from the tracker to point to the two in this class
            Tracker.MarkedLocationsUpdated += (_, _) => MarkedLocationUpdated.Invoke(this, new(""));
            Tracker.LocationCleared += (_, e) =>
            {
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

        public IItemService ItemService { get; private set; }

        public IWorldService WorldService { get; private set; }

        public World World => Tracker?.World ?? new World(new Config(), "", 0, "");

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
    }
}
