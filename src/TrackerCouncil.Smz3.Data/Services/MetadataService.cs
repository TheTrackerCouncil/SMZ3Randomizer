using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.Services;

/// <summary>
/// Service for retrieving additional metadata information
/// about objects and locations within the world
/// </summary>
public class MetadataService : IMetadataService
{
    private readonly ILogger<MetadataService> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configs">All configs</param>
    /// <param name="logger"></param>
    /// <param name="trackerOptionsAccessor"></param>
    /// <param name="trackerSpriteService"></param>
    public MetadataService(Configs configs, ILogger<MetadataService> logger, TrackerOptionsAccessor trackerOptionsAccessor, TrackerSpriteService trackerSpriteService)
    {
        var options = trackerOptionsAccessor.Options;
        TrackerProfileConfig? profileConfig = null;

        if (options != null)
        {
            var trackerPack = options.TrackerImagePackName ?? "default";
            profileConfig = trackerSpriteService.GetPack(trackerPack).ProfileConfig;
        }

        if (profileConfig == null)
        {
            Bosses = configs.Bosses;
            GameLines = configs.GameLines;
            HintTiles = configs.HintTileConfig;
            Items = configs.Items;
            Locations = configs.Locations;
            Metadata = configs.MetadataConfig;
            MsuConfig = configs.MsuConfig;
            Regions = configs.Regions;
            Requests = configs.Requests;
            Responses = configs.Responses;
            Rewards = configs.Rewards;
            Rooms = configs.Rooms;
            UILayouts = configs.UILayouts;
        }
        else
        {
            Bosses = IMergeable<BossInfo>.Combine(BossConfig.Default(), configs.Bosses, profileConfig.BossConfig);
            GameLines = IMergeable<GameLinesConfig>.Combine(GameLinesConfig.Default(), configs.GameLines);
            HintTiles = IMergeable<HintTileConfig>.Combine(HintTileConfig.Default(), configs.HintTileConfig);
            Items = IMergeable<ItemData>.Combine(ItemConfig.Default(), configs.Items, profileConfig.ItemConfig);
            Locations = IMergeable<LocationInfo>.Combine(LocationConfig.Default(), configs.Locations, profileConfig.LocationConfig);
            Metadata = IMergeable<MetadataConfig>.Combine(MetadataConfig.Default(), configs.MetadataConfig);
            MsuConfig = IMergeable<MsuConfig>.Combine(MsuConfig.Default(), configs.MsuConfig);
            Regions = IMergeable<RegionInfo>.Combine(RegionConfig.Default(), configs.Regions, profileConfig.RegionConfig);
            Requests = IMergeable<BasicVoiceRequest>.Combine(RequestConfig.Default(), configs.Requests, profileConfig.RequestConfig);
            Responses = IMergeable<ResponseConfig>.Combine(ResponseConfig.Default(), configs.Responses, profileConfig.ResponseConfig);
            Rewards = IMergeable<RewardInfo>.Combine(RewardConfig.Default(), configs.Rewards, profileConfig.RewardConfig);
            Rooms = IMergeable<RoomInfo>.Combine(RoomConfig.Default(), configs.Rooms, profileConfig.RoomConfig);
            UILayouts = IMergeable<UILayout>.Combine(UIConfig.Default(), configs.UILayouts);
        }

        _logger = logger;
    }

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

    public GameLinesConfig GameLines { get; }

    public HintTileConfig HintTiles { get; }

    public MetadataConfig Metadata { get; }

    public MsuConfig MsuConfig { get; }

    public IReadOnlyCollection<BasicVoiceRequest> Requests { get; }

    public ResponseConfig Responses { get; }

    public UIConfig UILayouts { get; set; }

    /// <summary>
    /// Returns extra information for the specified region.
    /// </summary>
    /// <param name="name">
    /// The name or fully qualified type name of the region.
    /// </param>
    /// <returns>
    /// A new <see cref="RegionInfo"/> for the specified region.
    /// </returns>
    public RegionInfo Region(string name)
        => Regions.Single(x => x.Type?.Name == name || x.Region == name);

    /// <summary>
    /// Returns extra information for the specified region.
    /// </summary>
    /// <param name="type">
    /// The Randomizer.SMZ3 type matching the region.
    /// </param>
    /// <returns>
    /// A new <see cref="RegionInfo"/> for the specified region.
    /// </returns>
    public RegionInfo Region(Type type)
        => Regions.Single(x => x.Type == type);

