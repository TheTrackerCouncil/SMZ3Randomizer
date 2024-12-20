using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.Data.Services;

/// <summary>
/// Service for collecting and applying player and ship sprites
/// </summary>
public class SpriteService
{
    private readonly ILogger<SpriteService> _logger;
    private readonly RandomizerOptions _options;

    public SpriteService(ILogger<SpriteService> logger, OptionsFactory optionsFactory)
    {
        _logger = logger;
        _options = optionsFactory.Create();
    }

    public List<Sprite> Sprites { get; set; } = [];
    public IEnumerable<Sprite> LinkSprites => Sprites.Where(x => x.SpriteType == SpriteType.Link);
    public IEnumerable<Sprite> SamusSprites => Sprites.Where(x => x.SpriteType == SpriteType.Samus);
    public IEnumerable<Sprite> ShipSprites => Sprites.Where(x => x.SpriteType == SpriteType.Ship);

    /// <summary>
    /// Loads all player and ship sprites
    /// </summary>
    public Task LoadSpritesAsync()
    {
        if (Sprites.Any() || !Directory.Exists(RandomizerDirectories.SpritePath)) return Task.CompletedTask;

        return Task.Run(() =>
        {
            var defaults = new List<Sprite>() { Sprite.DefaultSamus, Sprite.DefaultLink, Sprite.DefaultShip, Sprite.RandomSamus, Sprite.RandomLink, Sprite.RandomShip };

            var playerSprites = Directory.EnumerateFiles(RandomizerDirectories.SpritePath, "*.rdc", SearchOption.AllDirectories)
                .Select(LoadRdcSprite);

            var shipSprites = Directory.EnumerateFiles(Path.Combine(RandomizerDirectories.SpritePath, "Ships"), "*.ips", SearchOption.AllDirectories)
                .Select(LoadIpsSprite);

            var sprites = playerSprites.Concat(shipSprites).Concat(defaults).OrderBy(x => x.Name).ToList();

            var extraSpriteDirectory = RandomizerDirectories.UserSpritePath;

            if (Directory.Exists(extraSpriteDirectory))
            {
                sprites.AddRange(Directory.EnumerateFiles(extraSpriteDirectory, "*.rdc", SearchOption.AllDirectories)
                    .Select(LoadRdcSprite));
                sprites.AddRange(Directory.EnumerateFiles(extraSpriteDirectory, "*.ips", SearchOption.AllDirectories)
                    .Select(LoadIpsSprite));
            }

            Sprites = sprites;

            _logger.LogInformation("{LinkSprites} Link Sprites | {SamusSprites} Samus Sprites | {ShipCount} Ship Sprites", LinkSprites.Count(), SamusSprites.Count(), ShipSprites.Count());
        });
    }

