using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.SMZ3.Contracts;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Service for finding locations in the world
    /// </summary>
    public class WorldService : IWorldService
    {
        private readonly IWorldAccessor _worldAccessor;
        private readonly IItemService _itemService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="world"></param>
        /// <param name="itemService"></param>
        public WorldService(IWorldAccessor world, IItemService itemService)
        {
            _worldAccessor = world;
            _itemService = itemService;
        }

        /// <summary>
        /// Retrieves the world for the current player
        /// </summary>
        public World World => _worldAccessor.World;

        /// <summary>
        /// Retrives all locations for current player's world
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Location> AllLocations() => World.Locations;

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
            => AllLocations().Where(x => !x.State.Cleared).ToImmutableList();

        /// <summary>
        /// Retrieves all locations for the current player's world that has been marked as
        /// having an item at it
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Location> MarkedLocations()
            => AllLocations().Where(x => !x.State.Cleared && x.State.MarkedItem != null && x.State.MarkedItem != Shared.ItemType.Nothing).ToImmutableList();

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
        /// <returns></returns>
        public IEnumerable<Location> Locations(bool unclearedOnly = true, bool outOfLogic = false, bool assumeKeys = false, bool sortByTopRegion = false, RegionFilter regionFilter = RegionFilter.None, ItemType itemFilter = ItemType.Nothing, Region? inRegion = null, bool keysanityByRegion = false)
        {
            var progression = keysanityByRegion ? null : Progression(assumeKeys);

            if (sortByTopRegion)
            {
                // If we're sorting by regions, first grab all regions matching the region filter
                // Then get that region with all of its valid locations for the filter options
                // Order by last cleared region, then by location count per region
                return Regions
                    .Where(x => RegionMatchesFilter(regionFilter, x))
                    .Select(x => (Region: x, Locations: x.Locations.Where(x => IsValidLocation(x, unclearedOnly, outOfLogic, progression, itemFilter, inRegion))))
                    .OrderBy(x => x.Region != World.LastClearedLocation?.Region)
                    .ThenByDescending(x => x.Locations.Count())
                    .SelectMany(x => x.Locations)
                    .ToImmutableList();
            }
            else
            {
                return AllLocations()
                    .Where(x => IsValidLocation(x, unclearedOnly, outOfLogic, progression, itemFilter, inRegion))
                    .ToImmutableList();
            }

            bool IsValidLocation(Location location, bool unclearedOnly, bool outOfLogic, Progression? progression, ItemType itemFilter, Region? inRegion)
            {
                return (!unclearedOnly || !location.State.Cleared) && (outOfLogic || IsAvailable(location, progression ?? Progression(location.Region))) && (itemFilter == ItemType.Nothing || location.Item.Is(itemFilter, World)) && (inRegion == null || location.Region == inRegion);
            }
        }

        /// <summary>
        /// Returns the specific location matching the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Location Location(int id)
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

        /// <summary>
        /// Returns if a given region matches the LocationFilter
        /// </summary>
        /// <param name="filter">The filter to apply</param>
        /// <param name="region">The SMZ3 region to check</param>
        /// <returns>True if the region matches, false otherwise</returns>
        private static bool RegionMatchesFilter(RegionFilter filter, Region region) => filter switch
        {
            RegionFilter.None => true,
            RegionFilter.ZeldaOnly => region is Z3Region,
            RegionFilter.MetroidOnly => region is SMRegion,
            _ => throw new InvalidEnumArgumentException(nameof(filter), (int)filter, typeof(RegionFilter)),
        };

        private bool IsKeysanityForLocation(Location location)
            => World.Config.KeysanityForRegion(location.Region);

        private Progression Progression(bool assumeKeys)
            => _itemService.GetProgression(assumeKeys);

        private Progression Progression(Region region)
            => _itemService.GetProgression(region);

        
    }
}
