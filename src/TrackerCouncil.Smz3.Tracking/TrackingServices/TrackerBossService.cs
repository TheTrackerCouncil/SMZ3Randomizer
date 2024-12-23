using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerBossService(IPlayerProgressionService playerProgressionService) : TrackerService, ITrackerBossService
{
    public event EventHandler<BossTrackedEventArgs>? BossUpdated;

    public void MarkBossAsDefeated(IHasBoss region, float? confidence = null, bool autoTracked = false, bool admittedGuilt = false, bool force = false)
    {
        if (region.BossDefeated && !autoTracked)
        {
            Tracker.Say(response: Responses.DungeonBossAlreadyCleared, args: [region.RandomRegionName, region.RandomBossName]);
            return;
        }

        if (!autoTracked && Tracker.AutoTracker?.IsConnected == true && !force && region.BossType != BossType.None)
        {
            Tracker.Say(response: Responses.AutoTrackingEnabledSass, args: [$"Hey tracker, would you please track {region.RandomBossName}"]);
            return;
        }

        List<Action> undoActions = [];

        var addedEvent = History.AddEvent(
            HistoryEventType.BeatBoss,
            true,
            region.RandomBossName
        );

        region.Boss.Defeated = true;
        region.Boss.AutoTracked = autoTracked;
        BossUpdated?.Invoke(this, new BossTrackedEventArgs(region.Boss, confidence, autoTracked));

        // If all treasures have been retrieved and the boss is defeated, clear all locations in the dungeon
        if (region is IHasTreasure treasureRegion)
        {
            if (treasureRegion.RemainingTreasure == 0)
            {
                foreach (var location in ((Region)region).Locations.Where(x => !x.Cleared))
                {
                    Tracker.LocationTracker.Clear(location, confidence, autoTracked, false);

                    if (!autoTracked)
                    {
                        undoActions.Add(PopUndo().Action);
                    }
                }
            }

            Tracker.Say(response: Responses.DungeonBossCleared, args: [region.RandomRegionName, region.RandomBossName]);
        }
        else
        {
            if (!admittedGuilt && region.BossMetadata.WhenTracked != null)
                Tracker.Say(response: region.BossMetadata.WhenTracked, args: [region.RandomBossName]);
            else
                Tracker.Say(response: region.BossMetadata.WhenDefeated ?? Responses.BossDefeated, args: [region.RandomBossName]);
        }

        // Auto track the region's reward
        if (region is IHasReward rewardRegion)
        {
            Tracker.RewardTracker.GiveAreaReward(rewardRegion, autoTracked, true);
        }

        IsDirty = true;
        RestartIdleTimers();
        UpdateAllAccessibility(false);

        AddUndo(autoTracked, () =>
        {
            playerProgressionService.ResetProgression();
            region.BossDefeated = false;
            BossUpdated?.Invoke(this, new BossTrackedEventArgs(region.Boss, null, false));
            foreach (var undo in undoActions)
            {
                undo();
            }
            addedEvent.IsUndone = true;
            UpdateAllAccessibility(true);
        });
    }

    public void MarkBossAsDefeated(Boss boss, bool admittedGuilt = true, float? confidence = null, bool autoTracked = false, bool force = false)
    {
        if (boss.Defeated)
        {
            if (!autoTracked)
                Tracker.Say(x => x.BossAlreadyDefeated, args: [boss.Name]);
            return;
        }

        if (!autoTracked && Tracker.AutoTracker?.IsConnected == true && !force && boss.Type != BossType.None)
        {
            Tracker.Say(response: Responses.AutoTrackingEnabledSass, args: [$"Hey tracker, would you please track {boss.RandomName}"]);
            return;
        }

        if (boss.Region != null)
        {
            MarkBossAsDefeated(boss.Region, confidence, autoTracked, admittedGuilt);
            return;
        }

        boss.Defeated = true;
        boss.AutoTracked = autoTracked;
        BossUpdated?.Invoke(this, new BossTrackedEventArgs(boss, confidence, autoTracked));

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
        UpdateAllAccessibility(false);

        RestartIdleTimers();

        AddUndo(autoTracked, () =>
        {
            boss.Defeated = false;
            BossUpdated?.Invoke(this, new BossTrackedEventArgs(boss, null, false));
            addedEvent.IsUndone = true;
            UpdateAllAccessibility(true);
        });
    }

    public void MarkBossAsNotDefeated(Boss boss, float? confidence = null, bool force = false)
    {
        if (boss.Defeated != true)
        {
            Tracker.Say(x => x.BossNotYetDefeated, args: [boss.Name]);
            return;
        }

        if (Tracker.AutoTracker?.IsConnected == true && !force && boss.Type != BossType.None)
        {
            Tracker.Say(response: Responses.AutoTrackingEnabledSass, args: [$"Hey tracker, would you please untrack {boss.RandomName}"]);
            return;
        }

        if (boss.Region != null)
        {
            MarkBossAsNotDefeated(boss.Region, confidence);
            return;
        }

        boss.Defeated = false;
        BossUpdated?.Invoke(this, new BossTrackedEventArgs(boss, confidence, false));
        Tracker.Say(response: Responses.BossUndefeated, args: [boss.Name]);

        IsDirty = true;
        UpdateAllAccessibility(true);

        AddUndo(() =>
        {
            boss.Defeated = true;
            UpdateAllAccessibility(false);
            BossUpdated?.Invoke(this, new BossTrackedEventArgs(boss, null, false));
        });
    }

    /// <summary>
    /// Un-marks a dungeon as cleared and, if possible, untracks the boss
    /// reward.
    /// </summary>
    /// <param name="region">The dungeon that should be un-cleared.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="force">If the user requested to force the boss to be marked as not defeated</param>
    public void MarkBossAsNotDefeated(IHasBoss region, float? confidence = null, bool force = false)
    {
        if (!region.BossDefeated)
        {
            Tracker.Say(response: Responses.DungeonBossNotYetCleared, args: [region.RandomRegionName, region.RandomBossName]);
            return;
        }

        if (Tracker.AutoTracker?.IsConnected == true && !force && region.BossType != BossType.None)
        {
            Tracker.Say(response: Responses.AutoTrackingEnabledSass, args: [$"Hey tracker, would you please untrack {region.RandomBossName}"]);
            return;
        }

        region.BossDefeated = false;
        BossUpdated?.Invoke(this, new BossTrackedEventArgs(region.Boss, confidence, false));
        Tracker.Say(response: Responses.DungeonBossUncleared, args: [region.RandomRegionName, region.RandomBossName]);

        // Try to untrack the associated boss reward item
        List<Action> undoActions = [];

        if (region is { BossLocationId: not null, UnifiedBossAndItemLocation: true })
        {
            var bossLocation = World.LocationMap[region.BossLocationId.Value];
            if (bossLocation.Cleared)
            {
                Tracker.LocationTracker.Unclear(bossLocation);
                undoActions.Add(PopUndo().Action);
            }

            if (bossLocation.Item.Type != ItemType.Nothing && bossLocation.Item.TrackingState > 0)
            {
                Tracker.ItemTracker.UntrackItem(bossLocation.Item);
                undoActions.Add(PopUndo().Action);
            }
        }

        if (region is IHasReward rewardRegion)
        {
            Tracker.RewardTracker.RemoveAreaReward(rewardRegion, true);
        }

        IsDirty = true;
        UpdateAllAccessibility(true);

        AddUndo(() =>
        {
            region.BossDefeated = true;
            region.BossAccessibility = Accessibility.Cleared;
            BossUpdated?.Invoke(this, new BossTrackedEventArgs(region.Boss, null, false));

            foreach (var undo in undoActions)
            {
                undo.Invoke();
            }
            UpdateAllAccessibility(false);
        });
    }

    public void UpdateAccessibility(Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        actualProgression ??= playerProgressionService.GetProgression(false);
        withKeysProgression ??= playerProgressionService.GetProgression(true);
        foreach (var region in Tracker.World.BossRegions)
        {
            UpdateAccessibility(region, actualProgression, withKeysProgression);
        }
    }

    public void UpdateAccessibility(Boss boss, Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        if (boss.Region == null) return;
        actualProgression ??= playerProgressionService.GetProgression(false);
        withKeysProgression ??= playerProgressionService.GetProgression(true);
        UpdateAccessibility(boss.Region, actualProgression, withKeysProgression);
    }

    public void UpdateAccessibility(IHasBoss region, Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        if (region.BossDefeated)
        {
            region.BossAccessibility = Accessibility.Cleared;
            return;
        }

        actualProgression ??= playerProgressionService.GetProgression(false);
        withKeysProgression ??= playerProgressionService.GetProgression(true);

        region.Boss.UpdateAccessibility(actualProgression, withKeysProgression);
    }
}
