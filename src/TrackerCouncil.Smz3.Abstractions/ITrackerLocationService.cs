using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerLocationService
{
    public event EventHandler<LocationClearedEventArgs> LocationCleared;

    public event EventHandler<LocationClearedEventArgs> LocationMarked;

    /// <summary>
    /// Clears an item from the specified location.
    /// </summary>
    /// <param name="location">The location to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    void Clear(Location location, float? confidence = null, bool autoTracked = false, bool stateResponse = true, bool allowLocationComments = false, bool updateTreasureCount = true);

    /// <summary>
    /// Unclears an item from the specified location.
    /// </summary>
    /// <param name="location">The location to clear.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    void Unclear(Location location, bool updateTreasureCount = true);

    /// <summary>
    /// Clears an item from the specified locations.
    /// </summary>
    /// <param name="locations">The locations to clear.</param>
    /// <param name="confidence">The speech recognition confidence</param>
    void Clear(List<Location> locations, float? confidence = null);

    /// <summary>
    /// Marks an item at the specified location.
    /// </summary>
    /// <param name="location">The location to mark.</param>
    /// <param name="item">
    /// The item that is found at <paramref name="location"/>.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked location was auto tracked</param>
    public void MarkLocation(Location location, Item item, float? confidence = null, bool autoTracked = false);

    /// <summary>
    /// Marks an item at the specified location.
    /// </summary>
    /// <param name="location">The location to mark.</param>
    /// <param name="item">
    /// The item that is found at <paramref name="location"/>.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="autoTracked">If the marked location was auto tracked</param>
    /// <param name="metadata">The metadata of the item</param>
    public void MarkLocation(Location location, ItemType item, float? confidence = null, bool autoTracked = false,
        ItemData? metadata = null);

    /// <summary>
    /// Clears every item in the specified area, optionally tracking the
    /// cleared items.
    /// </summary>
    /// <param name="area">The area whose items to clear.</param>
    /// <param name="trackItems">
    /// <c>true</c> to track any items found; <c>false</c> to only clear the
    /// affected locations.
    /// </param>
    /// <param name="includeUnavailable">
    /// <c>true</c> to include every item in <paramref name="area"/>, even
    /// those that are not in logic. <c>false</c> to only include chests
    /// available with current items.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="assumeKeys">
    /// Set to true to ignore keys when clearing the location.
    /// </param>
    public void ClearArea(IHasLocations area, bool trackItems, bool includeUnavailable = false,
        float? confidence = null, bool assumeKeys = false);

    public void UpdateAccessibility(bool unclearedOnly = true, Progression? actualProgression = null, Progression? withKeysProgression = null);

    public void UpdateAccessibility(IEnumerable<Location> locations, Progression? actualProgression = null, Progression? withKeysProgression = null);

    public void UpdateAccessibility(Location location, Progression? actualProgression = null, Progression? withKeysProgression = null);
}
