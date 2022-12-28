using System.Collections.Generic;
using System.Linq;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Logic;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions
{
    /// <summary>
    /// Represents a region in a game.
    /// </summary>
    public abstract class Region : IHasLocations
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Region"/> class for the
        /// specified world and configuration.
        /// </summary>
        /// <param name="world">The world the region is in.</param>
        /// <param name="config">The config used.</param>
        /// <param name="metadata"></param>
        /// <param name="trackerState"></param>
        protected Region(World world, Config config, IMetadataService? metadata, TrackerState? trackerState)
        {
            Config = config;
            World = world;
            Metadata = null!;
        }

        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the name of the overall area the region is a part of.
        /// </summary>
        public virtual string Area => Name;

        /// <summary>
        /// Gets a collection of alternate names for the region.
        /// </summary>
        public virtual IReadOnlyCollection<string> AlsoKnownAs { get; } = new List<string>();

        /// <summary>
        /// Gets a collection of every location in the region.
        /// </summary>
        public IEnumerable<Location> Locations => GetStandaloneLocations()
            .Concat(GetRooms().SelectMany(x => x.GetLocations()));

        /// <summary>
        /// Gets a collection of every room in the region.
        /// </summary>
        public IEnumerable<Room> Rooms => GetRooms();

        /// <summary>
        /// Gets the world the region is located in.
        /// </summary>
        public World World { get; }

        /// <summary>
        /// Gets the relative weight used to bias the randomization process.
        /// </summary>
        public int Weight { get; init; }

        /// <summary>
        /// Gets the randomizer configuration options.
        /// </summary>
        public Config Config { get; }

        /// <summary>
        /// The Region's metadata
        /// </summary>
        public RegionInfo Metadata { get; set; }

        /// <summary>
        /// The Logic to be used to determine if certain actions can be done
        /// </summary>
        public ILogic Logic => World.Logic;

        /// <summary>
        /// Gets the list of region-specific items, e.g. keys, maps, compasses.
        /// </summary>
        protected IList<ItemType> RegionItems { get; init; } = new List<ItemType>();

        /// <summary>
        /// Determines whether the specified item is specific to this region.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>
        /// <see langword="true"/> if the item is specific to this region;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsRegionItem(Item item)
        {
            return RegionItems.Contains(item.Type);
        }

        /// <summary>
        /// Determines whether the specified item can be assigned to a location
        /// in this region.
        /// </summary>
        /// <param name="item">The item to fill.</param>
        /// <param name="items">The currently available items.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="item"/> can be
        /// assigned to a location in this region; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        public virtual bool CanFill(Item item, Progression items)
        {
            return (item.World.Config.ZeldaKeysanity || !item.IsDungeonItem || IsRegionItem(item)) && MatchesItemPlacementRule(item);
        }

        private bool MatchesItemPlacementRule(Item item)
        {
            if (Config.MultiWorld) return true;
            var rule = Config.ItemPlacementRule;
            if (rule == ItemPlacementRule.Anywhere
                || (!item.Progression && !item.IsKey && !item.IsKeycard && !item.IsBigKey)
                || (!item.World.Config.ZeldaKeysanity && (item.IsKey || item.IsBigKey))) return true;
            else if (rule == ItemPlacementRule.DungeonsAndMetroid)
            {
                return this is Z3Region { IsOverworld: false } || this is SMRegion;
            }
            else if (rule == ItemPlacementRule.CrystalDungeonsAndMetroid)
            {
                return this is IHasReward { RewardType: RewardType.CrystalBlue or RewardType.CrystalRed } || this is SMRegion;
            }
            else if (rule == ItemPlacementRule.OppositeGame)
            {
                return (item.Type.IsInCategory(ItemCategory.Zelda) && this is SMRegion) || (item.Type.IsInCategory(ItemCategory.Metroid) && this is Z3Region);
            }
            else if (rule == ItemPlacementRule.SameGame)
            {
                return (item.Type.IsInCategory(ItemCategory.Zelda) && this is Z3Region) || (item.Type.IsInCategory(ItemCategory.Metroid) && this is SMRegion);
            }
            return true;
        }

        /// <summary>
        /// Returns a string that represents the region.
        /// </summary>
        /// <returns>A new string that represents the region.</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Determines whether the region can be entered with the specified
        /// items.
        /// </summary>
        /// <param name="items">The currently available items.</param>
        /// <param name="requireRewards">If dungeon/boss rewards are required for the check</param>
        /// <returns>
        /// <see langword="true"/> if the region is available with <paramref
        /// name="items"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool CanEnter(Progression items, bool requireRewards)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of all locations in this region that are not
        /// part of a room.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="Location"/> that do not exist in <see
        /// cref="Rooms"/>.
        /// </returns>
        public IEnumerable<Location> GetStandaloneLocations()
            => GetType().GetPropertyValues<Location>(this);

        protected IEnumerable<Room> GetRooms()
            => GetType().GetPropertyValues<Room>(this);

        public bool CheckDungeonMedallion(Progression items, IDungeon dungeon)
        {
            if (!dungeon.NeedsMedallion) return true;
            var medallionItem = dungeon.MarkedMedallion;
            return (medallionItem != ItemType.Nothing && items.Contains(medallionItem)) ||
                (items.Bombos && items.Ether && items.Quake);
        }

        public int CountReward(Progression items, RewardType reward)
        {
            return World.Dungeons
                .Where(x => x is IHasReward rewardRegion && x.MarkedReward == reward)
                .Count(x => x.DungeonState.Cleared);
        }
    }
}
