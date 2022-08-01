using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared.Enums
{
    /// <summary>
    /// Indicates the current tracking status of the location
    /// </summary>
    public enum LocationStatus
    {
        /// <summary>
        /// Already cleared by the player
        /// </summary>
        Cleared,

        /// <summary>
        /// Currently in logic
        /// </summary>
        Available,

        /// <summary>
        /// Currently in logic, but the player must do something else before
        /// it's available
        /// </summary>
        Relevant,

        /// <summary>
        /// This location is not currently available to the player
        /// </summary>
        OutOfLogic
    }
}
