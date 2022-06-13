namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for performing the diver down trick
    /// Player is walking the lower water area from an unexpected direction
    /// </summary>
    public class DiverDown : IZeldaStateCheck
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
            // Back diver down
            if (currentState.CurrentRoom == 118 && currentState.LinkX < 3474 && (currentState.LinkX < 3400 || currentState.LinkX > 3430) && currentState.LinkY <= 3975 && prevState.LinkY > 3975 && (currentState.LinkState is 0 or 6 or 3) && currentState.IsOnBottomHalfOfroom && currentState.IsOnRightHalfOfRoom)
            {
                tracker.SayOnce(x => x.AutoTracker.DiverDown);
                return true;
            }
            // Left side diver down
            else if (currentState.CurrentRoom == 53 && currentState.PreviousRoom == 54 && currentState.LinkX > 2800 && currentState.LinkX < 2850 && currentState.LinkY <= 1915 && prevState.LinkY > 1915 && (currentState.LinkState is 0 or 6 or 3))
            {
                tracker.SayOnce(x => x.AutoTracker.DiverDown);
                return true;
            }
            return false;
        }
    }
}
