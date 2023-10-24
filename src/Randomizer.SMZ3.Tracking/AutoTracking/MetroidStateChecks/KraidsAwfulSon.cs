using Randomizer.Abstractions;
using Randomizer.Data;
using Randomizer.Data.Tracking;

namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for nearing Kraid's awful son
    /// Player enters the same room as KAD from the left
    /// </summary>
    public class KraidsAwfulSon : IMetroidStateCheck
    {
        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Super Metroid</param>
        /// <param name="prevState">The previous state in Super Metroid</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(ITracker tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
        {
            if (currentState.CurrentRegion == 1 && currentState.CurrentRoomInRegion == 45 && prevState.CurrentRoomInRegion == 44)
            {
                tracker.SayOnce(x => x.AutoTracker.NearKraidsAwfulSon);
                return true;
            }
            return false;
        }
    }
}
