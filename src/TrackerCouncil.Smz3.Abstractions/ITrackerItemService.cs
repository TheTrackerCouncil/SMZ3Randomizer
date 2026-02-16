using SnesConnectorLibrary.Responses;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Abstractions;

public interface ITrackerItemService
{
    public event EventHandler<ItemTrackedEventArgs>? ItemTracked;

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
    /// <param name="force">If the item should be forced to be tracked while auto tracking</param>
    /// <returns>
    /// <see langword="true"/> if the item was actually tracked; <see
    /// langword="false"/> if the item could not be tracked, e.g. when
    /// tracking Bow twice.
    /// </returns>
    bool TrackItem(Item item, string? trackedAs = null, float? confidence = null, bool tryClear = true,
        bool autoTracked = false, Location? location = null, bool giftedItem = false, bool silent = false, bool force = false);

    /// <summary>
    /// Tracks the specifies item and clears it from the specified dungeon.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="hasTreasure">The dungeon the item was tracked in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="force">If the item should be forced to be tracked while auto tracking</param>
    void TrackItemFrom(Item item, IHasTreasure hasTreasure, string? trackedAs = null, float? confidence = null, bool force = false);

    /// <summary>
    /// Tracks the specified item and clears it from the specified room.
    /// </summary>
    /// <param name="item">The item data to track.</param>
    /// <param name="trackedAs">
    /// The text that was tracked, when triggered by voice command.
    /// </param>
    /// <param name="area">The area the item was found in.</param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="force">If the item should be forced to be tracked while auto tracking</param>
    void TrackItemFrom(Item item, IHasLocations area, string? trackedAs = null, float? confidence = null, bool force = false);

    /// <summary>
    /// Sets the item count for the specified item.
    /// </summary>
    /// <param name="item">The item to track.</param>
    /// <param name="count">
    /// The amount of the item that is in the player's inventory now.
    /// </param>
    /// <param name="confidence">The speech recognition confidence.</param>
    /// <param name="force">If the item should be forced to be tracked while auto tracking</param>
    /// <param name="silent">If the item should be announced</param>
    void TrackItemAmount(Item item, int count, float confidence, bool autoTracked = false, bool force = false, bool silent = false);

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
    /// <param name="force">If the item should be forced to be untracked while auto tracking</param>
    /// <param name="silent">If the untracking of the item should be announced</param>
    void UntrackItem(Item item, float? confidence = null, bool autoTracked = false, bool force = false, bool silent = false);

    /// <summary>
    /// Updates the item tracking status based on memory from the SNES
    /// </summary>
    /// <param name="itemType">The item type being tracked</param>
    /// <param name="memoryType">How the memory should be read</param>
    /// <param name="currentData">The current memory values</param>
    /// <param name="previousData">The previous memory values</param>
    /// <param name="memoryOffset">The offset from the beginning of the current data</param>
    /// <param name="flagPosition">The flag position of the byte to be checked</param>
    void UpdateItemFromSnesMemory(ItemType itemType, ItemSnesMemoryType memoryType, SnesData currentData, SnesData previousData, int memoryOffset, int flagPosition = 0);
}

