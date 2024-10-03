using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

public class TrackerBossService(IItemService itemService) : TrackerService, ITrackerBossService
{
    /// <summary>
    /// Marks a dungeon as cleared and, if possible, tracks the boss reward.
    /// </summary>
    /// <param name="region">The dungeon that was cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was cleared by the auto tracker</param>
    public void MarkRegionBossAsDefeated(IHasBoss region, float? confidence = null, bool autoTracked = false)
    {
        if (region.BossDefeated)
        {
            if (!autoTracked)
                Tracker.Say(response: Responses.DungeonBossAlreadyCleared, args: [region.Metadata.Name, region.BossMetadata.Name]);

            return;
        }

        itemService.ResetProgression();
        List<Action> undoActions = [];

        var addedEvent = History.AddEvent(
            HistoryEventType.BeatBoss,
            true,
            region.BossMetadata.Name.ToString() ?? $"boss of {region.Metadata.Name}"
        );

        // If all treasures have been retrieved and the boss is defeated, clear all locations in the dungeon
        if (region is IHasTreasure { RemainingTreasure: 0 })
        {
            foreach (var location in ((Region)region).Locations.Where(x => !x.State.Cleared))
            {
                Tracker.LocationTracker.Clear(location, confidence, autoTracked, false);
                undoActions.Add(PopUndo().Action);
            }
        }

        // Auto track the dungeon reward if not already marked
        if (region is IHasReward rewardRegion && autoTracked && rewardRegion.MarkedReward != rewardRegion.RewardType)
        {
            rewardRegion.MarkedReward = rewardRegion.RewardType;
        }

        region.BossDefeated = true;
        Tracker.Say(response: Responses.DungeonBossCleared, args: [region.Metadata.Name, region.BossMetadata.Name]);
        IsDirty = true;
        RestartIdleTimers();

        if (!autoTracked)
        {
            AddUndo(() =>
            {
                itemService.ResetProgression();
                region.BossDefeated = false;
                foreach (var action in undoActions)
                {
                    action();
                }
                addedEvent.IsUndone = true;
            });
        }
    }

    /// <summary>
    /// Marks a boss as defeated.
    /// </summary>
    /// <param name="boss">The boss that was defeated.</param>
    /// <param name="admittedGuilt">
    /// <see langword="true"/> if the command implies the boss was killed;
    /// <see langword="false"/> if the boss was simply "tracked".
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    public void MarkBossAsDefeated(Boss boss, bool admittedGuilt = true, float? confidence = null, bool autoTracked = false)
    {
        if (boss.Defeated)
        {
            if (!autoTracked)
                Tracker.Say(x => x.BossAlreadyDefeated, args: [boss.Name]);
            return;
        }

        boss.Defeated = true;
        boss.AutoTracked = true;

        if (!admittedGuilt && boss.Metadata.WhenTracked != null)
            Tracker.Say(response: boss.Metadata.WhenTracked, args: [boss.Name]);
        else
            Tracker.Say(response: boss.Metadata.WhenDefeated ?? Responses.BossDefeated, args: [boss.Name]);

        var addedEvent = History.AddEvent(
            HistoryEventType.BeatBoss,
            true,
            boss.Name
        );

        IsDirty = true;
        itemService.ResetProgression();

        RestartIdleTimers();

        if (!autoTracked)
        {
            AddUndo(() =>
            {
                boss.Defeated = false;
                addedEvent.IsUndone = true;
            });
        }
    }

    /// <summary>
    /// Un-marks a boss as defeated.
    /// </summary>
    /// <param name="boss">The boss that should be 'revived'.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void MarkBossAsNotDefeated(Boss boss, float? confidence = null)
    {
        if (boss.Defeated != true)
        {
            Tracker.Say(x => x.BossNotYetDefeated, args: [boss.Name]);
            return;
        }

        boss.Defeated = false;
        Tracker.Say(response: Responses.BossUndefeated, args: [boss.Name]);

        IsDirty = true;
        itemService.ResetProgression();

        AddUndo(() =>
        {
            boss.Defeated = true;
        });
    }

    /// <summary>
    /// Un-marks a dungeon as cleared and, if possible, untracks the boss
    /// reward.
    /// </summary>
    /// <param name="region">The dungeon that should be un-cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void MarkRegionBossAsNotDefeated(IHasBoss region, float? confidence = null)
    {
        if (!region.BossDefeated)
        {
            Tracker.Say(response: Responses.DungeonBossNotYetCleared, args: [region.Metadata.Name, region.BossMetadata.Name]);
            return;
        }

        itemService.ResetProgression();
        region.BossDefeated = false;
        Tracker.Say(response: Responses.DungeonBossUncleared, args: [region.Metadata.Name, region.BossMetadata.Name]);

        // Try to untrack the associated boss reward item
        Action? undoUnclear = null;
        Action? undoUntrackTreasure = null;
        Action? undoUntrack = null;
        if (region.BossLocationId != null)
        {
            var rewardLocation = World.LocationMap[region.BossLocationId.Value];
            if (rewardLocation.Item.Type != ItemType.Nothing)
            {
                var item = rewardLocation.Item;
                if (item.Type != ItemType.Nothing && item.State.TrackingState > 0)
                {
                    Tracker.ItemTracker.UntrackItem(item);
                    undoUntrack = PopUndo().Action;
                }

                if (!rewardLocation.Item.IsDungeonItem && region is IHasTreasure treasureRegion)
                {
                    treasureRegion.RemainingTreasure++;
                    undoUntrackTreasure = () => treasureRegion.RemainingTreasure--;
                }
            }

            if (rewardLocation.State.Cleared)
            {
                rewardLocation.State.Cleared = false;
                undoUnclear = () => rewardLocation.State.Cleared = true;
            }
        }

        IsDirty = true;

        AddUndo(() =>
        {
            region.BossDefeated = false;
            undoUntrack?.Invoke();
            undoUntrackTreasure?.Invoke();
            undoUnclear?.Invoke();
            itemService.ResetProgression();
        });
    }
}
