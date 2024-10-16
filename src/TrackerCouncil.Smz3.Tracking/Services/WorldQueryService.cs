using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Tracking.Services;

/// <summary>
/// Service for finding locations in the world
/// </summary>
public class WorldQueryService : IWorldQueryService
{
    private readonly IWorldAccessor _worldAccessor;
    private readonly IPlayerProgressionService _playerProgressionService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="world"></param>
    /// <param name="playerProgressionService"></param>
    public WorldQueryService(IWorldAccessor world, IPlayerProgressionService playerProgressionService)
    {
        _worldAccessor = world;
        _playerProgressionService = playerProgressionService;
    }

    /// <summary>
    /// Retrieves the world for the current player
    /// </summary>
    public World World => _worldAccessor.World;

    /// <summary>
    /// Retrieves all worlds
    /// </summary>
    public List<World> Worlds => _worldAccessor.Worlds;

    /// <summary>
    /// Retrieves a particular world matching a player id
    /// </summary>
    /// <param name="id">The player id of the world to get</param>
    /// <returns></returns>
    public World GetWorld(int id) => Worlds.Single(x => x.Id == id);

    /// <summary>
    /// Retrives all locations for current player's world
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Location> AllLocations() => World.Locations;

    /// <summary>
    /// Retrives all locations for all worlds
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Location> AllMultiworldLocations() => _worldAccessor.Worlds.SelectMany(x => x.Locations);

    /// <summary>
    /// Retrieves all accessible uncleared locations for the current player's world
    /// </summary>
    /// <param name="progression">The progression object with all of the player's inventory and rewards</param>
    /// <returns></returns>
    public IEnumerable<Location> AccessibleLocations(Progression progression)
        => UnclearedLocations().Where(x => x.IsAvailable(progression, true)).ToImmutableList();

    /// <summary>
    /// Retrieves all accessible uncleared locations for the current player's world
    /// </summary>
    /// <param name="assumeKeys">If keys and keycards should be assumed for the player</param>
    /// <returns></returns>
    public IEnumerable<Location> AccessibleLocations(bool assumeKeys)
        => AccessibleLocations(Progression(assumeKeys)).ToImmutableList();

    /// <summary>
    /// Retrieves all uncleared locations for the current player's world, regardless of if
    /// they are accessible or out of logic
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Location> UnclearedLocations()
        => AllLocations().Where(x => !x.Cleared).ToImmutableList();

    /// <summary>
    /// Retrieves all locations for the current player's world that has been marked as
    /// having an item at it
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Location> MarkedLocations()
        => AllLocations().Where(x => x is { Cleared: false, MarkedItem: not null } && x.MarkedItem != ItemType.Nothing).ToImmutableList();

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
    public IEnumerable<Location> Locations(bool unclearedOnly = true, bool outOfLogic = false, bool assumeKeys = false, bool sortByTopRegion = false, RegionFilter regionFilter = RegionFilter.None, ItemType itemFilter = ItemType.Nothing, Region? inRegion = null, bool keysanityByRegion = false, bool checkAllWorlds = false)
    {
        var progression = keysanityByRegion ? null : Progression(assumeKeys);

        if (sortByTopRegion)
        {
            // If we're sorting by regions, first grab all regions matching the region filter
            // Then get that region with all of its valid locations for the filter options
            // Order by last cleared region, then by location count per region
            return Regions
                .Where(x => x.MatchesFilter(regionFilter))
                .Select(x => (Region: x, Locations: x.Locations.Where(l => IsValidLocation(l, unclearedOnly, outOfLogic, progression, itemFilter, inRegion))))
                .OrderBy(x => x.Region != World.LastClearedLocation?.Region)
                .ThenByDescending(x => x.Locations.Count())
                .SelectMany(x => x.Locations)
                .ToImmutableList();
        }
        else
        {
            var locations = checkAllWorlds ? AllMultiworldLocations() : AllLocations();
            return locations
                .Where(x => IsValidLocation(x, unclearedOnly, outOfLogic, progression, itemFilter, inRegion))
                .ToImmutableList();
        }
    }

    private bool IsValidLocation(Location location, bool unclearedOnly, bool outOfLogic, Progression? progression, ItemType itemFilter, Region? inRegion)
    {
        return (!unclearedOnly || location.Cleared == false) && (outOfLogic || IsAvailable(location, progression ?? Progression(location.Region))) && (itemFilter == ItemType.Nothing || location.Item.Is(itemFilter, World)) && (inRegion == null || location.Region == inRegion);
    }

