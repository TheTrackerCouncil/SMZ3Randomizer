using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;
using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Represents additional information about rewards
/// </summary>
public class RewardInfo : IMergeable<RewardInfo>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RewardInfo() { }

    /// <summary>
    /// Constructor
    /// </summary>
    public RewardInfo(RewardType type)
    {
        Reward = type.GetDescription();
        Name = new SchrodingersString(Reward);
        RewardType = type;
    }

    /// <summary>
    /// Unique key to connect the RewardInfo with other configs
    /// </summary>
    [MergeKey]
    public string Reward { get; set; } = "";

    /// <summary>
    /// Gets the possible names for the reward.
    /// </summary>
    public SchrodingersString? Name { get; set; }

    /// <summary>
    /// Gets the possible names for the reward with an article before it
    /// </summary>
    public SchrodingersString? ArticledName { get; set; }

    /// <summary>
    /// Gets the grammatical article for the item (e.g. "a" or "the").
    /// </summary>
    public string? Article { get; set; }

    /// <summary>
    /// The SMZ3 reward type
    /// </summary>
    [JsonIgnore, YamlIgnore]
    public RewardType RewardType { get; set; }

    /// <summary>
    /// Gets the name of the article, prefixed with "a", "the" or none,
    /// depending on the reward.
    /// </summary>
    [JsonIgnore, YamlIgnore]
    public string NameWithArticle => ArticledName?.Any() == true
        ? ArticledName.ToString() ?? DefaultNameWithArticle
        : DefaultNameWithArticle;

    [JsonIgnore, YamlIgnore]
    private string DefaultNameWithArticle => string.Join(" ",
        Article ?? "", Name);
}
