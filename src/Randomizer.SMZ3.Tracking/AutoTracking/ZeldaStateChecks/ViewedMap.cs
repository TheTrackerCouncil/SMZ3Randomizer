using System.Collections.Generic;
using System.Linq;

using Randomizer.Shared;
using Randomizer.SMZ3.Contracts;
using Randomizer.SMZ3.Regions;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld.DeathMountain;
using Randomizer.SMZ3.Regions.Zelda.LightWorld;
using Randomizer.SMZ3.Regions.Zelda.LightWorld.DeathMountain;
using Randomizer.SMZ3.Tracking.Services;

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

            var rewards = new List<Reward>();
            var dungeons = new (Region Region, ItemType Map)[] {
                (World.EasternPalace, ItemType.MapEP),
                (World.DesertPalace, ItemType.MapDP),
                (World.TowerOfHera, ItemType.MapTH)
            };

            foreach (var (region, map) in dungeons)
            {
                if (World.Config.Keysanity && !Items.IsTracked(map))
                    continue;

                var reward = ((IHasReward)region).Reward;
                var dungeonInfo = _tracker.WorldInfo.Dungeon(region);
                if (dungeonInfo.Reward == RewardItem.Unknown)
                {
                    rewards.Add(reward);
                    _tracker.SetDungeonReward(dungeonInfo, ConvertReward(reward));
                }
            }

            if (rewards.Count(x => x == Reward.CrystalRed || x == Reward.CrystalBlue) == 3)
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

            var stateMedallionMessage = true;
            var rewards = new List<Reward>();
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
                if (World.Config.Keysanity && !Items.IsTracked(map))
                    continue;

                var reward = ((IHasReward)region).Reward;
                var dungeonInfo = _tracker.WorldInfo.Dungeon(region);
                if (dungeonInfo.Reward == RewardItem.Unknown)
                {
                    rewards.Add(reward);
                    _tracker.SetDungeonReward(dungeonInfo, ConvertReward(reward));
                }
            }

            if (stateMedallionMessage)
            {
                _tracker.SayOnce(x => x.AutoTracker.DarkWorldNoMedallions);
            }
            else if (rewards.Count == 0)
            {
                _tracker.Say(x => x.AutoTracker.LookedAtNothing);
            }

        }

        /// <summary>
        /// Converts Rewards to RewardItems
        /// TODO: Try to figure out how to determine between blue and red pendants
        /// </summary>
        /// <param name="reward"></param>
        /// <returns></returns>
        private static RewardItem ConvertReward(Reward reward) => reward switch
        {
            Reward.CrystalRed => RewardItem.RedCrystal,
            Reward.CrystalBlue => RewardItem.Crystal,
            Reward.PendantGreen => RewardItem.GreenPendant,
            Reward.PendantNonGreen => RewardItem.NonGreenPendant,
            _ => RewardItem.Unknown,
        };
    }
}