    /// <summary>
    /// Enumerates all items that can be tracked for all players.
    /// </summary>
    /// <returns>A collection of items.</returns>
    public IEnumerable<Item> AllItems() // I really want to discourage this, but necessary for now
        => Worlds.SelectMany(x => x.AllItems);

    /// <summary>
    /// Enumerates all items that can be tracked for the local player.
    /// </summary>
    /// <returns>A collection of items.</returns>
    public IEnumerable<Item> LocalPlayersItems()
        => AllItems().Where(x => x.World.Id == World.Id);

    /// <summary>
    /// Enumerates all rewards that can be tracked for all players.
    /// </summary>
    /// <returns>A collection of rewards.</returns>

    public virtual IEnumerable<Reward> AllRewards()
        => Worlds.SelectMany(x => x.Rewards);

    /// <summary>
    /// Enumerates all rewards that can be tracked for the local player.
    /// </summary>
    /// <returns>A collection of rewards.</returns>

    public virtual IEnumerable<Reward> LocalPlayersRewards()
        => World.Rewards;

    /// <summary>
    /// Enumarates all currently tracked rewards for the local player.
    /// This uses what the player marked as the reward for dungeons,
    /// not the actual dungeon reward.
    /// </summary>
    /// <returns>
    /// A collection of reward that have been tracked.
    /// </returns>
    public virtual IEnumerable<Reward> TrackedRewards()
        => World.RewardRegions.Where(x => x.HasReceivedReward)
            .Select(x => x.Reward);

    /// <summary>
    /// Enumerates all bosses that can be tracked for all players.
    /// </summary>
    /// <returns>A collection of bosses.</returns>

    public virtual IEnumerable<Boss> AllBosses()
        => Worlds.SelectMany(x => x.AllBosses);

    /// <summary>
    /// Enumerates all bosses that can be tracked for the local player.
    /// </summary>
    /// <returns>A collection of bosses.</returns>

    public virtual IEnumerable<Boss> LocalPlayersBosses()
        => World.AllBosses;

    /// <summary>
    /// Enumarates all currently tracked bosses for the local player.
    /// </summary>
    /// <returns>
    /// A collection of bosses that have been tracked.
    /// </returns>
    public virtual IEnumerable<Boss> TrackedBosses()
        => LocalPlayersBosses().Where(x => x.Defeated);


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
    public Item? FirstOrDefault(string name)
        => LocalPlayersItems().FirstOrDefault(x => x.Is(name));

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
    public Item? FirstOrDefault(ItemType itemType)
        => LocalPlayersItems().FirstOrDefault(x => x.Type == itemType);

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
    public Reward? FirstOrDefault(RewardType rewardType)
        => World.Rewards.FirstOrDefault(x => x.Type == rewardType);


    /// <summary>
    /// Returns the specific location matching the given id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Location Location(LocationId id)
    {
        return AllLocations().First(x => x.Id == id);
    }

    /// <summary>
    /// Checks if the location is accessible with the given progression
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <param name="progression">The player's current inventory and rewards</param>
    /// <returns>True if accessible, false otherwise</returns>
    public bool IsAvailable(Location location, Progression progression)
        => location.IsAvailable(progression, true);

    /// <summary>
    /// Checks if the location is accessible
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <param name="assumeKeys">If keys should be ignored when it comes to determining logic</param>
    /// <returns>True if accessible, false otherwise</returns>
    public bool IsAvailable(Location location, bool assumeKeys)
        => IsAvailable(location, Progression(assumeKeys));

    /// <summary>
    /// Checks if the location is accessible based on that region's Keysanity logic
    /// </summary>
    /// <param name="location">The location to check</param>
    /// <returns>True if accessible, false otherwise</returns>
    public bool IsAvailable(Location location)
        => IsAvailable(location, Progression(!IsKeysanityForLocation(location)));

    /// <summary>
    /// Returns all regions in the current player's world
    /// </summary>
    public IEnumerable<Region> Regions
        => World.Regions;

    /// <summary>
    /// Returns the region matching the given name
    /// </summary>
    /// <param name="name">The region name to lookup</param>
    /// <returns></returns>
    public Region? Region(string name)
        => World.Regions.FirstOrDefault(x => x.Name == name || x.GetType().FullName == name);

    public IEnumerable<PlayerHintTile> ViewedHintTiles
        => World.HintTiles.Where(x => x.HintState == HintState.Viewed);

    private bool IsKeysanityForLocation(Location location)
        => World.Config.KeysanityForRegion(location.Region);

    private Progression Progression(bool assumeKeys)
        => _playerProgressionService.GetProgression(assumeKeys);

    private Progression Progression(Region region)
        => _playerProgressionService.GetProgression(region);


}
