using Randomizer.Abstractions;
using Randomizer.Data;
using Randomizer.Data.Tracking;

namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for nearing Crocomire
    /// Player is in the room above Crocomire and is near the door to Crocomire
    /// </summary>
    public class Crocomire : IMetroidStateCheck
    {
        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Super Metroid</param>
        /// <param name="prevState">The previous state in Super Metroid</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
        {
            if (currentState.CurrentRegion == 2 && currentState.CurrentRoomInRegion == 9 && currentState.SamusX >= 3000 && currentState.SamusY > 500)
            {
                tracker.SayOnce(x => x.AutoTracker.NearCrocomire, currentState.SuperMissiles, currentState.MaxSuperMissiles);
                return true;
            }
            return false;
        }
    }
}
