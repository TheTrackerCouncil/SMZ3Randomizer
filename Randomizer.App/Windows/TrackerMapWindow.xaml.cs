using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Extensions.Logging;
using Randomizer.SMZ3.Tracking;
using System.IO;
using Randomizer.App.ViewModels;
using System.ComponentModel;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for TrackerMapWindow.xaml
    /// </summary>
    public partial class TrackerMapWindow : Window
    {
        private ILogger<TrackerMapWindow> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMapWindow"/> class to display the map of accessible
        /// locations that the player can go to
        /// </summary>
        /// <param name="trackerMapConfigProvider">The config provider for the map json file with all the location details</param>
        /// <param name="logger">Logger for logging</param>
        public TrackerMapWindow(TrackerMapConfigProvider trackerMapConfigProvider, ILogger<TrackerMapWindow> logger)
        {
            InitializeComponent();

            _logger = logger;

            Maps = trackerMapConfigProvider.GetTrackerMapConfig()?.Maps;
            var regions = trackerMapConfigProvider.GetTrackerMapConfig()?.Regions;

            // To make querying easier for the viewmodel, compile a list of all locations on each map by combining all of their regions,
            // making any location adjusments needed
            foreach (TrackerMap map in Maps) {
                map.FullLocations = new List<TrackerMapLocation>();
                if (map.Regions != null)
                {
                    foreach (TrackerMapLocation mapRegion in map.Regions)
                    {
                        map.FullLocations.AddRange(regions.Where(region => mapRegion.Name == region.Name).SelectMany(region => region.Rooms).Select(room => new TrackerMapLocation(mapRegion.Name, room.Name, (int)Math.Floor((double)room.X * mapRegion.Scale) + mapRegion.X, (int)Math.Floor((double)room.Y * mapRegion.Scale) + mapRegion.Y)).ToList());
                    }
                }
            }

            TrackerMapViewModel = new TrackerMapViewModel();
            TrackerMapViewModel.MapNames = Maps.Select(x => x.Name).ToList();
            DataContext = TrackerMapViewModel;
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

        /// <summary>
        /// Connects the regular tracker and map view models so that we can update when
        /// it updates
        /// </summary>
        /// <param name="trackerViewModel">The regular tracker view model</param>
        public void SetupTrackerViewModel (TrackerViewModel trackerViewModel)
        {
            TrackerMapViewModel.TrackerViewModel = trackerViewModel;
            trackerViewModel.PropertyChanged += TrackerViewModel_PropertyChanged;
        }

        /// <summary>
        /// When the canvas's parent grid size changes. Used for updating the canvas size to the correct proportions.
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
        /// Updates the current map to the option in the combo box when the combo box is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentMap = Maps.ElementAt(MapComboBox.SelectedIndex);
            TrackerMapViewModel.CurrentMap = CurrentMap;
        }

        private void TrackerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TrackerMapViewModel.OnPropertyChanged();
        }
    }
}
