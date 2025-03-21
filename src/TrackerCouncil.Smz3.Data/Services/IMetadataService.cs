﻿using System;
using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.Services;

/// <summary>
/// Service for retrieving information about the current state of
/// the world
/// </summary>
public interface IMetadataService
{
    /// <summary>
    /// Collection of all additional region information
    /// </summary>
    public IReadOnlyCollection<RegionInfo> Regions { get; }

    /// <summary>
    /// Collection of all additional dungeon information
    /// </summary>
    public IReadOnlyCollection<DungeonInfo> Dungeons { get; }

    /// <summary>
    /// Collection of all additional room information
    /// </summary>
    public IReadOnlyCollection<RoomInfo> Rooms { get; }

    /// <summary>
    /// Collection of all additional location information
    /// </summary>
    public IReadOnlyCollection<LocationInfo> Locations { get; }

    /// <summary>
    /// Collection of all additional boss information
    /// </summary>
    public IReadOnlyCollection<BossInfo> Bosses { get; }

    /// <summary>
    /// Collection of all additional item information
    /// </summary>
    public IReadOnlyCollection<ItemData> Items { get; }

    /// <summary>
    /// Collection of all additional reward information
    /// </summary>
    public IReadOnlyCollection<RewardInfo> Rewards { get; }

    /// <summary>
    /// Returns extra information for the specified region.
    /// </summary>
    /// <param name="name">
    /// The name or fully qualified type name of the region.
    /// </param>
    /// <returns>
    /// A new <see cref="RegionInfo"/> for the specified region.
    /// </returns>
    public RegionInfo Region(string name);

    /// <summary>
    /// Returns extra information for the specified region.
    /// </summary>
    /// <param name="type">
    /// The type of the region.
    /// </param>
    /// <returns>
    /// A new <see cref="RegionInfo"/> for the specified region.
    /// </returns>
    public RegionInfo Region(Type type);

    /// <summary>
    /// Returns extra information for the specified region.
    /// </summary>
    /// <param name="region">The region to get extra information for.</param>
    /// <returns>
    /// A new <see cref="RegionInfo"/> for the specified region.
    /// </returns>
    public RegionInfo Region(Region region);

    /// <summary>
    /// Returns extra information for the specified region.
    /// </summary>
    /// <typeparam name="TRegion">
    /// The type of region to get extra information for.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="RegionInfo"/> for the specified region.
    /// </returns>
    public RegionInfo Region<TRegion>() where TRegion : Region;

    /// <summary>
    /// Returns extra information for the specified dungeon.
    /// </summary>
    /// <param name="name">
    /// The name or fully qualified type name of the dungeon region.
    /// </param>
    /// <returns>
    /// A new <see cref="DungeonInfo"/> for the specified dungeon region, or
    /// <c>null</c> if <paramref name="name"/> is not a valid dungeon.
    /// </returns>
    public DungeonInfo? Dungeon(string name);

    /// <summary>
    /// Returns extra information for the specified dungeon.
    /// </summary>
    /// <param name="type">
    /// The type of dungeon to be looked up
    /// </param>
    /// <returns>
    /// A new <see cref="DungeonInfo"/> for the specified dungeon region, or
    /// <c>null</c> if <paramref name="type"/> is not a valid dungeon.
    /// </returns>
    public DungeonInfo Dungeon(Type type);

    /// <summary>
    /// Returns extra information for the specified dungeon.
    /// </summary>
    /// <param name="region">
    /// The dungeon region to get extra information for.
    /// </param>
    /// <returns>
    /// A new <see cref="DungeonInfo"/> for the specified dungeon region.
    /// </returns>
    public DungeonInfo Dungeon(Region region);

    /// <summary>
    /// Returns extra information for the specified dungeon.
    /// </summary>
    /// <typeparam name="TRegion">
    /// The type of region that represents the dungeon to get extra
    /// information for.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="DungeonInfo"/> for the specified dungeon region.
    /// </returns>
    public DungeonInfo Dungeon<TRegion>() where TRegion : Region;

