namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check related to Spore Spawn
    /// Checks if the player skips Spore Spawn
    /// </summary>
    public class SporeSpawn : IMetroidStateCheck
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
            if (currentState.CurrentRegion == 1 && currentState.CurrentRoomInRegion == 22 && prevState.CurrentRoomInRegion == 9)
            {
                tracker.SayOnce(x => x.AutoTracker.SkipSporeSpawn);
                return true;
            }
            return false;
        }
    }
}
