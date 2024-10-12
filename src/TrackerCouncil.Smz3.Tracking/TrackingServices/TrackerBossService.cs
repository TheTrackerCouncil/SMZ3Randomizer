using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerBossService(IItemService itemService) : TrackerService, ITrackerBossService
{
    public event EventHandler<BossTrackedEventArgs>? BossUpdated;

    public void MarkRegionBossAsDefeated(IHasBoss region, float? confidence = null, bool autoTracked = false, bool admittedGuilt = false)
    {
        if (region.BossDefeated)
        {
            if (!autoTracked)
                Tracker.Say(response: Responses.DungeonBossAlreadyCleared, args: [region.Metadata.Name, region.BossMetadata.Name]);

            return;
        }

        List<Action> undoActions = [];

        var addedEvent = History.AddEvent(
            HistoryEventType.BeatBoss,
            true,
            region.BossMetadata.Name.ToString() ?? $"boss of {region.Metadata.Name}"
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
                    undoActions.Add(PopUndo().Action);
                }
            }

            Tracker.Say(response: Responses.DungeonBossCleared, args: [region.Metadata.Name, region.BossMetadata.Name]);
        }
        else
        {
            if (!admittedGuilt && region.BossMetadata.WhenTracked != null)
                Tracker.Say(response: region.BossMetadata.WhenTracked, args: [region.BossMetadata.Name]);
            else
                Tracker.Say(response: region.BossMetadata.WhenDefeated ?? Responses.BossDefeated, args: [region.BossMetadata.Name]);
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
            itemService.ResetProgression();
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

    public void MarkBossAsDefeated(Boss boss, bool admittedGuilt = true, float? confidence = null, bool autoTracked = false)
    {
        if (boss.Defeated)
        {
            if (!autoTracked)
                Tracker.Say(x => x.BossAlreadyDefeated, args: [boss.Name]);
            return;
        }

        if (boss.Region != null)
        {
            MarkRegionBossAsDefeated(boss.Region, confidence, autoTracked, admittedGuilt);
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

    public void MarkBossAsNotDefeated(Boss boss, float? confidence = null)
    {
        if (boss.Defeated != true)
        {
            Tracker.Say(x => x.BossNotYetDefeated, args: [boss.Name]);
            return;
        }

        if (boss.Region != null)
        {
            MarkRegionBossAsNotDefeated(boss.Region, confidence);
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
    public void MarkRegionBossAsNotDefeated(IHasBoss region, float? confidence = null)
    {
        if (!region.BossDefeated)
        {
            Tracker.Say(response: Responses.DungeonBossNotYetCleared, args: [region.Metadata.Name, region.BossMetadata.Name]);
            return;
        }

        region.BossDefeated = false;
        BossUpdated?.Invoke(this, new BossTrackedEventArgs(region.Boss, confidence, false));
        Tracker.Say(response: Responses.DungeonBossUncleared, args: [region.Metadata.Name, region.BossMetadata.Name]);

        // Try to untrack the associated boss reward item
        List<Action> undoActions = [];

        if (region.BossLocationId != null)
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
        actualProgression ??= itemService.GetProgression(false);
        withKeysProgression ??= itemService.GetProgression(true);
        foreach (var region in Tracker.World.BossRegions)
        {
            UpdateAccessibility(region, actualProgression, withKeysProgression);
        }
    }

    public void UpdateAccessibility(Boss boss, Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        if (boss.Region == null) return;
        actualProgression ??= itemService.GetProgression(false);
        withKeysProgression ??= itemService.GetProgression(true);
        UpdateAccessibility(boss.Region, actualProgression, withKeysProgression);
    }

    public void UpdateAccessibility(IHasBoss region, Progression? actualProgression = null, Progression? withKeysProgression = null)
    {
        if (region.BossDefeated)
        {
            region.BossAccessibility = Accessibility.Cleared;
            return;
        }

        actualProgression ??= itemService.GetProgression(false);
        withKeysProgression ??= itemService.GetProgression(true);

        region.Boss.UpdateAccessibility(actualProgression, withKeysProgression);
    }
}
