﻿using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Regions.Zelda;
using Randomizer.SMZ3.Tracking.Configuration;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for detecting entering a dungeon
    /// Player is now in the dungeon state from the overworld in one of the designated starting rooms
    /// </summary>
    public class EnteredDungeon : IZeldaStateCheck
    {
        private readonly HashSet<DungeonInfo> _enteredDungeons = new();

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
                var region = tracker.World.Regions.Where(x => x is Z3Region)
                    .Select(x => x as Z3Region)
                    .FirstOrDefault(x => x != null && x.StartingRooms != null && x.StartingRooms.Contains(currentState.CurrentRoom) && !x.IsOverworld);
                if (region == null) return false;

                // Get the dungeon info for the room
                var dungeonInfo = tracker.WorldInfo.Dungeons.First(x => x.Is(region));

                if (!_enteredDungeons.Contains(dungeonInfo) && (dungeonInfo.RewardType == ItemType.PendantGreen || dungeonInfo.RewardType == ItemType.PendantNonGreen))
                {
                    var rewardName = tracker.ItemService.GetName(dungeonInfo.RewardType);
                    tracker.Say(tracker.Responses.AutoTracker.EnterPendantDungeon, dungeonInfo.Name, rewardName);
                }
                else if (region is CastleTower)
                {
                    tracker.Say(x => x.AutoTracker.EnterHyruleCastleTower);
                }
                else if (region is GanonsTower)
                {
                    var clearedCrystalDungeonCount = tracker.WorldInfo.Dungeons
                                                        .Where(x => x.Cleared)
                                                        .Select(x => x.GetRegion(tracker.World) as IHasReward)
                                                        .Count(x => x != null && x.RewardType is ItemType.CrystalBlue or ItemType.CrystalRed);
                    if (clearedCrystalDungeonCount < 7)
                    {
                        tracker.SayOnce(x => x.AutoTracker.EnteredGTEarly, clearedCrystalDungeonCount);
                    }
                }

                tracker.UpdateRegion(region, tracker.Options.AutoTrackerChangeMap);
                _enteredDungeons.Add(dungeonInfo);
                return true;
            }

            return false;
        }
    }
}
