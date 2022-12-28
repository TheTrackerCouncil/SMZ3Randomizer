using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;

namespace Randomizer.SMZ3.Tracking.Services
{
    public interface IWorldService
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
        public Location Location(int id);

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
    }
}
