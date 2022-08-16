using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
{
    /// <summary>
    /// Represents a single spot in the Tracker UI
    /// </summary>
    public class UIGridLocation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UIGridLocation() { }

        /// <summary>
        /// The type of object this location represents
        /// </summary>
        public UIGridLocationType Type { get; set; }

        /// <summary>
        /// The row in the UI where this spot is located
        /// </summary>
        public int Row { get; init; }

        /// <summary>
        /// The column in the UI where this spot is located
        /// </summary>
        public int Column { get; init; }

        /// <summary>
        /// Image to display in this location
        /// </summary>
        public string? Image { get; init; }

        /// <summary>
        /// Collection of object identifiers to look up for this location
        /// </summary>
        public ICollection<string> Identifiers { get; set; }
    }
}
