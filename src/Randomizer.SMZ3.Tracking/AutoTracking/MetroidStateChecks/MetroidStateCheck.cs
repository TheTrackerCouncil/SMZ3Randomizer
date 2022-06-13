namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Abstract class for various Metroid state checks
    /// </summary>
    public abstract class MetroidStateCheck
    {
        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Zelda</param>
        /// <param name="prevState">The previous state in Zelda</param>
        public abstract bool ExecuteCheck(Tracker tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState);
    }
}
