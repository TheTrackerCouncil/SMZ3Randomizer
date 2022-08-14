using System;
using System.Linq;

using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for detecting deaths in Metroid
    /// Checks if the player has 0 health and 0 reserve tanks
    /// </summary>
    public class MetroidDeath : IMetroidStateCheck
    {
        public MetroidDeath(IItemService itemService)
        {
            Items = itemService;
        }

        protected IItemService Items { get; }

        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Super Metroid</param>
        /// <param name="prevState">The previous state in Super Metroid</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(Tracker tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
        {
            if (currentState.Energy == 0 && currentState.ReserveTanks == 0 && prevState.Energy != 0 && !(currentState.CurrentRoom == 0 && currentState.CurrentRegion == 0 && currentState.SamusY == 0))
            {
                // Check if there is a special message for dying in this room
                var region = tracker.World.Regions.Select(x => x as SMRegion)
                    .Where(x => x != null && x.MemoryRegionId == currentState.CurrentRegion)
                    .Select(x => tracker.WorldInfo.Regions.FirstOrDefault(y => y.GetRegion(tracker.World) == x && y.WhenDiedInRoom != null))
                    .FirstOrDefault(x => x != null && x.WhenDiedInRoom != null && x.WhenDiedInRoom.ContainsKey(currentState.CurrentRoomInRegion));
                if (region != null && region.WhenDiedInRoom != null)
                {
                    tracker.SayOnce(region.WhenDiedInRoom[currentState.CurrentRoomInRegion]);
                }

                var death = Items.FindOrDefault("Death");
                if (death is not null)
                {
                    tracker.TrackItem(death);
                    return true;
                }
            }
            return false;
        }
    }
}