    /// <summary>
    /// Returns extra information for the specified dungeon.
    /// </summary>
    /// <param name="hasTreasure">
    /// The dungeon to get extra information for.
    /// </param>
    /// <returns>
    /// A new <see cref="DungeonInfo"/> for the specified dungeon region.
    /// </returns>
    public DungeonInfo Dungeon(IHasTreasure hasTreasure);

    /// <summary>
    /// Returns extra information for the specified room.
    /// </summary>
    /// <param name="name">
    /// The name or fully qualified type name of the room.
    /// </param>
    /// <returns>
    /// A new <see cref="RoomInfo"/> for the specified room.
    /// </returns>
    public RoomInfo Room(string name);

    /// <summary>
    /// Returns extra information for the specified room.
    /// </summary>
    /// <param name="type">
    /// The type of the room.
    /// </param>
    /// <returns>
    /// A new <see cref="RoomInfo"/> for the specified room.
    /// </returns>
    public RoomInfo? Room(Type type);

    /// <summary>
    /// Returns extra information for the specified room.
    /// </summary>
    /// <param name="room">The room to get extra information for.</param>
    /// <returns>
    /// A new <see cref="RoomInfo"/> for the specified room.
    /// </returns>
    public RoomInfo? Room(Room room);

    /// <summary>
    /// Returns extra information for the specified room.
    /// </summary>
    /// <typeparam name="TRoom">
    /// The type of room to get extra information for.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="RoomInfo"/> for the specified room.
    /// </returns>
    public RoomInfo? Room<TRoom>() where TRoom : Room;

    /// <summary>
    /// Returns extra information for the specified location.
    /// </summary>
    /// <param name="id">The numeric ID of the location.</param>
    /// <returns>
    /// A new <see cref="LocationInfo"/> for the specified room.
    /// </returns>
    public LocationInfo Location(LocationId id);

    /// <summary>
    /// Returns extra information for the specified location.
    /// </summary>
    /// <param name="location">
    /// The location to get extra information for.
    /// </param>
    /// <returns>
    /// A new <see cref="LocationInfo"/> for the specified room.
    /// </returns>
    public LocationInfo Location(Location location);

    /// <summary>
    /// Returns information about a specified boss
    /// </summary>
    /// <param name="name">The name of the boss</param>
    /// <returns>The <see cref="BossInfo"/> for the specified boss.</returns>
    public BossInfo? Boss(string name);

    /// <summary>
    /// Returns information about a specified boss
    /// </summary>
    /// <param name="boss">The type of the boss</param>
    /// <returns>The <see cref="BossInfo"/> for the specified boss.</returns>
    public BossInfo? Boss(BossType boss);

    /// <summary>
    /// Returns information about a specified item
    /// </summary>
    /// <param name="type">The type of the item</param>
    /// <returns></returns>
    public ItemData? Item(ItemType type);

    /// <summary>
    /// Returns information about a specified item
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <returns></returns>
    public ItemData? Item(string name);

    /// <summary>
    /// Returns information about a specified reward
    /// </summary>
    /// <param name="type">The type of the reward</param>
    /// <returns></returns>
    public RewardInfo? Reward(RewardType type);

    /// <summary>
    /// Returns a random name for the specified item including article, e.g.
    /// "an E-Tank" or "the Book of Mudora".
    /// </summary>
    /// <param name="itemType">The type of item whose name to get.</param>
    /// <returns>
    /// The name of the type of item, including "a", "an" or "the" if
    /// applicable.
    /// </returns>
    string GetName(ItemType itemType);


    /// <summary>
    /// Returns a random name for the specified item including article, e.g.
    /// "a blue crystal" or "the green pendant".
    /// </summary>
    /// <param name="rewardType">The reward of item whose name to get.</param>
    /// <returns>
    /// The name of the reward of item, including "a", "an" or "the" if
    /// applicable.
    /// </returns>
    string GetName(RewardType rewardType);

}
