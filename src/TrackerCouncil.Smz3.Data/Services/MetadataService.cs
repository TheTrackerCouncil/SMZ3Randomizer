using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
internal class MetadataService : IMetadataService
{
    private readonly ConfigProvider _configProvider;
    private readonly ILogger<MetadataService> _logger;
    private readonly OptionsFactory _optionsFactory;
    private readonly TrackerSpriteService _trackerSpriteService;

    public MetadataService(ConfigProvider configs, ILogger<MetadataService> logger,
        OptionsFactory optionsFactory, TrackerSpriteService trackerSpriteService)
    {
        _configProvider = configs;
        _logger = logger;
        _optionsFactory = optionsFactory;
        _trackerSpriteService = trackerSpriteService;
        ReloadConfigs();
    }

    public RegionConfig Regions { get; private set; } = null!;

    public RoomConfig Rooms { get; private set; } = null!;

    public LocationConfig Locations { get; private set; } = null!;

    public BossConfig Bosses { get; private set; } = null!;

    public ItemConfig Items { get; private set; } = null!;

    public RewardConfig Rewards { get; private set; } = null!;

    public GameLinesConfig GameLines { get; private set; } = null!;

    public HintTileConfig HintTiles { get; private set; } = null!;

    public MetadataConfig Metadata { get; private set; } = null!;

    public MsuConfig MsuConfig { get; private set; } = null!;

    public IReadOnlyCollection<BasicVoiceRequest> Requests { get; private set; } = null!;

    public ResponseConfig Responses { get; private set; } = null!;

    public UIConfig UILayouts { get; private set; } = null!;

    public string Mood { get; private set; } = null!;

    public TrackerProfileConfig? TrackerSpriteProfile { get; private set; } = null!;

    public void ReloadConfigs()
    {
        var options = _optionsFactory.Create();

        var trackerPack = options.GeneralOptions.TrackerSpeechImagePack;
        if (string.IsNullOrEmpty(trackerPack))
        {
            trackerPack = "default";
        }
        TrackerSpriteProfile = _trackerSpriteService.GetPack(trackerPack)?.ProfileConfig;

        var profiles = options.GeneralOptions.SelectedProfiles.NonNull().ToImmutableList();
        Mood = _configProvider.GetAvailableMoods(profiles.NonNull().ToImmutableList()).Random(Random.Shared) ?? "";

        _logger.LogInformation("Utilizing tracker profiles {List}", string.Join(", ", profiles));
        _logger.LogInformation("Tracker is feeling {Mood} today", Mood);

        if (TrackerSpriteProfile == null)
        {
            _logger.LogInformation("No tracker sprite profile config found for profile {Name}", trackerPack);
            Bosses = _configProvider.GetBossConfig(profiles, Mood);
            GameLines = _configProvider.GetGameConfig(profiles, Mood);
            HintTiles = _configProvider.GetHintTileConfig(profiles, Mood);
            Items = _configProvider.GetItemConfig(profiles, Mood);
            Locations = _configProvider.GetLocationConfig(profiles, Mood);
            Metadata = _configProvider.GetMetadataConfig(profiles, Mood);
            MsuConfig = _configProvider.GetMsuConfig(profiles, Mood);
            Regions = _configProvider.GetRegionConfig(profiles, Mood);
            Requests = _configProvider.GetRequestConfig(profiles, Mood);
            Responses = _configProvider.GetResponseConfig(profiles, Mood);
            Rewards = _configProvider.GetRewardConfig(profiles, Mood);
            Rooms = _configProvider.GetRoomConfig(profiles, Mood);
            UILayouts = _configProvider.GetUIConfig(profiles, Mood);
        }
        else
        {
            _logger.LogInformation("Adding tracker sprite profile {Name} config", trackerPack);
            Bosses = IMergeable<BossInfo>.Combine(_configProvider.GetBossConfig(profiles, Mood), TrackerSpriteProfile.BossConfig);
            GameLines = _configProvider.GetGameConfig(profiles, Mood);
            HintTiles = _configProvider.GetHintTileConfig(profiles, Mood);
            Items = IMergeable<ItemData>.Combine(_configProvider.GetItemConfig(profiles, Mood), TrackerSpriteProfile.ItemConfig);
            Locations = IMergeable<LocationInfo>.Combine(_configProvider.GetLocationConfig(profiles, Mood), TrackerSpriteProfile.LocationConfig);
            Metadata = _configProvider.GetMetadataConfig(profiles, Mood);
            MsuConfig = _configProvider.GetMsuConfig(profiles, Mood);
            Regions = IMergeable<RegionInfo>.Combine(_configProvider.GetRegionConfig(profiles, Mood), TrackerSpriteProfile.RegionConfig);
            Requests = IMergeable<BasicVoiceRequest>.Combine(_configProvider.GetRequestConfig(profiles, Mood), TrackerSpriteProfile.RequestConfig);
            Responses = IMergeable<ResponseConfig>.Combine(_configProvider.GetResponseConfig(profiles, Mood), TrackerSpriteProfile.ResponseConfig);
            Rewards = IMergeable<RewardInfo>.Combine(_configProvider.GetRewardConfig(profiles, Mood), TrackerSpriteProfile.RewardConfig);
            Rooms = IMergeable<RoomInfo>.Combine(_configProvider.GetRoomConfig(profiles, Mood), TrackerSpriteProfile.RoomConfig);
            UILayouts = _configProvider.GetUIConfig(profiles, Mood);
        }
    }

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