    /// <summary>
    /// Returns extra information for the specified region.
    /// </summary>
    /// <param name="region">The region to get extra information for.</param>
    /// <returns>
    /// A new <see cref="RegionInfo"/> for the specified region.
    /// </returns>
    public RegionInfo Region(Region region)
        => Region(region.GetType());

    /// <summary>
    /// Returns extra information for the specified region.
    /// </summary>
    /// <typeparam name="TRegion">
    /// The type of region to get extra information for.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="RegionInfo"/> for the specified region.
    /// </returns>
    public RegionInfo Region<TRegion>() where TRegion : Region
        => Region(typeof(TRegion));

    /// <summary>
    /// Returns extra information for the specified room.
    /// </summary>
    /// <param name="name">
    /// The name or fully qualified type name of the room.
    /// </param>
    /// <returns>
    /// A new <see cref="RoomInfo"/> for the specified room.
    /// </returns>
    public RoomInfo Room(string name)
        => Rooms.Single(x => x.Type?.Name == name || x.Room == name);

    /// <summary>
    /// Returns extra information for the specified room.
    /// </summary>
    /// <param name="type">
    /// The type of the room.
    /// </param>
    /// <returns>
    /// A new <see cref="RoomInfo"/> for the specified room.
    /// </returns>
    public RoomInfo? Room(Type type)
        => Rooms.SingleOrDefault(x => x.Type == type);

    /// <summary>
    /// Returns extra information for the specified room.
    /// </summary>
    /// <param name="room">The room to get extra information for.</param>
    /// <returns>
    /// A new <see cref="RoomInfo"/> for the specified room.
    /// </returns>
    public RoomInfo? Room(Room room)
        => Room(room.GetType());

    /// <summary>
    /// Returns extra information for the specified room.
    /// </summary>
    /// <typeparam name="TRoom">
    /// The type of room to get extra information for.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="RoomInfo"/> for the specified room.
    /// </returns>
    public RoomInfo? Room<TRoom>() where TRoom : Room
        => Room(typeof(TRoom));

    /// <summary>
    /// Returns extra information for the specified location.
    /// </summary>
    /// <param name="id">The ID of the location.</param>
    /// <returns>
    /// A new <see cref="LocationInfo"/> for the specified room.
    /// </returns>
    public LocationInfo Location(LocationId id)
        => Locations.Single(x => x.LocationNumber == (int)id);

    /// <summary>
    /// Returns extra information for the specified location.
    /// </summary>
    /// <param name="location">
    /// The location to get extra information for.
    /// </param>
    /// <returns>
    /// A new <see cref="LocationInfo"/> for the specified room.
    /// </returns>
    public LocationInfo Location(Location location)
        => Locations.Single(x => x.LocationNumber == (int)location.Id);

    /// <summary>
    /// Returns information about a specified boss
    /// </summary>
    /// <param name="name">The name of the boss</param>
    /// <returns>The <see cref="BossInfo"/> for the specified boss.</returns>
    public BossInfo? Boss(string name)
        => Bosses.SingleOrDefault(x => x.Boss == name);

    /// <summary>
    /// Returns information about a specified boss
    /// </summary>
    /// <param name="boss">The type of the boss</param>
    /// <returns>The <see cref="BossInfo"/> for the specified boss.</returns>
    public BossInfo? Boss(BossType boss)
        => Bosses.SingleOrDefault(x => x.Type == boss);

    /// <summary>
    /// Returns information about a specified item
    /// </summary>
    /// <param name="type">The type of the item</param>
    /// <returns></returns>
    public ItemData? Item(ItemType type)
        => Items.FirstOrDefault(x => x.InternalItemType == type);

    /// <summary>
    /// Returns information about a specified item
    /// </summary>
    /// <param name="name">The name of the item</param>
    /// <returns></returns>
    public ItemData? Item(string name)
        => Items.SingleOrDefault(x => x.Item == name);

    /// <summary>
    /// Returns information about a specified reward
    /// </summary>
    /// <param name="type">The type of the reward</param>
    /// <returns></returns>
    public RewardInfo? Reward(RewardType type)
        => Rewards.FirstOrDefault(x => x.RewardType == type);

    public string GetName(ItemType itemType) => Item(itemType)?.NameWithArticle ?? itemType.GetDescription();

    public string GetName(RewardType rewardType) => Reward(rewardType)?.NameWithArticle ?? rewardType.GetDescription();

}
