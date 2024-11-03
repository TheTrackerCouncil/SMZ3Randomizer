using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerPrerequisiteService
{
    /// <summary>
    /// Sets the dungeon's medallion requirement to the specified item.
    /// </summary>
    /// <param name="region">The dungeon to mark.</param>
    /// <param name="medallion">The medallion that is required.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked dungeon requirement was autotracked</param>
    public void SetDungeonRequirement(IHasPrerequisite region, ItemType? medallion = null, float? confidence = null,
        bool autoTracked = false);
}
