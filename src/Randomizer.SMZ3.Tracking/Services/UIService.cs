using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Randomizer.Shared;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Data.Options;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Service for getting layouts and images for displaying in the UI
    /// </summary>
    public class UIService: IUIService
    {
        private readonly UIConfig _layouts;
        private readonly List<string> _iconPaths;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">The tracker options</param>
        /// <param name="configProvider">The tracker configs</param>
        /// <param name="uiConfig">The UI configs</param>
        public UIService(TrackerOptionsAccessor options,
            ConfigProvider configProvider,
            UIConfig uiConfig
        )
        {
            _layouts = uiConfig;

            var iconPaths = options.Options?.TrackerProfiles
                .Where(x => !string.IsNullOrEmpty(x))
                .NonNull()
                .Select(x => Path.Combine(configProvider.ConfigDirectory, x)).Reverse().ToList() ?? new();
            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (basePath != null)
            {
                iconPaths.Add(basePath);
            }
            _iconPaths = iconPaths;
        }

        /// <summary>
        /// Retrieves a list of layouts that can be selected by the user
        /// </summary>
        public List<UILayout> SelectableLayouts => _layouts.Where(x => x.Name != "Peg World").ToList();

        /// <summary>
        /// Retrieves a layout by name
        /// </summary>
        /// <param name="name">The name of the requested</param>
        /// <returns>The matching layout or the first one if it is not found</returns>
        public UILayout GetLayout(string? name) => _layouts.FirstOrDefault(x => x.Name == name) ?? _layouts[0];

        /// <summary>
        /// Returns the path of the sprite for the number
        /// </summary>
        /// <param name="digit">The number requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(int digit) => GetSpritePath("Marks", $"{digit % 10}.png", out _);

        /// <summary>
        /// Returns the path of the sprite for the item
        /// </summary>
        /// <param name="item">The item requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(Item item)
        {
            string? fileName;

            if (item.Metadata.Image != null)
            {
                fileName = GetSpritePath("Items", item.Metadata.Image, out _);
                if (File.Exists(fileName))
                    return fileName;
            }

            if (item.Metadata.HasStages || item.Metadata.Multiple)
            {
                var baseFileName = GetSpritePath("Items", $"{item.Metadata.Item.ToLowerInvariant()}.png", out var profilePath);
                fileName = GetSpritePath("Items", $"{item.Metadata.Item.ToLowerInvariant()} ({item.State.TrackingState}).png", out _, profilePath);
                if (File.Exists(fileName))
                    return fileName;
                else
                    return baseFileName;
            }

            return GetSpritePath("Items", $"{item.Metadata.Item.ToLowerInvariant()}.png", out _);
        }

        /// <summary>
        /// Returns the path of the sprite for the boss
        /// </summary>
        /// <param name="boss">The boss requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(BossInfo boss)
        {
            string? fileName;

            if (boss.Image != null)
            {
                fileName = GetSpritePath("Items", boss.Image, out _);
                if (File.Exists(fileName))
                    return fileName;
            }

            return GetSpritePath("Items", $"{boss.Boss.ToLowerInvariant()}.png", out _);
        }

        /// <summary>
        /// Returns the path of the sprite for the dungeon
        /// </summary>
        /// <param name="dungeon">The dungeon requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(IDungeon dungeon) => GetSpritePath("Dungeons",
            $"{dungeon.DungeonName.ToLowerInvariant()}.png", out _);

        /// <summary>
        /// Returns the path of the sprite for the reward
        /// </summary>
        /// <param name="reward">The reward requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(Reward reward) => GetSpritePath(reward.Type);

        /// <summary>
        /// Returns the path of the sprite for the reward
        /// </summary>
        /// <param name="reward">The reward requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(RewardType reward) => GetSpritePath("Dungeons",
            $"{reward.GetDescription().ToLowerInvariant()}.png", out _);

        /// <summary>
        /// Returns the path of the sprite
        /// </summary>
        /// <param name="category">The category of sprite</param>
        /// <param name="imageFileName">The individual filename of the sprite</param>
        /// <param name="profilePath">The path of the selected profile</param>
        /// <param name="basePath">The base path of the desired sprite</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(string category, string imageFileName, out string? profilePath, string? basePath = null)
        {
            if (!string.IsNullOrEmpty(basePath))
            {
                var path = Path.Combine(basePath, "Sprites", category, imageFileName);
                if (File.Exists(path))
                {
                    profilePath = basePath;
                    return path;
                }
            }
            else
            {
                foreach (var profile in _iconPaths)
                {
                    var path = Path.Combine(profile, "Sprites", category, imageFileName);
                    if (File.Exists(path))
                    {
                        profilePath = profile;
                        return path;
                    }
                }
            }
            profilePath = null;
            return null;
        }
    }
}
