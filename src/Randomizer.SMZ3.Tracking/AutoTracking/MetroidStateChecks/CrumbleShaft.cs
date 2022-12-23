namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for reaching Crumble Shaft
    /// Player enters the room from the door at the top
    /// </summary>
    public class CrumbleShaft : IMetroidStateCheck
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
            if (currentState.CurrentRegion == 2 && currentState.CurrentRoomInRegion == 8 && prevState.CurrentRoomInRegion == 4)
            {
                tracker.SayOnce(x => x.AutoTracker.AtCrumbleShaft);
                return true;
            }
            return false;
        }
    }
}
