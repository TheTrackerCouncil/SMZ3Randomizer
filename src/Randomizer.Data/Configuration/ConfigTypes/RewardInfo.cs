using Randomizer.Shared;

namespace Randomizer.Data.Configuration.ConfigTypes
{
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
        public SchrodingersString Name { get; set; } = new();

        /// <summary>
        /// Gets the grammatical article for the item (e.g. "a" or "the").
        /// </summary>
        public string? Article { get; set; }

        /// <summary>
        /// The SMZ3 reward type
        /// </summary>
        public RewardType RewardType { get; set; }

        /// <summary>
        /// Gets the name of the article, prefixed with "a", "the" or none,
        /// depending on the reward.
        /// </summary>
        public string NameWithArticle => string.Join(" ",
            Article ?? "", Name);
    }
}
