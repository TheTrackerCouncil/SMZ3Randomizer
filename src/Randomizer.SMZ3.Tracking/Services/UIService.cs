using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Shared;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Configuration.ConfigFiles;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Services
{
    /// <summary>
    /// Service for getting layouts and images for displaying in the UI
    /// </summary>
    public class UIService: IUIService
    {
        private readonly UIConfig _layouts;
        private readonly TrackerOptionsAccessor _options;
        private readonly TrackerConfigProvider _configProvider;
        private readonly List<string> IconPaths;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">The tracker options</param>
        /// <param name="configProvider">The tracker configs</param>
        /// <param name="uiConfig">The UI configs</param>
        public UIService(TrackerOptionsAccessor options,
            TrackerConfigProvider configProvider,
            UIConfig uiConfig
        )
        {
            _layouts = uiConfig;
            _options = options;
            _configProvider = configProvider;

            var iconPaths = _options.Options.TrackerProfiles
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => Path.Combine(_configProvider.ConfigDirectory, x)).Reverse().ToList();
            iconPaths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            IconPaths = iconPaths;
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
        public UILayout GetLayout(string name) => _layouts.FirstOrDefault(x => x.Name == name) ?? _layouts[0];

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
        public string? GetSpritePath(ItemData item)
        {
            var fileName = (string?)null;

            if (item.Image != null)
            {
                fileName = GetSpritePath("Items", item.Image, out _);
                if (File.Exists(fileName))
                    return fileName;
            }

            if (item.HasStages || item.Multiple)
            {
                var baseFileName = GetSpritePath("Items", $"{item.Item.ToLowerInvariant()}.png", out string profilePath);
                fileName = GetSpritePath("Items", $"{item.Item.ToLowerInvariant()} ({item.TrackingState}).png", out _, profilePath);
                if (File.Exists(fileName))
                    return fileName;
                else
                    return baseFileName;
            }

            return GetSpritePath("Items", $"{item.Item.ToLowerInvariant()}.png", out _);
        }

        /// <summary>
        /// Returns the path of the sprite for the boss
        /// </summary>
        /// <param name="boss">The boss requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(BossInfo boss)
        {
            var fileName = (string?)null;

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
        public string? GetSpritePath(DungeonInfo dungeon) => GetSpritePath("Dungeons",
            $"{dungeon.Dungeon.ToLowerInvariant()}.png", out _);

        /// <summary>
        /// Returns the path of the sprite for the reward
        /// </summary>
        /// <param name="reward">The reward requested</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(RewardItem reward) => GetSpritePath("Dungeons",
            $"{reward.GetDescription().ToLowerInvariant()}.png", out _);

        /// <summary>
        /// Returns the path of the sprite
        /// </summary>
        /// <param name="category">The category of sprite</param>
        /// <param name="imageFileName">The individual filename of the sprite</param>
        /// <param name="profilePath">The path of the selected profile</param>
        /// <param name="basePath">The base path of the desired sprite</param>
        /// <returns>The full path of the sprite or null if it's not found</returns>
        public string? GetSpritePath(string category, string imageFileName, out string profilePath, string basePath = null)
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
                foreach (var profile in IconPaths)
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
