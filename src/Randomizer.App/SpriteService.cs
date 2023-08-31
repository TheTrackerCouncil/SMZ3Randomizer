using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Randomizer.App.Patches;
using Randomizer.Data;
using Randomizer.Data.Options;
using Randomizer.Shared;
using Randomizer.SMZ3.FileData;

namespace Randomizer.App;

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

    public IEnumerable<Sprite> Sprites { get; set; } = new List<Sprite>();
    public IEnumerable<Sprite> LinkSprites => Sprites.Where(x => x.SpriteType == SpriteType.Link);
    public IEnumerable<Sprite> SamusSprites => Sprites.Where(x => x.SpriteType == SpriteType.Samus);
    public IEnumerable<Sprite> ShipSprites => Sprites.Where(x => x.SpriteType == SpriteType.Ship);

    /// <summary>
    /// Loads all player and ship sprites
    /// </summary>
    public Task LoadSprites()
    {
        if (Sprites.Any()) return Task.CompletedTask;

        return Task.Run(() =>
        {
            var defaults = new List<Sprite>() { Sprite.DefaultSamus, Sprite.DefaultLink, Sprite.DefaultShip, Sprite.RandomSamus, Sprite.RandomLink, Sprite.RandomShip };

            var playerSprites = Directory.EnumerateFiles(SpritePath, "*.rdc", SearchOption.AllDirectories)
                .Select(LoadRdcSprite);

            var shipSprites = Directory.EnumerateFiles(Path.Combine(SpritePath, "Ships"), "*.ips", SearchOption.AllDirectories)
                .Select(LoadIpsSprite);

            var sprites = playerSprites.Concat(shipSprites).Concat(defaults).OrderBy(x => x.Name).ToList();

            var extraSpriteDirectory = Environment.ExpandEnvironmentVariables("%LocalAppData%\\SMZ3CasRandomizer\\Sprites");

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

    /// <summary>
    /// Retrieves the random sprite image for the given sprite type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string GetRandomPreviewImage(SpriteType type)
    {
        var spriteFolder = type == SpriteType.Ship ? "Ships" : type.ToString();
        return Path.Combine(SpritePath, spriteFolder, "random.png");
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

        _options.GeneralOptions.LinkSpriteOptions.TryGetValue(path, out var spriteOption);

        return new Sprite(name, author, path, SpriteType.Ship, previewPath, spriteOption);
    }

    /// <summary>
    /// Applys the user selected sprite options to the given rom bytes
    /// </summary>
    /// <param name="sprite">The sprite to apply</param>
    /// <param name="bytes">The rom bytes to apply the sprite to</param>
    /// <returns>The actual selected sprite object</returns>
    public Sprite ApplySpriteOptionsTo(Sprite sprite, byte[] bytes)
    {
        if (sprite.IsDefault) return sprite;

        if (sprite.IsRandomSprite)
        {
            if (!Sprites.Any())
            {
                LoadSprites().Wait();
            }

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
                x.SpriteType == spriteType && x.MatchesFilter(searchText, filter) && !x.IsRandomSprite);

            // Try not to pick the same sprite twice in a row
            if (previousSprite != null && randomSpriteOptions.Count() > 1)
            {
                randomSpriteOptions = randomSpriteOptions.Where(x => x != previousSprite);
            }

            sprite = randomSpriteOptions.Random(random)
                     ?? Sprites.First(x => x.SpriteType == spriteType && x.IsDefault);

            if (sprite.IsDefault)
            {
                return sprite;
            }
        }

        if (sprite.SpriteType == SpriteType.Ship)
        {
            ApplyShipSpriteTo(sprite, bytes);
        }
        else if (sprite.SpriteType is SpriteType.Link or SpriteType.Samus)
        {
            ApplyRdcSpriteTo(sprite, bytes);
        }

        return sprite;
    }

    private void ApplyShipSpriteTo(Sprite sprite, byte[] bytes)
    {
        var shipPatchFileName = sprite.FilePath;
        if (File.Exists(shipPatchFileName))
        {
            using var customShipBasePatch = IpsPatch.CustomShip();
            Rom.ApplySuperMetroidIps(bytes, customShipBasePatch);

            using var shipPatch = File.OpenRead(shipPatchFileName);
            Rom.ApplySuperMetroidIps(bytes, shipPatch);
        }
    }

    private void ApplyRdcSpriteTo(Sprite sprite, byte[] bytes)
    {
        using var stream = File.OpenRead(sprite.FilePath);
        var rdc = Rdc.Parse(stream);

        if (rdc.TryParse<LinkSprite>(stream, out var linkSprite))
            linkSprite?.Apply(bytes);

        if (rdc.TryParse<SamusSprite>(stream, out var samusSprite))
            samusSprite?.Apply(bytes);
    }

    private string SpritePath => Path.Combine(
        Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? "")!,
        "Sprites");
}
