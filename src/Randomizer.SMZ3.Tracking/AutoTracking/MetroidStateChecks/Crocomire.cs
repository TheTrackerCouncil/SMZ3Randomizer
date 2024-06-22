﻿using Randomizer.Abstractions;
using Randomizer.Data;
using Randomizer.Data.Tracking;
using Randomizer.Shared;

namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for nearing Crocomire
    /// Player is in the room above Crocomire and is near the door to Crocomire
    /// </summary>
    public class Crocomire(IItemService itemService) : IMetroidStateCheck
    {

        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Super Metroid</param>
        /// <param name="prevState">The previous state in Super Metroid</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
        {
            if (currentState is { CurrentRegion: 2, CurrentRoomInRegion: 9, SamusX: >= 3000, SamusY: > 500 } && (!tracker.World.Config.MetroidKeysanity || itemService.IsTracked(ItemType.CardNorfairBoss)))
            {
                tracker.SayOnce(x => x.AutoTracker.NearCrocomire, currentState.SuperMissiles, currentState.MaxSuperMissiles);
                return true;
            }
            return false;
        }
    }
}
