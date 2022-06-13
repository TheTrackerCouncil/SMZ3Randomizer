namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for nearing Crocomire
    /// Player is in the room above Crocomire and is near the door to Crocomire
    /// </summary>
    public class Crocomire : MetroidStateCheck
    {
        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Zelda</param>
        /// <param name="prevState">The previous state in Zelda</param>
        public override bool ExecuteCheck(Tracker tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
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
