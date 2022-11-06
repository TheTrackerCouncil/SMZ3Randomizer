using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Shared;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Service for getting layouts and images for displaying in the UI
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// Retrieves a list of layouts that can be selected by the user
        /// </summary>
        public List<UILayout> SelectableLayouts { get; }

        /// <summary>
        /// Retrieves a layout by name
        /// </summary>
        /// <param name="name">The name of the requested</param>
        /// <returns>The matching layout or the first one if it is not found</returns>
        public UILayout GetLayout(string? name);

        /// <summary>
        /// Returns the path of the sprite for the number
        /// </summary>
        /// <param name="digit">The number requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(int digit);

        /// <summary>
        /// Returns the path of the sprite for the item
        /// </summary>
        /// <param name="item">The item requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(Item item);

        /// <summary>
        /// Returns the path of the sprite for the boss
        /// </summary>
        /// <param name="boss">The boss requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(BossInfo boss);

        /// <summary>
        /// Returns the path of the sprite for the dungeon
        /// </summary>
        /// <param name="dungeon">The dungeon requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(IDungeon dungeon);

        /// <summary>
        /// Returns the path of the sprite for the reward
        /// </summary>
        /// <param name="reward">The reward requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(Reward reward);

        /// <summary>
        /// Returns the path of the sprite for the reward
        /// </summary>
        /// <param name="reward">The reward requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(RewardType reward);

        /// <summary>
        /// Returns the path of the sprite
        /// </summary>
        /// <param name="category">The category of sprite</param>
        /// <param name="imageFileName">The individual filename of the sprite</param>
        /// <param name="profilePath">The path of the selected profile</param>
        /// <param name="basePath">The base path of the desired sprite</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(string category, string imageFileName, out string? profilePath, string? basePath = null);
    }
}
