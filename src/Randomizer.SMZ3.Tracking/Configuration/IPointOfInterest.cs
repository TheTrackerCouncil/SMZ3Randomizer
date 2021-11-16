using System;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Defines a potential point of interest on the map.
    /// </summary>
    public interface IPointOfInterest
    {
        /// <summary>
        /// Gets the x-coordinate of the POI on the map, if it should be
        /// displayed.
        /// </summary>
        public int? X { get; }

        /// <summary>
        /// Gets the y-coordinate of the POI on the map, if it should be
        /// displayed.
        /// </summary>
        public int? Y { get; }

        /// <summary>
        /// Gets the scale of the POI, if it should be displayed on a map.
        /// </summary>
        public double? Scale { get; }
    }
}
