using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        /// <param name="locations">The list of SMZ3 randomizer locations to look at to determine progress</param>
        /// <param name="progression">The progression object to determine current progress at the location</param>
        /// <param name="progressionWithKeys">The progression object to determine current progress at the location</param>
        /// <param name="scaledRatio">The ratio in which the canvas has been scaled to fit in the grid</param>
        public TrackerMapLocationViewModel(TrackerMapLocation mapLocation, List<Location> locations, Progression progression, Progression progressionWithKeys, double scaledRatio)
        {
            Size = 12;
            X = mapLocation.X * scaledRatio - Size / 2;
            Y = mapLocation.Y * scaledRatio - Size / 2;
            Progression = progression;
            ProgressionWithKeys = progressionWithKeys;
            Locations = locations;
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
        /// The number of available/accessible locations here that have not been cleared
        /// </summary>
        public int ClearableLocationCount => Locations == null ? 0 : Locations.Where(x => (x.IsAvailable(Progression) || x.IsAvailable(ProgressionWithKeys)) && !x.Cleared).Count();

        /// <summary>
        /// The total number of all locations here that have been cleared
        /// </summary>
        public int TotalUnclearedLocationCount => Locations == null ? 0 : Locations.Where(x => !x.Cleared).Count();

        /// <summary>
        /// Display the green rectangle if there are available uncleared locations that match the number of total uncleared locations
        /// </summary>
        public Visibility RectangleVisibility => (ClearableLocationCount > 0 && ClearableLocationCount == TotalUnclearedLocationCount) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Display the orange circle if there are available uncleared locations, but it is less than the number of total uncleared locations
        /// </summary>
        public Visibility EllipseVisibility => (ClearableLocationCount > 0 && ClearableLocationCount < TotalUnclearedLocationCount) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Progression object to use to determine if a location is available or not
        /// </summary>
        protected Progression Progression { get; }

        /// <summary>
        /// Progression object to use to determine if a location is available or not
        /// </summary>
        protected Progression ProgressionWithKeys { get; }

    }
}
