using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.Data.Configuration;

/// <summary>
/// Class that contains a collection of all configs with the user selected tracker profiles
/// </summary>
public class Configs
{
    private ConfigProvider _configProvider;
    private RandomizerOptions _randomizerOptions;
    private ILogger<Configs> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="optionsFactory">The tracker options for determining the selected tracker profiles</param>
    /// <param name="provider">The config provider for loading configs</param>
    /// <param name="logger"></param>
    public Configs(OptionsFactory optionsFactory, ConfigProvider provider, ILogger<Configs> logger)
    {
        _configProvider = provider;
        _randomizerOptions = optionsFactory.Create();
        _logger = logger;
        LoadConfigs();
    }

    public void LoadConfigs()
    {
        var profiles = _randomizerOptions.GeneralOptions.SelectedProfiles.NonNull().Where(x => x != "Default").ToArray();
        var moods = _configProvider.GetAvailableMoods(profiles);
        CurrentMood = moods.Random(Random.Shared);
        _logger.LogInformation("Tracker is feeling {Mood} today", CurrentMood);

        Bosses = _configProvider.GetBossConfig(profiles, CurrentMood);
        Dungeons = _configProvider.GetDungeonConfig(profiles, CurrentMood);
        Items = _configProvider.GetItemConfig(profiles, CurrentMood);
        Locations = _configProvider.GetLocationConfig(profiles, CurrentMood);
        Regions = _configProvider.GetRegionConfig(profiles, CurrentMood);
        Requests = _configProvider.GetRequestConfig(profiles, CurrentMood);
        Responses = _configProvider.GetResponseConfig(profiles, CurrentMood);
        Rooms = _configProvider.GetRoomConfig(profiles, CurrentMood);
        Rewards = _configProvider.GetRewardConfig(profiles, CurrentMood);
        UILayouts = _configProvider.GetUIConfig(profiles, CurrentMood);
        GameLines = _configProvider.GetGameConfig(profiles, CurrentMood);
        MsuConfig = _configProvider.GetMsuConfig(profiles, CurrentMood);
        HintTileConfig = _configProvider.GetHintTileConfig(profiles, CurrentMood);
        MetadataConfig = _configProvider.GetMetadataConfig(profiles, CurrentMood);
    }

    /// <summary>
    /// Gets the current mood.
    /// </summary>
    public string? CurrentMood { get; private set; }

    /// <summary>
    /// Gets a collection of trackable items.
    /// </summary>
    public ItemConfig Items { get; private set; } = null!;

    /// <summary>
    /// Gets the peg world peg configuration. This will be moved to UI
    /// configs in a future release, but just storing it here for now
    /// </summary>
    public IReadOnlyCollection<Peg> Pegs { get; } = new List<Peg>
    {
        new Peg(1, 0),
        new Peg(2, 0),
        new Peg(3, 0),
        new Peg(4, 0),
        new Peg(5, 0),
        new Peg(1, 1),
        new Peg(2, 1),
        new Peg(3, 1),
        new Peg(4, 1),
        new Peg(5, 1),
        new Peg(0, 2),
        new Peg(1, 2),
        new Peg(2, 2),
        new Peg(3, 2),
        new Peg(4, 2),
        new Peg(5, 2),
        new Peg(0, 3),
        new Peg(1, 3),
        new Peg(2, 3),
        new Peg(0, 4),
        new Peg(1, 4),
        new Peg(2, 4)
    };

    /// <summary>
    /// Gets a collection of configured responses.
    /// </summary>
    public ResponseConfig Responses { get; private set; } = null!;

    /// <summary>
    /// Gets a collection of basic requests and responses.
    /// </summary>
    public RequestConfig Requests { get; private set; } = null!;

    /// <summary>
    /// Gets a collection of extra information about regions.
    /// </summary>
    public RegionConfig Regions { get; private set; } = null!;

    /// <summary>
    /// Gets a collection of extra information about dungeons.
    /// </summary>
    public DungeonConfig Dungeons { get; private set; } = null!;

    /// <summary>
    /// Gets a collection of bosses.
    /// </summary>
    public BossConfig Bosses { get; private set; } = null!;

    /// <summary>
    /// Gets a collection of extra information about rooms.
    /// </summary>
    public RoomConfig Rooms { get; private set; } = null!;

    /// <summary>
    /// Gets a collection of extra information about locations.
    /// </summary>
    public LocationConfig Locations { get; private set; } = null!;

    /// <summary>
    /// Gets a collection of extra information about rewards
    /// </summary>
    public RewardConfig Rewards { get; private set; } = null!;

    /// <summary>
    /// Gets a collection of available UI layouts
    /// </summary>
    public UIConfig UILayouts { get; private set; } = null!;

    /// <summary>
    /// Gets the in game lines
    /// </summary>
    public GameLinesConfig GameLines { get; private set; } = null!;

    /// <summary>
    /// Gets the msu config
    /// </summary>
    public MsuConfig MsuConfig { get; private set; } = null!;

    /// <summary>
    /// Gets the hint tile config
    /// </summary>
    public HintTileConfig HintTileConfig { get; private set; } = null!;

    /// <summary>
    /// Gets the metadata config
    /// </summary>
    public MetadataConfig MetadataConfig { get; private set; } = null!;
}
