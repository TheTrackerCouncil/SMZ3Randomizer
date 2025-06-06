﻿using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.Data.Tracking;

/// <summary>
/// Provides data for events that occur when tracking a dungeon.
/// </summary>
public class DungeonTrackedEventArgs : TrackerEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="DungeonTrackedEventArgs"/> class.
    /// </summary>
    /// <param name="dungeon">The dungeon that was tracked.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the location was automatically tracked</param>
    public DungeonTrackedEventArgs(IHasTreasure? dungeon, float? confidence, bool autoTracked)
        : base(confidence, autoTracked)
    {
        Dungeon = dungeon;
    }

    /// <summary>
    /// Gets the boss that was tracked.
    /// </summary>
    public IHasTreasure? Dungeon { get; }
}
