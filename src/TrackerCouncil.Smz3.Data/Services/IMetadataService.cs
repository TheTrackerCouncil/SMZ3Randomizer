using System;
using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
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
    public RegionConfig Regions { get; }

    /// <summary>
    /// Collection of all additional room information
    /// </summary>
    public RoomConfig Rooms { get; }

    /// <summary>
    /// Collection of all additional location information
    /// </summary>
    public LocationConfig Locations { get; }

    /// <summary>
    /// Collection of all additional boss information
    /// </summary>
    public BossConfig Bosses { get; }

    /// <summary>
    /// Collection of all additional item information
    /// </summary>
    public ItemConfig Items { get; }

    /// <summary>
    /// Collection of all additional reward information
    /// </summary>
    public RewardConfig Rewards { get; }

    /// <summary>
    /// Lines that are displayed in the rom itself
    /// </summary>
    public GameLinesConfig GameLines { get; }

    /// <summary>
    /// Data about all of the potential hint tiles and text used
    /// </summary>
    public HintTileConfig HintTiles { get; }

    /// <summary>
    /// General tracker metadata and settings
    /// </summary>
    public MetadataConfig Metadata { get; }

    /// <summary>
    /// Data for MSUs and particular track responses
    /// </summary>
    public MsuConfig MsuConfig { get; }

    /// <summary>
    /// Different requests that the user can ask tracker
    /// </summary>
    public IReadOnlyCollection<BasicVoiceRequest> Requests { get; }

    /// <summary>
    /// Different lines for tracker to respond to the player
    /// </summary>
    public ResponseConfig Responses { get; }

    /// <summary>
    /// Layouts used by the UI
    /// </summary>
    public UIConfig UILayouts { get; }

    /// <summary>
    /// The current selected mood
    /// </summary>
    public string Mood { get; }

    /// <summary>
    /// The selected tracker sprite profile config, if applicable
    /// </summary>
    public TrackerProfileConfig? TrackerSpriteProfile { get; }

    /// <summary>
    /// Updates the configs and picks a new mood
    /// </summary>
    public void ReloadConfigs();

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
