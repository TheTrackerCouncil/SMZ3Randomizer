using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Class used to serialize JSON responses from USB2SNES 
    /// </summary>
    public class USB2SNESResponse
    {
        /// <summary>
        /// List of all values returned by USB2SNES
        /// </summary>
        public ICollection<string>? Results { get; set; }
    }
}
