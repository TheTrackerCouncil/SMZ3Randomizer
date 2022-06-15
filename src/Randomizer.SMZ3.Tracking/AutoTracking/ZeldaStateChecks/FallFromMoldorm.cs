namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for falling from the moldorm room(s)
    /// Checks if the current room is the one below moldorm and the previous room was the moldorm room
    /// </summary>
    public class FallFromMoldorm : IZeldaStateCheck
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
            // Tower of Hera
            if (currentState.CurrentRoom == 23 && currentState.PreviousRoom == 7 && prevState.CurrentRoom == 7)
            {
                tracker.SayOnce(x => x.AutoTracker.FallFromMoldorm);
                return true;
            }
            // Ganon's Tower
            else if (currentState.CurrentRoom == 166 && currentState.PreviousRoom == 77 && prevState.CurrentRoom == 77)
            {
                tracker.SayOnce(x => x.AutoTracker.FallFromMoldorm);
                return true;
            }
            return false;
        }
    }
}
