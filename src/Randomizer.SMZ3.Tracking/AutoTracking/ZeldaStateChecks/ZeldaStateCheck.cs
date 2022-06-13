namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Abstract for Zelda state checks
    /// </summary>
    public abstract class ZeldaStateCheck
    {
        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Zelda</param>
        /// <param name="prevState">The previous state in Zelda</param>
        public abstract bool ExecuteCheck(Tracker tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState);
    }
}
