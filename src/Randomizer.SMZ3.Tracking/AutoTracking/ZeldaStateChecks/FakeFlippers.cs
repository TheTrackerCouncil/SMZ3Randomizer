using System.Linq;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for performing fake flippers
    /// Checks if the player is in the swimming state for two states without the flippers
    /// </summary>
    public class FakeFlippers : IZeldaStateCheck
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
            return false;
            /*if (currentState.LinkState == 0x04 && prevState.LinkState == 0x04 && tracker.Items.Any(x => x.InternalItemType == ItemType.Flippers && x.TrackingState == 0))
            {
                tracker.SayOnce(x => x.AutoTracker.FakeFlippers);
                return true;
            }
            return false;*/
        }
    }
}
