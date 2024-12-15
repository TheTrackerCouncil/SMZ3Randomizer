using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Logic;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Options;

/// <summary>
/// Represents the configuration for a plandomizer world.
/// </summary>
public class PlandoConfig // TODO: Consider using this instead of SeedData?
{
    /// <summary>
    /// Initializes a new empty instance of the <see cref="PlandoConfig"/>
    /// class.
    /// </summary>
    public PlandoConfig()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlandoConfig"/> based
    /// on the specified world.
    /// </summary>
    /// <param name="world">The world instance to export to a config.</param>
    public PlandoConfig(World world)
    {
        Seed = world.Config.Seed;
        KeysanityMode = world.Config.KeysanityMode;
        GanonsTowerCrystalCount = world.Config.GanonCrystalCount;
        GanonCrystalCount = world.Config.GanonCrystalCount;
        OpenPyramid = world.Config.OpenPyramid;
        TourianBossCount = world.Config.TourianBossCount;
        Items = world.Locations
            .ToDictionary(x => x.ToString(), x => x.Item.Type);
        Rewards = world.Regions.Where(x => x is IHasReward)
            .ToDictionary(x => x.ToString(), x => ((IHasReward)x).RewardType);
        Medallions = world.Regions.Where(x => x is IHasPrerequisite)
            .ToDictionary(x => x.ToString(), x => ((IHasPrerequisite)x).RequiredItem);
        Logic = world.Config.LogicConfig.Clone();
        StartingInventory = world.Config.ItemOptions;
        var prizes = DropPrizes.GetPool(world.Config.CasPatches.ZeldaDrops).ToList();
        ZeldaPrizes.EnemyDrops = prizes.Take(56).ToList();
        ZeldaPrizes.TreePulls = prizes.Skip(56).Take(3).ToList();
        ZeldaPrizes.CrabBaseDrop = prizes.Skip(59).First();
        ZeldaPrizes.CrabEightDrop = prizes.Skip(60).First();
        ZeldaPrizes.StunPrize = prizes.Skip(61).First();
        ZeldaPrizes.FishPrize = prizes.Skip(62).First();

        var bottleItems = Enum.GetValues<ItemType>().Where(x => x.IsInCategory(ItemCategory.Bottle))
            .Shuffle(new Random().Sanitize()).Take(2)
            .ToList();
        WaterfallFairyTrade = bottleItems.First();
        PyramidFairyTrade = bottleItems.Last();
    }

    /// <summary>
    /// Gets or sets the name of the file from which the plando config was
    /// deserialized.
    /// </summary>
    [YamlIgnore]
    public string FileName { get; set; } = "";

    public string Seed { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether Keysanity should be enabled.
    /// </summary>
    public KeysanityMode KeysanityMode { get; set; }

    /// <summary>
    /// Gets or sets the number of crystals for entering GT
    /// </summary>
    public int GanonsTowerCrystalCount { get; set; } = 7;

    /// <summary>
    /// Gets or sets the number of crystals needed to hurt Ganon
    /// </summary>
    public int GanonCrystalCount { get; set; } = 7;

    /// <summary>
    /// Gets or sets if the pyramid should be open by default
    /// </summary>
    public bool OpenPyramid { get; set; } = false;

    /// <summary>
    /// Gets or sets the number of SM Golden Bosses need to be defeated to enter Tourian
    /// </summary>
    public int TourianBossCount { get; set; } = 4;

    /// <summary>
    /// Gets or sets the logic options that apply to the plando.
    /// </summary>
    public LogicConfig Logic { get; set; } = new();

    /// <summary>
    /// Gets or sets a dictionary that contains the names of locations and
    /// the types of items they should be filled with.
    /// </summary>
    public Dictionary<string, ItemType> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets a dictionary that contains the names of regions and the
    /// boss rewards they should be filled with.
    /// </summary>
    public Dictionary<string, RewardType> Rewards { get; set; } = new();

    /// <summary>
    /// Gets or sets a dictionary that contains the names of regions and the
    /// medallions they require.
    /// </summary>
    public Dictionary<string, ItemType> Medallions { get; set; } = new();

    /// <summary>
    /// Text overrides
    /// </summary>
    public PlandoTextConfig Text { get; set; } = new();

    /// <summary>
    /// Various Zelda enemy drops and other prizes
    /// </summary>
    public PlandoZeldaPrizeConfig ZeldaPrizes { get; set; } = new();

    /// <summary>
    /// Bottle trade offer with the waterfall fairy
    /// </summary>
    public ItemType? WaterfallFairyTrade { get; set; }

    /// <summary>
    /// Bottle trade offer with the pyramid fairy
    /// </summary>
    public ItemType? PyramidFairyTrade { get; set; }

    /// <summary>
    /// Item Options for the starting inventory
    /// </summary>
    public IDictionary<string, int> StartingInventory { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Lines for tracker to say when tracking a location
    /// </summary>
    public IDictionary<string, string> TrackerLocationLines { get; set; } = new Dictionary<string, string>();
}
