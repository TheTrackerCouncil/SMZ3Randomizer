using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerItemService
{
    /// <summary>
    /// Tracks the specifies item.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="tryClear">
    /// <see langword="true"/> to attempt to clear a location for the
    /// tracked item; <see langword="false"/> if that is done by the caller.
    /// </param>
    /// <param name="autoTracked">If this was tracked by the auto tracker</param>
    /// <param name="location">The location an item was tracked from</param>
    /// <param name="giftedItem">If the item was gifted to the player by tracker or another player</param>
    /// <param name="silent">If tracker should not say anything</param>
    /// <returns>
    /// <see langword="true"/> if the item was actually tracked; <see
    /// langword="false"/> if the item could not be tracked, e.g. when
    /// tracking Bow twice.
    /// </returns>
    bool TrackItem(Item item, string? trackedAs = null, float? confidence = null, bool tryClear = true,
        bool autoTracked = false, Location? location = null, bool giftedItem = false, bool silent = false);

    /// <summary>
    /// Tracks the specifies item and clears it from the specified dungeon.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="hasTreasure">The dungeon the item was tracked in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    void TrackItemFrom(Item item, IHasTreasure hasTreasure, string? trackedAs = null, float? confidence = null);

    /// <summary>
    /// Tracks the specified item and clears it from the specified room.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="area">The area the item was found in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    void TrackItemFrom(Item item, IHasLocations area, string? trackedAs = null, float? confidence = null);

    /// <summary>
    /// Sets the item count for the specified item.
    /// </summary>
    /// <param name="item">The item to track.</param>
    /// <param name="count">
    /// The amount of the item that is in the player's inventory now.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    void TrackItemAmount(Item item, int count, float confidence);

    /// <summary>
    /// Tracks multiple items at the same time
    /// </summary>
    /// <param name="items">The items to track</param>
    /// <param name="autoTracked">If the items were tracked via auto tracker</param>
    /// <param name="giftedItem">If the items were gifted to the player</param>
    void TrackItems(List<Item> items, bool autoTracked, bool giftedItem);

    /// <summary>
    /// Removes an item from the tracker.
    /// </summary>
    /// <param name="item">The item to untrack.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    void UntrackItem(Item item, float? confidence = null);
}
