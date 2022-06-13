using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Class used to serialize JSON requests to USB2SNES
    /// </summary>
    public class USB2SNESRequest
    {
        /// <summary>
        /// The type of request being sent to USB2SNES
        /// </summary>
        public string? Opcode { get; set; }
        /// <summary>
        /// Where to get the data from in USB2SNES (always should be SNES probably?)
        /// </summary>
        public string? Space { get; set; }
        /// <summary>
        /// Parameters for the request
        /// </summary>
        public ICollection<string>? Operands { get; set; }
    }
}
