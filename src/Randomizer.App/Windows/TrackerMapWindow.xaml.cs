using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

using Microsoft.Extensions.Logging;

using Randomizer.App.ViewModels;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.SMZ3;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigTypes;
using static Randomizer.Data.Configuration.ConfigTypes.TrackerMapLocation;
using Randomizer.SMZ3.Tracking;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for TrackerMapWindow.xaml
    /// </summary>
    public partial class TrackerMapWindow : Window
    {
        private readonly ILogger<TrackerMapWindow> _logger;
        private readonly Tracker _tracker;
        private TrackerLocationSyncer _syncer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMapWindow"/>
        /// class to display the map of accessible locations that the player can
        /// go to
        /// </summary>
        /// <param name="config">
        /// The config for the map json file with all the location details
        /// </param>
        /// <param name="logger">Logger for logging</param>
        public TrackerMapWindow(Tracker tracker, TrackerLocationSyncer syncer, TrackerMapConfig config, ILogger<TrackerMapWindow> logger)
        {
            InitializeComponent();

            _logger = logger;
            _tracker = tracker;
            _syncer = syncer;

            Maps = config.Maps;
            var regions = config.Regions;

            // To make querying easier for the viewmodel, compile a list of all
            // locations on each map by combining all of their regions, making
            // any location adjusments needed
            foreach (var map in Maps)
            {
                map.FullLocations = new List<TrackerMapLocation>();
                if (map.Regions != null)
                {
                    foreach (var mapRegion in map.Regions)
                    {
                        var region = regions.First(region => mapRegion.Name == region.Name);
                        var mapLocations = region.Rooms
                            .Select(room => new TrackerMapLocation(MapLocationType.Item, mapRegion.Name, region.TypeName, room.Name,
                                x: (int)Math.Floor(room.X * mapRegion.Scale) + mapRegion.X,
                                y: (int)Math.Floor(room.Y * mapRegion.Scale) + mapRegion.Y))
                            .ToList();
                        map.FullLocations.AddRange(mapLocations);

                        // Add the boss for this region if one is specified
                        if (region.BossX != null && region.BossY != null)
                        {
                            map.FullLocations.Add(new TrackerMapLocation(MapLocationType.Boss, mapRegion.Name, region.TypeName, null,
                                x: (int)Math.Floor((region.BossX ?? 0) * mapRegion.Scale) + mapRegion.X,
                                y: (int)Math.Floor((region.BossY ?? 0) * mapRegion.Scale) + mapRegion.Y));
                        }

                        // Add all SM keysanity doors
                        if (region.Doors != null)
                        {
                            foreach (var door in region.Doors)
                            {
                                map.FullLocations.Add(new TrackerMapLocation(MapLocationType.SMDoor, mapRegion.Name, region.TypeName, door.Item,
                                    x: (int)Math.Floor((door.X) * mapRegion.Scale) + mapRegion.X,
                                    y: (int)Math.Floor((door.Y) * mapRegion.Scale) + mapRegion.Y));
                            }
                        }
                    }
                }
            }

            TrackerMapViewModel = new TrackerMapViewModel();
            TrackerMapViewModel.MapNames = Maps.Select(x => x.ToString()).ToList();
            DataContext = TrackerMapViewModel;

            App.RestoreWindowPositionAndSize(this);
        }

        /// <summary>
        /// A collection of all of the maps that the player can choose from
        /// </summary>
        public IReadOnlyCollection<TrackerMap> Maps { get; }

        /// <summary>
        /// The current selected map
        /// </summary>
        public TrackerMap CurrentMap { get; private set; }

        /// <summary>
        /// The view model for the tracker map window
        /// </summary>
        public TrackerMapViewModel TrackerMapViewModel { get; set; }

        public TrackerLocationSyncer Syncer
        {
            get
            {
                return _syncer;
            }
            set
            {
                TrackerMapViewModel.Syncer = value;
                value.TrackedLocationUpdated += PropertyChanged;
                _syncer = value;
            }
        }

        /// <summary>
        /// Updates the displayed map
        /// </summary>
        /// <param name="mapName">The name of the map to display</param>
        public void UpdateMap(string mapName)
        {
            if (mapName == null)
                return;

            UpdateMap(Maps.First(x => x.ToString() == mapName));
        }

        /// <summary>
        /// Updates the displayed map
        /// </summary>
        /// <param name="map">The map to display</param>
        public void UpdateMap(TrackerMap map)
        {
            CurrentMap = map;
            TrackerMapViewModel.CurrentMap = CurrentMap;
        }

        /// <summary>
        /// When the canvas's parent grid size changes. Used for updating the
        /// canvas size to the correct proportions.
        /// </summary>
        /// <param name="sender">The original propagator of the change</param>
        /// <param name="e">The event with the resize information</param>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TrackerMapViewModel.GridSize = e.NewSize;
        }

        /// <summary>
        /// When the window has fully loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Maps == null || Maps.Count == 0)
            {
                _logger.LogError("Map json was not loaded successfully");
                return;
            }
            UpdateMap(Maps.Last());
        }

        /// <summary>
        /// Updates the current map to the option in the combo box when the
        /// combo box is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _tracker.UpdateMap(Maps.ElementAt(MapComboBox.SelectedIndex).ToString());
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TrackerMapViewModel.OnPropertyChanged();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            App.SaveWindowPositionAndSize(this);
        }

        /// <summary>
        /// Called when a location is clicked on the map to clear out all
        /// affiliated SMZ3 locations
        /// </summary>
        /// <param name="sender">The rectangle/ellipse that was clicked</param>
        /// <param name="e"></param>
        private void Location_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Because we have to do this a gross way, let's at least be sure
            // this is originating from the correct object with Location data
            if (sender is not Rectangle and not Ellipse)
                return;

            var shape = (Shape)sender;

            // Determine what type of location this is
            if (shape.Tag is List<Location> locations)
            {
                locations.Where(x => !x.State.Cleared)
                    .ToList()
                    .ForEach(x => _tracker.Clear(x));
            }
            else if (shape.Tag is Region region)
            {
                _tracker.ClearArea(region, false, false, null, region.Name != "Hyrule Castle");
            }
            else if (shape.Tag is Boss boss)
            {
                _tracker.MarkBossAsDefeated(boss);
            }
            else if(shape.Tag is IDungeon dungeon)
            {
                _tracker.MarkDungeonAsCleared(dungeon);
            }
            
        }

        /// <summary>
        /// Clicked on the right click menu for clearing an individual location
        /// inside of a room or region
        /// </summary>
        /// <param name="sender">The menu item that was clicked</param>
        /// <param name="e"></param>
        private void LocationContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (menuItem.Tag is not Location location)
                return;

            _tracker.Clear(location);
        }
    }
}
