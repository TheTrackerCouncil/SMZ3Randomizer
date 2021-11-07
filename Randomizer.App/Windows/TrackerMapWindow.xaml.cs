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
using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for TrackerMapWindow.xaml
    /// </summary>
    public partial class TrackerMapWindow : Window
    {
        private readonly ILogger<TrackerMapWindow> _logger;
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
        public TrackerMapWindow(TrackerMapConfig config, ILogger<TrackerMapWindow> logger)
        {
            InitializeComponent();

            _logger = logger;

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
                        var mapLocations = regions.Where(region => mapRegion.Name == region.Name)
                            .SelectMany(region => region.Rooms)
                            .Select(room => new TrackerMapLocation(mapRegion.Name, room.Name,
                                x: (int)Math.Floor(room.X * mapRegion.Scale) + mapRegion.X,
                                y: (int)Math.Floor(room.Y * mapRegion.Scale) + mapRegion.Y))
                            .ToList();
                        map.FullLocations.AddRange(mapLocations);
                    }
                }
            }

            TrackerMapViewModel = new TrackerMapViewModel();
            TrackerMapViewModel.MapNames = Maps.Select(x => x.Name).ToList();
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
            CurrentMap = Maps.First();
            TrackerMapViewModel.CurrentMap = CurrentMap;
        }

        /// <summary>
        /// Updates the current map to the option in the combo box when the
        /// combo box is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentMap = Maps.ElementAt(MapComboBox.SelectedIndex);
            TrackerMapViewModel.CurrentMap = CurrentMap;
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
                Syncer.ClearLocations(locations);
            }
            else if (shape.Tag is Region region)
            {
                Syncer.ClearRegion(region);
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

            Syncer.ClearLocation(location);
        }
    }
}
