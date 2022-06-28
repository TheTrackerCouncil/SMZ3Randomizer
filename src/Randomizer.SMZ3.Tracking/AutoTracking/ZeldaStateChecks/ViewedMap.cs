using System.Collections.Generic;
using System.Linq;
using Randomizer.Shared;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld;
using Randomizer.SMZ3.Regions.Zelda.DarkWorld.DeathMountain;
using Randomizer.SMZ3.Regions.Zelda.LightWorld;
using Randomizer.SMZ3.Regions.Zelda.LightWorld.DeathMountain;

namespace Randomizer.SMZ3.Tracking.AutoTracking.ZeldaStateChecks
{
    /// <summary>
    /// Zelda State check for viewing the map
    /// Checks if the game state is viewing the map and the value for viewing the full map is set
    /// </summary>
    public class ViewedMap : IZeldaStateCheck
    {
        private Tracker? _tracker;

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

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapEP))
            {
                var ep = _tracker.World.EasternPalace;
                var epInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(ep));
                if (epInfo.Reward == RewardItem.Unknown)
                {
                    rewards.Add(ep.Reward);
                    _tracker.SetDungeonReward(epInfo, ConvertReward(ep.Reward));
                }
            }

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapDP))
            {
                var dp = _tracker.World.DesertPalace;
                var dpInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(dp));
                if (dpInfo.Reward == RewardItem.Unknown)
                {
                    rewards.Add(dp.Reward);
                    _tracker.SetDungeonReward(dpInfo, ConvertReward(dp.Reward));
                }
            }

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapTH))
            {
                var toh = _tracker.World.TowerOfHera;
                var tohInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(toh));
                if (tohInfo.Reward == RewardItem.Unknown)
                {
                    rewards.Add(toh.Reward);
                    _tracker.SetDungeonReward(tohInfo, ConvertReward(toh.Reward));
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
            var addedReward = false;

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapPD))
            {
                var pod = _tracker.World.PalaceOfDarkness;
                var podInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(pod));
                if (podInfo.Reward == RewardItem.Unknown)
                {
                    _tracker.SetDungeonReward(podInfo, ConvertReward(pod.Reward));
                    addedReward = true;
                }
            }

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapSP))
            {
                var sp = _tracker.World.SwampPalace;
                var spInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(sp));
                if (spInfo.Reward == RewardItem.Unknown)
                {
                    _tracker.SetDungeonReward(spInfo, ConvertReward(sp.Reward));
                    addedReward = true;
                }
            }

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapSW))
            {
                var sw = _tracker.World.SkullWoods;
                var swInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(sw));
                if (swInfo.Reward == RewardItem.Unknown)
                {
                    _tracker.SetDungeonReward(swInfo, ConvertReward(sw.Reward));
                    addedReward = true;
                }
            }

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapTT))
            {
                var tt = _tracker.World.ThievesTown;
                var ttInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(tt));
                if (ttInfo.Reward == RewardItem.Unknown)
                {
                    _tracker.SetDungeonReward(ttInfo, ConvertReward(tt.Reward));
                    addedReward = true;
                }
            }

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapIP))
            {
                var ip = _tracker.World.IcePalace;
                var ipInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(ip));
                if (ipInfo.Reward == RewardItem.Unknown)
                {
                    _tracker.SetDungeonReward(ipInfo, ConvertReward(ip.Reward));
                    addedReward = true;
                }
            }

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapMM))
            {
                var mm = _tracker.World.MiseryMire;
                var mmInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(mm));
                if (mmInfo.Reward == RewardItem.Unknown)
                {
                    _tracker.SetDungeonReward(mmInfo, ConvertReward(mm.Reward));
                    addedReward = true;
                    if (mm.Reward == Reward.CrystalRed || mm.Reward == Reward.CrystalBlue) stateMedallionMessage = false;
                }
            }

            if (!_tracker.World.Config.Keysanity || _tracker.Items.Any(x => x.TrackingState > 0 && x.InternalItemType == ItemType.MapTR))
            {
                var tr = _tracker.World.TurtleRock;
                var trInfo = _tracker.WorldInfo.Dungeons.First(x => x.Is(tr));
                if (trInfo.Reward == RewardItem.Unknown)
                {
                    _tracker.SetDungeonReward(trInfo, ConvertReward(tr.Reward));
                    addedReward = true;
                    if (tr.Reward == Reward.CrystalRed || tr.Reward == Reward.CrystalBlue) stateMedallionMessage = false;
                }
            }

            if (stateMedallionMessage)
            {
                _tracker.SayOnce(x => x.AutoTracker.DarkWorldNoMedallions);
            }
            else if(!addedReward)
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
        private RewardItem ConvertReward(Reward reward)
        {
            switch (reward)
            {
                case Reward.CrystalRed:
                    return RewardItem.RedCrystal;
                case Reward.CrystalBlue:
                    return RewardItem.Crystal;
                case Reward.PendantGreen:
                    return RewardItem.GreenPendant;
                case Reward.PendantNonGreen:
                    return RewardItem.NonGreenPendant;
                default:
                    return RewardItem.Unknown;
            }
        }
    }
}
