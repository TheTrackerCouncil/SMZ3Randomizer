using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigTypes
{
    /// <summary>
    /// Represents a Super Metroid keysanity door
    /// </summary>
    public class TrackerMapSMDoor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item">Item identifier for the key to unlock the door</param>
        /// <param name="x">X position on the map</param>
        /// <param name="y">Y position on the map</param>
        public TrackerMapSMDoor(string item, int x, int y)
        {
            Item = item;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Item identifier for the key to unlock the door
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// X position on the map
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y position on the map
        /// </summary>
        public int Y { get; set; }
    }
}
