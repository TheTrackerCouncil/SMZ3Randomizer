using System;
using System.Linq;

using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for when dying
    /// Checks if the player is in the death spiral animation without a fairy
    /// </summary>
    public class ZeldaDeath : IZeldaStateCheck
    {
        public ZeldaDeath(IItemService itemService)
        {
            Items = itemService;
        }

        public IItemService Items { get; }

        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Zelda</param>
        /// <param name="prevState">The previous state in Zelda</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(Tracker tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
        {
            if (tracker.AutoTracker == null) return false;

            if (currentState.State == 0x12 && prevState.State != 0x12 && !tracker.AutoTracker.PlayerHasFairy)
            {
                // Say specific message for dying in the particular screen/room the player is in
                if (tracker.CurrentRegion != null && tracker.CurrentRegion.WhenDiedInRoom != null)
                {
                    var region = tracker.CurrentRegion.GetRegion(tracker.World) as Z3Region;
                    if (region != null && region.IsOverworld && tracker.CurrentRegion.WhenDiedInRoom.ContainsKey(prevState.OverworldScreen))
                    {
                        tracker.Say(tracker.CurrentRegion.WhenDiedInRoom[prevState.OverworldScreen]);
                    }
                    else if (region != null && !region.IsOverworld && tracker.CurrentRegion.WhenDiedInRoom.ContainsKey(prevState.CurrentRoom))
                    {
                        tracker.Say(tracker.CurrentRegion.WhenDiedInRoom[prevState.CurrentRoom]);
                    }
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
