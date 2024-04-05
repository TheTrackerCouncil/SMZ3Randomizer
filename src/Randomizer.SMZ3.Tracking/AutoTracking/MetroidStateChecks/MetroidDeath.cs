using System.Linq;
using Randomizer.Abstractions;
using Randomizer.Data;
using Randomizer.Data.Tracking;
using Randomizer.Data.WorldData.Regions;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.SMZ3.Tracking.AutoTracking.MetroidStateChecks
{
    /// <summary>
    /// Metroid state check for detecting deaths in Metroid
    /// Checks if the player has 0 health and 0 reserve tanks
    /// </summary>
    public class MetroidDeath : IMetroidStateCheck
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemService"></param>
        public MetroidDeath(IItemService itemService)
        {
            Items = itemService;
        }

        /// <summary>
        /// Service for retrieving item data
        /// </summary>
        protected IItemService Items { get; }

        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Super Metroid</param>
        /// <param name="prevState">The previous state in Super Metroid</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(TrackerBase tracker, AutoTrackerMetroidState currentState, AutoTrackerMetroidState prevState)
        {
            if (currentState.Energy != 0 || prevState.Energy == 0 ||
                currentState.CurrentRoom == 0 && currentState is { CurrentRegion: 0, SamusY: 0 })
                return false;

            if (currentState is { ReserveTanks: > 0, AutoReserveTanks: true })
                return false;

            var silent = tracker.GameService!.PlayerRecentlyKilled;

            // Check if there is a special message for dying in this room
            var region = tracker.World.Regions.OfType<SMRegion>()
                .FirstOrDefault(x =>
                    x.MemoryRegionId == currentState.CurrentRegion && x.Metadata.WhenDiedInRoom != null &&
                    currentState.CurrentRoomInRegion != null &&
                    x.Metadata.WhenDiedInRoom.ContainsKey(currentState.CurrentRoomInRegion.Value));

            if (!silent && region is { Metadata.WhenDiedInRoom: not null } && currentState.CurrentRoomInRegion != null)
            {
                tracker.SayOnce(region.Metadata.WhenDiedInRoom[currentState.CurrentRoomInRegion.Value]);
            }

            tracker.TrackDeath(true);

            var death = Items.FirstOrDefault("Death");
            if (death is not null)
            {
                tracker.TrackItem(death, autoTracked: true, silent: silent);
                return true;
            }
            return false;
        }
    }
}
