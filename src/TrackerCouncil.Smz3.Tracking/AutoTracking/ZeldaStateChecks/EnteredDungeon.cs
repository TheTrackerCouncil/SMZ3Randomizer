using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;

namespace TrackerCouncil.Smz3.Tracking.AutoTracking.ZeldaStateChecks;

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
    /// <param name="trackerBase">The tracker instance</param>
    /// <param name="currentState">The current state in Zelda</param>
    /// <param name="prevState">The previous state in Zelda</param>
    /// <returns>True if the check was identified, false otherwise</returns>
    public bool ExecuteCheck(TrackerBase trackerBase, AutoTrackerZeldaState currentState, AutoTrackerZeldaState prevState)
    {
        if (currentState.State != 0x07 || prevState.State is not (0x06 or 0x09 or 0x0F or 0x10 or 0x11) || currentState.CurrentRoom == null)
        {
            return false;
        }

        // Get the region for the room
        var region = trackerBase.World.Regions
            .OfType<Z3Region>()
            .FirstOrDefault(x => x.StartingRooms.Count > 0 && x.StartingRooms.Contains(currentState.CurrentRoom.Value) && !x.IsOverworld);

        // Get the dungeon info for the room
        if (region is not IDungeon dungeon) return false;

        if (!_worldAccessor.World.Config.ZeldaKeysanity && !_enteredDungeons.Contains(region) && dungeon.IsPendantDungeon)
        {
            trackerBase.Say(x => x.AutoTracker.EnterPendantDungeon, args: [dungeon.DungeonMetadata.Name, dungeon.DungeonReward?.Metadata.Name]);
        }
        else if (!_worldAccessor.World.Config.ZeldaKeysanity && region is CastleTower)
        {
            trackerBase.Say(x => x.AutoTracker.EnterHyruleCastleTower);
        }
        else if (region is GanonsTower)
        {
            var clearedCrystalDungeonCount = trackerBase.World.Dungeons
                .Count(x => x.DungeonState.Cleared && x.IsCrystalDungeon);

            if (clearedCrystalDungeonCount < trackerBase.World.Config.GanonsTowerCrystalCount)
            {
                trackerBase.Say(x => x.AutoTracker.EnteredGTEarly, args: [clearedCrystalDungeonCount], once: true);
            }
        }

        trackerBase.UpdateRegion(region, trackerBase.Options.AutoTrackerChangeMap);
        _enteredDungeons.Add(region);
        return true;

    }
}
