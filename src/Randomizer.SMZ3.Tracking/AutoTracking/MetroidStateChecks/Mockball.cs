using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for performing the Mockball trick
    /// Player gets past the speed gates without the speedbooster
    /// </summary>
    public class Mockball : IMetroidStateCheck
    {
        private readonly ItemService _itemService;

        public Mockball(ItemService itemService)
        {
            _itemService = itemService;
        }

        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Super Metroid</param>
        /// <param name="prevState">The previous state in Super Metroid</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(Tracker tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
        {
            if (_itemService.IsTracked(ItemType.SpeedBooster))
                return false;

            // Brinstar Mockball
            if (currentState.CurrentRegion == 1
                && currentState.CurrentRoomInRegion == 3
                && currentState.SamusX >= 560
                && prevState.SamusX < 560
                && currentState.SamusX < 800)
            {
                tracker.SayOnce(x => x.AutoTracker.MockBall);
                return true;
            }
            // Norfair Mockball
            else if (currentState.CurrentRegion == 2
                     && currentState.CurrentRoomInRegion == 4
                     && currentState.SamusX <= 1016
                     && prevState.SamusX > 1016
                     && currentState.SamusX > 800)
            {
                tracker.SayOnce(x => x.AutoTracker.MockBall);
                return true;
            }
            return false;
        }
    }
}
