namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for performing the specky clip trick
    /// player was on top of the horizontal wall and is now below it
    /// </summary>
    public class SpeckyClip : IZeldaStateCheck
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
            var inCorrectLocation = currentState is
                { CurrentRoom: 55, IsOnBottomHalfOfRoom: true, IsOnRightHalfOfRoom: false };
            var prevInWall = prevState.LinkY is >= 1845 and <= 1855;
            var nowBelowWall = currentState is
                { LinkX: >= 3665, LinkX: <= 3695, LinkY: >= 1858, LinkY: <= 1875, CurrentRoom: 55 };
            if (inCorrectLocation && prevInWall && nowBelowWall)
            {
                tracker.Say(x => x.AutoTracker.SpeckyClip);
                return true;
            }
            return false;
        }
    }
}
