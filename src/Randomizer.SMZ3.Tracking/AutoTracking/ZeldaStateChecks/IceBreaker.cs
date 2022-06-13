namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for performing the ice breaker trick
    /// player is on the right side of the wall but was previous in the room to the left
    /// </summary>
    public class IceBreaker : IZeldaStateCheck
    {
        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Zelda</param>
        /// <param name="prevState">The previous state in Zelda</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(Tracker tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
        {
            if (currentState.CurrentRoom == 31 && currentState.PreviousRoom == 30 && currentState.LinkX >= 8000 && prevState.LinkX < 8000 && currentState.IsOnRightHalfOfRoom && prevState.IsOnRightHalfOfRoom)
            {
                tracker.SayOnce(x => x.AutoTracker.IceBreaker);
                return true;
            }
            return false;
        }
    }
}
