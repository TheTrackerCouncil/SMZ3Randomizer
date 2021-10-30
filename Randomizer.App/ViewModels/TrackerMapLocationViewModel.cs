using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Randomizer.SMZ3;
using Randomizer.SMZ3.Tracking;

namespace Randomizer.App.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a specific location on the map and its progress
    /// </summary>
    public class TrackerMapLocationViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMap"/> class
        /// with data to display a specific location on the map and its progress
        /// </summary>
        /// <param name="mapLocation">The TrackerMapLocation with details of where to place this location on the map</param>
        /// <param name="syncer">The LocationSyncer for keeping in sync with the data in the tracker</param>
        /// <param name="scaledRatio">The ratio in which the canvas has been scaled to fit in the grid</param>
        public TrackerMapLocationViewModel(TrackerMapLocation mapLocation, TrackerLocationSyncer syncer, double scaledRatio)
        {
            Size = 12;
            X = mapLocation.X * scaledRatio - Size / 2;
            Y = mapLocation.Y * scaledRatio - Size / 2;
            Syncer = syncer;
            Locations = Syncer.AllLocations.Where(loc => mapLocation.MatchesSMZ3Location(loc)).ToList();
            Name = mapLocation.Name;

            // To avoid multiple linq statements, let's loop through them all to calculate the results
            int numClearable = 0;
            int numOutOfLogic = 0;
            int numUncleared = 0;
            int numCleared = 0;

            Locations.ForEach(x =>
            {
                numClearable += Syncer.IsLocationClearable(x, false) ? 1 : 0;
                numOutOfLogic += Syncer.IsLocationClearable(x, true) && !Syncer.IsLocationClearable(x, false) ? 1 : 0;
                numUncleared += !x.Cleared ? 1 : 0;
                numCleared += x.Cleared ? 1 : 0;
            });

            ClearableLocationsCount = numClearable;
            OutOfLogicLocationsCount = numOutOfLogic;
            UnclearedLocationsCount = numUncleared;
            ClearedLocationsCount = numCleared;
        }

        /// <summary>
        /// The list of SMZ3 randomizer locations to look at to determine progress
        /// </summary>
        public List<Location> Locations { get; set; }

        /// <summary>
        /// The X value of where to display this location on the map
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The Y value of where to display this location on the map
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// The size of the square/circle to display
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// The name of the location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The number of available/accessible locations here that have not been cleared
        /// </summary>
        public int ClearableLocationsCount { get; set; }

        /// <summary>
        /// The total number of all locations here that have been cleared
        /// </summary>
        public int UnclearedLocationsCount { get; set; }

        public int OutOfLogicLocationsCount { get; set; }

        public int ClearedLocationsCount { get; set; }

        /// <summary>
        /// Display the green rectangle if there are available uncleared locations that match the number of total uncleared locations
        /// </summary>
        public Visibility RectangleVisibility => (ClearableLocationsCount > 0 && ClearableLocationsCount == UnclearedLocationsCount) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Display the orange circle if there are available uncleared locations, but it is less than the number of total uncleared locations
        /// </summary>
        public Visibility EllipseVisibility => (ClearableLocationsCount > 0 && ClearableLocationsCount < UnclearedLocationsCount) || (ClearableLocationsCount == 0 && OutOfLogicLocationsCount > 0) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Set the color of the icon based on if it's fully or partially clearable
        /// </summary>
        public Brush Color {
            get
            {
                SolidColorBrush brush = new SolidColorBrush();
                if (ClearableLocationsCount > 0 && ClearableLocationsCount == UnclearedLocationsCount)
                {
                    brush.Color = Colors.LightGreen;
                }
                else if (ClearableLocationsCount > 0 && ClearableLocationsCount < UnclearedLocationsCount)
                {
                    brush.Color = Colors.DarkOrange;
                }
                else if (ClearableLocationsCount == 0 && OutOfLogicLocationsCount > 0)
                {
                    brush.Color = Colors.DarkSlateGray;
                }
                else
                {
                    brush.Color = Colors.Magenta;
                }
                return brush;
            }
        }

        /// <summary>
        /// LocationSyncer to use for keeping locations in sync between the locations list and map
        /// </summary>
        protected TrackerLocationSyncer Syncer { get; }

    }
}
