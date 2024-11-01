using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerTreasureService
{
    /// <summary>
    /// Removes one or more items from the available treasure in the specified region.
    /// </summary>
    /// <param name="region">The dungeon.</param>
    /// <param name="amount">The number of treasures to track.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was called by the auto tracker</param>
    /// <param name="stateResponse">If tracker should state the treasure amount</param>
    /// <returns>
    /// <c>true</c> if treasure was tracked; <c>false</c> if there is no
    /// treasure left to track.
    /// </returns>
    /// <remarks>
    /// This method adds to the undo history if the return value is
    /// <c>true</c>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="amount"/> is less than 1.
    /// </exception>
    bool TrackDungeonTreasure(IHasTreasure region, float? confidence = null, int amount = 1,
        bool autoTracked = false, bool stateResponse = true);

    public bool UntrackDungeonTreasure(IHasTreasure region, int amount = 1);

    Action? TryTrackDungeonTreasure(Location location, float? confidence, bool autoTracked = false,
        bool stateResponse = true);

    public Action? TryUntrackDungeonTreasure(Location location);

    /// <summary>
    /// Marks all locations and treasure within a dungeon as cleared.
    /// </summary>
    /// <param name="treasureRegion">The dungeon to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    public void ClearDungeon(IHasTreasure treasureRegion, float? confidence = null);
}
