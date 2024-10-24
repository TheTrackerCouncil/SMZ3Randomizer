using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Service for looking up various information about the world
/// (or worlds for multiworld games)
/// </summary>
public interface IWorldQueryService
{
    /// <summary>
    /// Retrieves the world for the current player
    /// </summary>
    public World World { get; }

    /// <summary>
    /// Retrieves all worlds
    /// </summary>
    public List<World> Worlds { get; }

    /// <summary>
    /// Retrieves a particular world matching a player id
    /// </summary>
    /// <param name="id">The player id of the world to get</param>
    /// <returns></returns>
    public World GetWorld(int id);

    /// <summary>
    /// Retrives all locations for current player's world
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Location> AllLocations();

    /// <summary>
    /// Retrieves all accessible uncleared locations for the current player's world
    /// </summary>
    /// <param name="progression">The progression object with all of the player's inventory and rewards</param>
    /// <returns></returns>
    public IEnumerable<Location> AccessibleLocations(Progression progression);

    /// <summary>
    /// Retrieves all accessible uncleared locations for the current player's world
    /// </summary>
    /// <param name="assumeKeys">If keys and keycards should be assumed for the player</param>
    /// <returns></returns>
    public IEnumerable<Location> AccessibleLocations(bool assumeKeys);

    /// <summary>
    /// Retrieves all uncleared locations for the current player's world, regardless of if
    /// they are accessible or out of logic
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Location> UnclearedLocations();

    /// <summary>
    /// Retrieves all locations for the current player's world that has been marked as
    /// having an item at it
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Location> MarkedLocations();

    /// <summary>
    /// Retrieves a collection of locations for the current player's world that match the given filter criteria
    /// </summary>
    /// <param name="unclearedOnly">Set to false to show locations that have already been cleared by the player</param>
    /// <param name="outOfLogic">Set to true to show locations that are not currently accessible to the player</param>
    /// <param name="assumeKeys">Set to true if keys should be ignored when it comes to determining logic</param>
    /// <param name="sortByTopRegion">Set to true to sort locations by the most recent region and region with the most locations</param>
    /// <param name="regionFilter">Filter for the type of region (SM or LttP)</param>
    /// <param name="itemFilter">Set to return locations that have the matching item</param>
    /// <param name="inRegion">Set to return locations that match a specific region</param>
    /// <param name="keysanityByRegion">Set to true if keys should be assumed or not based on if keysanity is enabled for that region</param>
    /// <param name="checkAllWorlds">If all locations in all of the worlds in the multiworld should be checked</param>
    /// <returns></returns>
    public IEnumerable<Location> Locations(bool unclearedOnly = true, bool outOfLogic = false, bool assumeKeys = false, bool sortByTopRegion = false, RegionFilter regionFilter = RegionFilter.None, ItemType itemFilter = ItemType.Nothing, Region? inRegion = null, bool keysanityByRegion = false, bool checkAllWorlds = false);

    /// <summary>
    /// Returns the specific location matching the given id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Location Location(LocationId id);

    /// <summary>
    /// Checks if the location is accessible with the given progression
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <param name="progression">The player's current inventory and rewards</param>
    /// <returns>True if accessible, false otherwise</returns>
    public bool IsAvailable(Location location, Progression progression);

    /// <summary>
    /// Checks if the location is accessible
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <param name="assumeKeys">If keys should be ignored when it comes to determining logic</param>
    /// <returns>True if accessible, false otherwise</returns>
    public bool IsAvailable(Location location, bool assumeKeys);

    /// <summary>
    /// Checks if the location is accessible based on that region's Keysanity logic
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <returns>True if accessible, false otherwise</returns>
    public bool IsAvailable(Location location);

    /// <summary>
    /// Returns all regions in the current player's world
    /// </summary>
    public IEnumerable<Region> Regions { get; }

    /// <summary>
    /// Returns the region matching the given name
    /// </summary>
    /// <param name="name">The region name to lookup</param>
    /// <returns></returns>
    public Region? Region(string name);

    /// <summary>
    /// Returns the list of hint tiles that have been viewed but not cleared
    /// </summary>
    public IEnumerable<PlayerHintTile> ViewedHintTiles { get; }

    /// <summary>
    /// Enumerates all items that can be tracked for all players.
    /// </summary>
    /// <returns>A collection of items.</returns>
    IEnumerable<Item> AllItems();

    /// <summary>
    /// Enumerates all items that can be tracked for the local player.
    /// </summary>
    /// <returns>A collection of items.</returns>
    IEnumerable<Item> LocalPlayersItems();

    /// <summary>
    /// Finds the item with the specified name for the local player.
    /// </summary>
    /// <param name="name">
    /// The name of the item or item stage to find.
    /// </param>
    /// <returns>
    /// An <see cref="Item"/> representing the item with the specified
    /// name, or <see langword="null"/> if there is no item that has the
    /// specified name.
    /// </returns>
    Item? FirstOrDefault(string name);

    /// <summary>
    /// Finds an item with the specified item type for the local player.
    /// </summary>
    /// <param name="itemType">The type of item to find.</param>
    /// <returns>
    /// An <see cref="Item"/> representing the item. If there are
    /// multiple configured items with the same type, this method returns
    /// one at random. If there no configured items with the specified type,
    /// this method returns <see langword="null"/>.
    /// </returns>
    Item? FirstOrDefault(ItemType itemType);

    /// <summary>
    /// Finds an reward with the specified item type.
    /// </summary>
    /// <param name="rewardType">The type of reward to find.</param>
    /// <returns>
    /// An <see cref="Reward"/> representing the reward. If there are
    /// multiple configured rewards with the same type, this method returns
    /// one at random. If there no configured rewards with the specified type,
    /// this method returns <see langword="null"/>.
    /// </returns>
    Reward? FirstOrDefault(RewardType rewardType);

}
