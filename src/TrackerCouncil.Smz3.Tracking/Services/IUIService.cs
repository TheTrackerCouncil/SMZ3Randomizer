using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.Services;

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
    /// <param name="hasTreasure">The dungeon requested</param>
    /// <returns>The full path of the sprite or null if it's not found</returns>
    public string? GetSpritePath(IHasTreasure hasTreasure);

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

    /// <summary>
    /// Gets the images for tracker talking
    /// </summary>
    /// <param name="profilePath">The selected profile</param>
    /// <param name="basePath">The base path of the folder used</param>
    /// <returns>A dictionary of all of the available tracker speech images</returns>
    public Dictionary<string, TrackerSpeechImages> GetTrackerSpeechSprites(out string? profilePath, string? basePath = null);
}
