using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// A representation of a map that a user can select with multiple regions on it.
    /// </summary>
    public class TrackerMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerMap"/> class
        /// with data to display the map to the user
        /// </summary>
        /// <param name="name">The name of the map in the dropdown menu</param>
        /// <param name="image">The image file to use for the map</param>
        /// <param name="width">The width of the map image in pixels</param>
        /// <param name="height">The height of the map image in pixels</param>
        /// <param name="regions">A list of all regions that are a part of this map</param>
        public TrackerMap(string name, string image, int width, int height, IReadOnlyCollection<TrackerMapLocation> regions)
        {
            Name = name;
            Image = image;
            Width = width;
            Height = height;
            Regions = regions;
            FullLocations = new();
        }

        /// <summary>
        /// The name of the map in the dropdown menu
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The image file to use for the map
        /// </summary>
        public string Image { get; }

        /// <summary>
        /// The width of the map image in pixels
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the map image in pixels
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// A list of all of the regions that are a part of this map and where they
        /// should be placed on the map
        /// </summary>
        public IReadOnlyCollection<TrackerMapLocation> Regions { get; }

        /// <summary>
        /// List of all actual locations that are underneath the region
        /// </summary>
        public List<TrackerMapLocation> FullLocations { get; set; }
    }
}
