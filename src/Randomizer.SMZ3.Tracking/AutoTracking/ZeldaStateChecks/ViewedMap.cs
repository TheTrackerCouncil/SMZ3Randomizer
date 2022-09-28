using System.Collections.Generic;
using System.Linq;

using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.SMZ3.Contracts;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData.Regions.Zelda.DarkWorld;
using Randomizer.Data.WorldData.Regions.Zelda.DarkWorld.DeathMountain;
using Randomizer.Data.WorldData.Regions.Zelda.LightWorld;
using Randomizer.Data.WorldData.Regions.Zelda.LightWorld.DeathMountain;
using Randomizer.SMZ3.Tracking.Services;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for viewing the map
    /// Checks if the game state is viewing the map and the value for viewing the full map is set
    /// </summary>
    public class ViewedMap : IZeldaStateCheck
    {
        private Tracker? _tracker;
        private readonly IWorldAccessor _worldAccessor;

        public ViewedMap(IWorldAccessor worldAccessor, IItemService itemService)
        {
            _worldAccessor = worldAccessor;
            Items = itemService;
        }

        protected World World => _worldAccessor.World;

        protected IItemService Items { get; }

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

            if (currentState.State == 14 && currentState.Substate == 7 && currentState.ReadUInt8(0xE0) == 0x80 && (tracker.AutoTracker.LatestViewAction == null || !tracker.AutoTracker.LatestViewAction.IsValid))
            {
                _tracker = tracker;
                var currentRegion = tracker.CurrentRegion?.GetRegion(tracker.World);
                if (currentRegion is LightWorldNorthWest or LightWorldNorthEast or LightWorldSouth or LightWorldDeathMountainEast or LightWorldDeathMountainWest)
                {
                    tracker.AutoTracker.LatestViewAction = new AutoTrackerViewedAction(UpdateLightWorldRewards);
                }
                else if (currentRegion is DarkWorldNorthWest or DarkWorldNorthEast or DarkWorldSouth or DarkWorldMire or DarkWorldDeathMountainEast or DarkWorldDeathMountainWest)
                {
                    tracker.AutoTracker.LatestViewAction = new AutoTrackerViewedAction(UpdateDarkWorldRewards);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Marks all of the rewards for the light world dungeons
        /// </summary>
        private void UpdateLightWorldRewards()
        {
            if (_tracker == null) return;

            var rewards = new List<RewardType>();
            var dungeons = new (Region Region, ItemType Map)[] {
                (World.EasternPalace, ItemType.MapEP),
                (World.DesertPalace, ItemType.MapDP),
                (World.TowerOfHera, ItemType.MapTH)
            };

            foreach (var (region, map) in dungeons)
            {
                if (World.Config.ZeldaKeysanity && !Items.IsTracked(map))
                    continue;

                var dungeon = (IDungeon)region;
                var rewardRegion = (IHasReward)region;
                if (dungeon.DungeonState.MarkedReward != dungeon.DungeonState.Reward)
                {
                    rewards.Add(rewardRegion.Reward.Type);
                    _tracker.SetDungeonReward(dungeon, rewardRegion.Reward.Type);
                }
            }

            if (!World.Config.ZeldaKeysanity && rewards.Count(x => x == RewardType.CrystalRed || x == RewardType.CrystalBlue) == 3)
            {
                _tracker.SayOnce(x => x.AutoTracker.LightWorldAllCrystals);
            }
            else if (rewards.Count == 0)
            {
                _tracker.Say(x => x.AutoTracker.LookedAtNothing);
            }
        }

        /// <summary>
        /// Marks all of the rewards for the dark world dungeons
        /// </summary>
        protected void UpdateDarkWorldRewards()
        {
            if (_tracker == null) return;

            var rewards = new List<RewardType>();
            var dungeons = new (Region Region, ItemType Map)[] {
                (World.PalaceOfDarkness, ItemType.MapPD),
                (World.SwampPalace, ItemType.MapSP),
                (World.SkullWoods, ItemType.MapSW),
                (World.ThievesTown, ItemType.MapTT),
                (World.IcePalace, ItemType.MapIP),
                (World.MiseryMire, ItemType.MapMM),
                (World.TurtleRock, ItemType.MapTR)
            };

            foreach (var (region, map) in dungeons)
            {
                if (World.Config.ZeldaKeysanity && !Items.IsTracked(map))
                    continue;

                var dungeon = (IDungeon)region;
                var rewardRegion = (IHasReward)region;
                if (dungeon.DungeonState.MarkedReward != dungeon.DungeonState.Reward)
                {
                    rewards.Add(rewardRegion.Reward.Type);
                    _tracker.SetDungeonReward(dungeon, rewardRegion.Reward.Type);
                }
            }

            var isMiseryMirePendant = World.MiseryMire.RewardType is RewardType.PendantGreen or RewardType.PendantRed or RewardType.PendantBlue;
            var isTurtleRockPendant = World.TurtleRock.RewardType is RewardType.PendantGreen or RewardType.PendantRed or RewardType.PendantBlue;

            if (!World.Config.ZeldaKeysanity && isMiseryMirePendant && isTurtleRockPendant)
            {
                _tracker.SayOnce(x => x.AutoTracker.DarkWorldNoMedallions);
            }
            else if (rewards.Count == 0)
            {
                _tracker.Say(x => x.AutoTracker.LookedAtNothing);
            }

        }
    }
}
