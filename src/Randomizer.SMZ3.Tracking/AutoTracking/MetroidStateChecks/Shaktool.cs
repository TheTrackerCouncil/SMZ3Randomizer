namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for nearing Shaktool
    /// Player enters the room with the grapple block from the left
    /// </summary>
    public class Shaktool : IMetroidStateCheck
    {
        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Super Metroid</param>
        /// <param name="prevState">The previous state in Super Metroid</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(Tracker tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
        {
            if (currentState.CurrentRegion == 4 && currentState.CurrentRoomInRegion == 36 && prevState.CurrentRoomInRegion == 28)
            {
                tracker.SayOnce(x => x.AutoTracker.NearShaktool);
                return true;
            }
            return false;
        }
    }
}
