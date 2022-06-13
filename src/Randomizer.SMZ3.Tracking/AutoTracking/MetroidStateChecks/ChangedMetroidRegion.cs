using System.Linq;

namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for changing regions
    /// Checks if the game is in the overworld state and was either not in the overworld state previously or has changed overworld screens
    /// </summary>
    public class ChangedMetroidRegion : MetroidStateCheck
    {
        private int _previousMetroidRegionValue = -1;

        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Zelda</param>
        /// <param name="prevState">The previous state in Zelda</param>
        public override bool ExecuteCheck(Tracker tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
        {
            if (currentState.CurrentRegion != _previousMetroidRegionValue)
            {
                var newRegion = tracker.World.Regions.Select(x => x as SMRegion).FirstOrDefault(x => x != null && x.MemoryRegionId == currentState.CurrentRegion);
                if (newRegion != null)
                {
                    tracker.UpdateRegion(newRegion, tracker.Options.AutoTrackerChangeMap);
                }
                _previousMetroidRegionValue = currentState.CurrentRegion;
                return true;
            }
            return false;
        }
    }
}