    public Sprite? AddCustomSprite(string spritePath, string? previewImagePath)
    {
        var userSpritePath = RandomizerDirectories.UserSpritePath;

        if (spritePath.StartsWith(RandomizerDirectories.SpritePath) ||
            spritePath.StartsWith(userSpritePath) ||
            !File.Exists(spritePath))
        {
            return null;
        }

        var destinationSpritePath = Path.Combine(userSpritePath, Path.GetFileName(spritePath));
        File.Copy(spritePath, destinationSpritePath, true);
        if (File.Exists(previewImagePath))
        {
            var baseFileName = Path.GetFileNameWithoutExtension(spritePath);
            File.Copy(previewImagePath, Path.Combine(userSpritePath, $"{baseFileName}.png"), true);
        }

        var sprite = new Sprite();
        try
        {
            sprite = ".rdc".Equals(Path.GetExtension(destinationSpritePath), StringComparison.OrdinalIgnoreCase)
                ? LoadRdcSprite(destinationSpritePath)
                : LoadIpsSprite(destinationSpritePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Sprite {SpritePath}", destinationSpritePath);
        }

        if (Sprites.Any(x => x.FilePath == destinationSpritePath))
        {
            var oldSprite = Sprites.First(x => x.FilePath == destinationSpritePath);
            oldSprite.Name = sprite.Name;
            oldSprite.Author = sprite.Author;
            oldSprite.PreviewPath = sprite.PreviewPath;
            return oldSprite;
        }

        Sprites.Add(sprite);
        return sprite;
    }

    /// <summary>
    /// Retrieves the random sprite image for the given sprite type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string GetRandomPreviewImage(SpriteType type)
    {
        var spriteFolder = type == SpriteType.Ship ? "Ships" : type.ToString();
        return Path.Combine(RandomizerDirectories.SpritePath, spriteFolder, "random.png");
    }

    /// <summary>
    /// Loads a Link or Samus rdc sprite
    /// </summary>
    /// <param name="path">The path to the rdc file</param>
    /// <returns>The Sprite object</returns>
    private Sprite LoadRdcSprite(string path)
    {
        using var stream = File.OpenRead(path);
        var rdc = Rdc.Parse(stream);

        var spriteType = SpriteType.Unknown;
        var spriteOption = SpriteOptions.Default;
        if (rdc.Contains<LinkSprite>())
        {
            spriteType = SpriteType.Link;
            _options.GeneralOptions.LinkSpriteOptions.TryGetValue(path, out spriteOption);
        }
        else if (rdc.Contains<SamusSprite>())
        {
            spriteType = SpriteType.Samus;
            _options.GeneralOptions.SamusSpriteOptions.TryGetValue(path, out spriteOption);
        }

        var name = Path.GetFileName(path);
        var author = rdc.Author;
        if (rdc.TryParse<MetaDataBlock>(stream, out var block))
        {
            var title = block?.Content?.Value<string>("title");
            if (!string.IsNullOrEmpty(title))
                name = title;

            var author2 = block?.Content?.Value<string>("author");
            if (string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(author2))
                author = author2;
        }

        var file = new FileInfo(path);
        var previewPath = file.FullName.Replace(file.Extension, ".png");
        if (!File.Exists(previewPath))
        {
            previewPath = GetRandomPreviewImage(spriteType);
        }

        return new Sprite(name, author, path, spriteType, previewPath, spriteOption);
    }

    /// <summary>
    /// Loads a ship ips sprite
    /// </summary>
    /// <param name="path">The path to the ips file</param>
    /// <returns>The sprite object</returns>
    private Sprite LoadIpsSprite(string path)
    {
        var file = new FileInfo(path);
        var filename = Path.GetFileNameWithoutExtension(path);
        var name = filename;
        var author = "";

        if (name.Contains(" by "))
        {
            var parts = name.Split(" by ");
            name = parts.First();
            author = parts.Last();
        }

        var previewPath = file.FullName.Replace(file.Extension, ".png");
        if (!File.Exists(previewPath))
        {
            previewPath = GetRandomPreviewImage(SpriteType.Ship);
        }

        _options.GeneralOptions.ShipSpriteOptions.TryGetValue(path, out var spriteOption);

        return new Sprite(name, author, path, SpriteType.Ship, previewPath, spriteOption);
    }

    public Sprite GetSprite(SpriteType type)
    {
        var sprite = type switch
        {
            SpriteType.Link => _options.PatchOptions.SelectedLinkSprite,
            SpriteType.Samus => _options.PatchOptions.SelectedSamusSprite,
            _ => _options.PatchOptions.SelectedShipSprite
        };

        if (!Sprites.Any())
        {
            LoadSpritesAsync().Wait();
        }

        if (sprite.IsRandomSprite)
        {
            var spriteType = sprite.SpriteType;

            var searchText = spriteType switch
            {
                SpriteType.Link => _options.GeneralOptions.LinkSpriteSearchText,
                SpriteType.Samus => _options.GeneralOptions.SamusSpriteSearchText,
                _ => _options.GeneralOptions.ShipSpriteSearchText
            };

            var filter = spriteType switch
            {
                SpriteType.Link => _options.GeneralOptions.LinkSpriteFilter,
                SpriteType.Samus => _options.GeneralOptions.SamusSpriteFilter,
                _ => _options.GeneralOptions.ShipSpriteFilter
            };

            var previousSprite = spriteType switch
            {
                SpriteType.Link => _options.PatchOptions.PreviousLinkSprite,
                SpriteType.Samus => _options.PatchOptions.PreviousSamusSprite,
                _ => _options.PatchOptions.PreviousShipSprite
            };

            var random = new Random().Sanitize();

            var randomSpriteOptions = Sprites.Where(x =>
                x.SpriteType == spriteType && x.MatchesFilter(searchText, filter) && !x.IsRandomSprite).ToList();

            // Try not to pick the same sprite twice in a row
            if (previousSprite != null && randomSpriteOptions.Count() > 1)
            {
                randomSpriteOptions = randomSpriteOptions.Where(x => x != previousSprite).ToList();
            }

            sprite = randomSpriteOptions.Random(random)
                     ?? Sprites.First(x => x.SpriteType == spriteType && x.IsDefault);

            if (sprite.IsDefault)
            {
                return sprite;
            }
        }
        else if (!sprite.IsDefault)
        {
            var matchingSprite = Sprites.FirstOrDefault(x => x == sprite);
            sprite = matchingSprite ?? Sprites.First(x => x.IsDefault && x.SpriteType == type);
        }

        if (type == SpriteType.Link)
        {
            _options.PatchOptions.PreviousLinkSprite = sprite;
        }
        else if (type == SpriteType.Samus)
        {
            _options.PatchOptions.PreviousSamusSprite = sprite;
        }
        else if (type == SpriteType.Ship)
        {
            _options.PatchOptions.PreviousShipSprite = sprite;
        }

        return sprite;
    }

    /// <summary>
    /// Deletes a user added sprite
    /// </summary>
    /// <param name="sprite">The sprite to delete</param>
    public bool DeleteSprite(Sprite sprite)
    {
        if (!sprite.IsUserSprite)
        {
            return false;
        }

        try
        {
            if (!string.IsNullOrEmpty(sprite.PreviewPath) && File.Exists(sprite.PreviewPath))
            {
                File.Delete(sprite.PreviewPath);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete sprite preview file");
        }

        try
        {
            File.Delete(sprite.FilePath);
            Sprites.Remove(sprite);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete sprite preview file");
            return false;
        }
    }

}
