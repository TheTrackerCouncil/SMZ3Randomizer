using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Config file for additional reward information
/// </summary>
[Description("Config file for dungeon reward names")]
public class RewardConfig : List<RewardInfo>, IMergeable<RewardInfo>, IConfigFile<RewardConfig>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RewardConfig()
    {
    }

    /// <summary>
    /// Returns default reward information
    /// </summary>
    /// <returns></returns>
    public static RewardConfig Default()
    {
        return new RewardConfig
        {
            new()
            {
                Reward = "Unknown",
                Article = null,
                RewardType = RewardType.None,
            },
            new()
            {
                Reward = "Crystal",
                Article = "a",
                RewardType = RewardType.CrystalBlue,
            },
            new()
            {
                Reward = "Red Crystal",
                Article = "a",
                RewardType = RewardType.CrystalRed,
            },
            new()
            {
                Reward = "Green Pendant",
                Article = "the",
                RewardType = RewardType.PendantGreen,
            },
            new()
            {
                Reward = "Red Pendant",
                Article = "the",
                RewardType = RewardType.PendantRed,
            },
            new()
            {
                Reward = "Blue Pendant",
                Article = "the",
                RewardType = RewardType.PendantBlue,
            },
            new()
            {
                Reward = "Agahnim",
                Article = null,
                RewardType = RewardType.Agahnim,
            },
        };
    }

    public static object Example()
    {
        return new RewardConfig
        {
            new()
            {
                Reward = "Red Crystal",
                Name = new("Red Crystal", new SchrodingersString.Possibility("Blood red crystal", 0.1)),
                ArticledName = new("a Red Crystal", new SchrodingersString.Possibility("a blood red crystal", 0.1)),
            }
        };
    }
}
