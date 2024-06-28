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
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="optionsFactory">The tracker options for determining the selected tracker profiles</param>
    /// <param name="provider">The config provider for loading configs</param>
    /// <param name="logger"></param>
    public Configs(OptionsFactory optionsFactory, Smz3.Data.Configuration.ConfigProvider provider, ILogger<Configs> logger)
    {
        var options = optionsFactory.Create();
        var profiles = options.GeneralOptions.SelectedProfiles.NonNull().Where(x => x != "Default").ToArray();
        var moods = provider.GetAvailableMoods(profiles);
        CurrentMood = moods.Random(Random.Shared);
        logger.LogInformation("Tracker is feeling {Mood} today", CurrentMood);

        Bosses = provider.GetBossConfig(profiles, CurrentMood);
        Dungeons = provider.GetDungeonConfig(profiles, CurrentMood);
        Items = provider.GetItemConfig(profiles, CurrentMood);
        Locations = provider.GetLocationConfig(profiles, CurrentMood);
        Regions = provider.GetRegionConfig(profiles, CurrentMood);
        Requests = provider.GetRequestConfig(profiles, CurrentMood);
        Responses = provider.GetResponseConfig(profiles, CurrentMood);
        Rooms = provider.GetRoomConfig(profiles, CurrentMood);
        Rewards = provider.GetRewardConfig(profiles, CurrentMood);
        UILayouts = provider.GetUIConfig(profiles, CurrentMood);
        GameLines = provider.GetGameConfig(profiles, CurrentMood);
        MsuConfig = provider.GetMsuConfig(profiles, CurrentMood);
        HintTileConfig = provider.GetHintTileConfig(profiles, CurrentMood);
    }

    /// <summary>
    /// Gets the current mood.
    /// </summary>
    public string? CurrentMood { get; }

    /// <summary>
    /// Gets a collection of trackable items.
    /// </summary>
    public ItemConfig Items { get; }

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
    public ResponseConfig Responses { get; }

    /// <summary>
    /// Gets a collection of basic requests and responses.
    /// </summary>
    public RequestConfig Requests { get; }

    /// <summary>
    /// Gets a collection of extra information about regions.
    /// </summary>
    public RegionConfig Regions { get; }

    /// <summary>
    /// Gets a collection of extra information about dungeons.
    /// </summary>
    public DungeonConfig Dungeons { get; }

    /// <summary>
    /// Gets a collection of bosses.
    /// </summary>
    public BossConfig Bosses { get; }

    /// <summary>
    /// Gets a collection of extra information about rooms.
    /// </summary>
    public RoomConfig Rooms { get; }

    /// <summary>
    /// Gets a collection of extra information about locations.
    /// </summary>
    public LocationConfig Locations { get; }

    /// <summary>
    /// Gets a collection of extra information about rewards
    /// </summary>
    public RewardConfig Rewards { get; }

    /// <summary>
    /// Gets a collection of available UI layouts
    /// </summary>
    public UIConfig UILayouts { get; }

    /// <summary>
    /// Gets the in game lines
    /// </summary>
    public GameLinesConfig GameLines { get; }

    /// <summary>
    /// Gets the msu config
    /// </summary>
    public MsuConfig MsuConfig { get; }

    /// <summary>
    /// Gets the hint tile config
    /// </summary>
    public HintTileConfig HintTileConfig { get; }
}
