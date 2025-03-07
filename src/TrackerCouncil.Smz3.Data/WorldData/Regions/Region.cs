﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions;

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
        .Concat(GetRooms().SelectMany(x => x.Locations));

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
    /// Gets the map of generic items to region-specific items, e.g. keys, maps, compasses
    /// </summary>
    protected IDictionary<ItemType, ItemType> RegionItems { get; init; } = new Dictionary<ItemType, ItemType>();

    /// <summary>
    /// Name of the map to display when in this region
    /// </summary>
    public string MapName { get; set; } = "";

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
        return RegionItems.Values.Contains(item.Type);
    }

    /// <summary>
    /// Takes a generic item (e.g. key, compass, etc.) and returns the regional specific version of it if found
    /// </summary>
    /// <param name="originalType">The generic item that is desired to be replaced</param>
    /// <returns>The regional version of the item, if found. Returns the originalType passed in if not found.</returns>
    public ItemType ConvertToRegionItemType(ItemType originalType)
    {
        return RegionItems.TryGetValue(originalType, out var itemType) ? itemType : originalType;
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
    /// Returns a random string from the region's metadata
    /// </summary>
    public string RandomAreaName => Metadata.Name?.ToString() ?? Name;

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

    public Accessibility GetKeysanityAdjustedAccessibility(Accessibility accessibility)
    {
        if (Config.KeysanityForRegion(this))
        {
            return accessibility;
        }
        else if (accessibility == Accessibility.AvailableWithKeys)
        {
            return Accessibility.Available;
        }
        else if (accessibility == Accessibility.RelevantWithKeys)
        {
            return Accessibility.Relevant;
        }

        return accessibility;
    }

    /// <summary>
    /// Returns if the region matches the LocationFilter
    /// </summary>
    /// <param name="filter">The filter to apply</param>
    /// <returns>True if the region matches, false otherwise</returns>
    public bool MatchesFilter(RegionFilter filter) => filter switch
    {
        RegionFilter.None => true,
        RegionFilter.ZeldaOnly => this is Z3Region,
        RegionFilter.MetroidOnly => this is SMRegion,
        _ => throw new InvalidEnumArgumentException(nameof(filter), (int)filter, typeof(RegionFilter)),
    };

    /*public bool CheckDungeonMedallion(Progression items, IDungeon dungeon)
    {
        if (!dungeon.NeedsMedallion) return true;
        var medallionItem = dungeon.MarkedMedallion;
        return (medallionItem != ItemType.Nothing && items.Contains(medallionItem)) ||
               (items.Bombos && items.Ether && items.Quake);
    }*/
}
