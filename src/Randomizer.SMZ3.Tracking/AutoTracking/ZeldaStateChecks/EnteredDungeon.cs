using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda;
using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for detecting entering a dungeon
    /// Player is now in the dungeon state from the overworld in one of the designated starting rooms
    /// </summary>
    public class EnteredDungeon : IZeldaStateCheck
    {
        private readonly HashSet<Region> _enteredDungeons = new();
        private readonly IWorldAccessor _worldAccessor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="worldAccessor"></param>
        public EnteredDungeon(IWorldAccessor worldAccessor)
        {
            _worldAccessor = worldAccessor;
        }

        /// <summary>
        /// Executes the check for the current state
        /// </summary>
        /// <param name="tracker">The tracker instance</param>
        /// <param name="currentState">The current state in Zelda</param>
        /// <param name="prevState">The previous state in Zelda</param>
        /// <returns>True if the check was identified, false otherwise</returns>
        public bool ExecuteCheck(Tracker tracker, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
        {
            if (currentState.State == 0x07 && (prevState.State == 0x06 || prevState.State == 0x09 || prevState.State == 0x0F || prevState.State == 0x10 || prevState.State == 0x11))
            {
                // Get the region for the room
                var region = tracker.World.Regions
                    .OfType<Z3Region>()
                    .FirstOrDefault(x => x.StartingRooms.Count == 0 && x.StartingRooms.Contains(currentState.CurrentRoom) && !x.IsOverworld);
                if (region == null) return false;

                // Get the dungeon info for the room
                var dungeon = region as IDungeon;
                if (dungeon == null) return false;

                if (!_worldAccessor.World.Config.ZeldaKeysanity && !_enteredDungeons.Contains(region) && dungeon.IsPendantDungeon)
                {
                    tracker.Say(tracker.Responses.AutoTracker.EnterPendantDungeon, dungeon.DungeonMetadata.Name, dungeon.DungeonReward?.Metadata.Name);
                }
                else if (!_worldAccessor.World.Config.ZeldaKeysanity && region is CastleTower)
                {
                    tracker.Say(x => x.AutoTracker.EnterHyruleCastleTower);
                }
                else if (region is GanonsTower)
                {
                    var clearedCrystalDungeonCount = tracker.World.Dungeons
                                                        .Count(x => x.DungeonState.Cleared && x.IsCrystalDungeon);

                    if (clearedCrystalDungeonCount < 7)
                    {
                        tracker.SayOnce(x => x.AutoTracker.EnteredGTEarly, clearedCrystalDungeonCount);
                    }
                }

                tracker.UpdateRegion(region, tracker.Options.AutoTrackerChangeMap);
                _enteredDungeons.Add(region);
                return true;
            }

            return false;
        }
    }
}
