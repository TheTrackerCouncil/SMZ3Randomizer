﻿using System;
using System.Linq;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.WorldData.Regions.Zelda;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions;

/// <summary>
/// Defines a region that has a boss which is required to beat the game
/// </summary>
public interface IHasBoss
{
    string Name { get; }

    RegionInfo Metadata { get; set; }

    World World { get; }

    Boss Boss { get; protected set; }

    BossInfo BossMetadata => Boss.Metadata;

    BossType BossType => Boss.Type;

    BossType DefaultBossType { get; }

    TrackerBossState BossState => Boss.State;

    LocationId? BossLocationId { get; }

    /// <summary>
    /// If the boss and location are tied where one can't be received without the other
    /// (e.g. Zelda dungeons)
    /// </summary>
    public bool UnifiedBossAndItemLocation { get; }

    Region Region => (Region)this;

    public bool BossDefeated
    {
        get => Boss.Defeated;
        set => Boss.Defeated = value;
    }

    public Accessibility BossAccessibility
    {
        get => Boss.Accessibility;
        set => Boss.Accessibility = value;
    }

    public Accessibility GetKeysanityAdjustedBossAccessibility()
    {
        return Region.GetKeysanityAdjustedAccessibility(BossAccessibility);
    }

    public void SetBossType(BossType bossType)
    {
        Boss = World.Bosses.First(x => x.Type == bossType && x.Region == null);
        Boss.Region = this;
    }

    public void ApplyState(TrackerState? state)
    {
        if (state == null)
        {
            SetBossType(DefaultBossType);
            Boss.State = new TrackerBossState
            {
                WorldId = World.Id,
                RegionName = GetType().Name,
                Type = DefaultBossType,
                BossName = Boss.Name
            };
        }
        else
        {
            var bossState = state.BossStates.First(x =>
                x.WorldId == World.Id && x.RegionName == GetType().Name);
            SetBossType(bossState.Type);
            Boss.State = bossState;
        }
    }

    /// <summary>
    /// Determines whether the boss for the region can be defeated.
    /// </summary>
    /// <param name="items">The items currently available.</param>
    /// <param name="isTracking">If this is checking for tracking rather than seed generation</param>
    /// <returns>
    /// <see langword="true"/> if the boss can be beaten; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool CanBeatBoss(Progression items, bool isTracking);

    /// <summary>
    /// Returns a randomized name from the metadata for the region
    /// </summary>
    public string RandomRegionName => Region.RandomAreaName;

    /// <summary>
    /// Returns a randomized name from the metadata for the boss
    /// </summary>
    public string RandomBossName => Boss.RandomName;
}
