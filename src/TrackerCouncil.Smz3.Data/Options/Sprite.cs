using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using YamlDotNet.Serialization;

namespace TrackerCouncil.Smz3.Data.Options;

public class Sprite : IEquatable<Sprite>
{
    public static HashSet<string> ValidDownloadExtensions = [".png", ".rdc", ".ips", ".gif"];

    private static readonly Dictionary<SpriteType, string> s_folderNames = new()
    {
        { SpriteType.Samus, "Samus" },
        { SpriteType.Link, "Link" },
        { SpriteType.Ship, "Ships" }
    };

    public static readonly Sprite DefaultSamus = new("Default Samus", SpriteType.Samus, true, false, "default.png");
    public static readonly Sprite DefaultLink = new("Default Link", SpriteType.Link, true, false, "default.png");
    public static readonly Sprite DefaultShip = new("Default Ship", SpriteType.Ship, true, false, "default.png");
    public static readonly Sprite RandomSamus = new("Random Sprite", SpriteType.Samus, false, true, "random.png");
    public static readonly Sprite RandomLink = new("Random Sprite", SpriteType.Link, false, true, "random.png");
    public static readonly Sprite RandomShip = new("Random Sprite", SpriteType.Ship, false, true, "random.png");

    public Sprite()
    {
        Name = "";
        Author = "";
        PreviewPath = "";
    }

    public Sprite(string name, string author, string filePath, SpriteType spriteType, string previewPath, SpriteOptions spriteOption)
    {
        Name = name;
        Author = author;
        FilePath = filePath;
        SpriteType = spriteType;
        PreviewPath = previewPath;
        SpriteOption = spriteOption;
        IsUserSprite = filePath.StartsWith(Directories.UserSpritePath);

        if (string.IsNullOrEmpty(filePath))
            IsDefault = true;
    }

    private Sprite(string name, SpriteType spriteType, bool isDefault, bool isRandomSprite, string sprite)
    {
        Name = name;
        Author = isDefault ? "Nintendo" : "";
        SpriteType = spriteType;
        IsDefault = isDefault;
        IsRandomSprite = isRandomSprite;
        PreviewPath = Path.Combine(Directories.SpritePath, s_folderNames[spriteType], sprite);
    }

    [YamlIgnore]
    public string Name { get; set; }

    [YamlIgnore]
    public string Author { get; set; }

    public string FilePath { get; set; } = "";

    public SpriteType SpriteType { get; set; }

    [YamlIgnore]
    public string PreviewPath { get; set; }

    public bool IsDefault { get; set; }

    public bool IsRandomSprite { get; set; }

    [YamlIgnore]
    public SpriteOptions SpriteOption { get; set; }

    public bool IsUserSprite { get; set; }

    public bool MatchesFilter(string searchTerm, SpriteFilter spriteFilter) => (string.IsNullOrEmpty(searchTerm) ||
                                                                                   Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                                                   Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
                                                                               ((spriteFilter == SpriteFilter.Default && SpriteOption != SpriteOptions.Hide) ||
                                                                                   (spriteFilter == SpriteFilter.Favorited && SpriteOption == SpriteOptions.Favorite) ||
                                                                                   (spriteFilter == SpriteFilter.Hidden && SpriteOption == SpriteOptions.Hide) ||
                                                                                   (spriteFilter == SpriteFilter.User && IsUserSprite) ||
                                                                                   spriteFilter == SpriteFilter.All);

    public static bool operator ==(Sprite? a, Sprite? b)
        => a is null && b is null || a?.Equals(b) == true;

    public static bool operator !=(Sprite? a, Sprite? b)
        => !(a == b);

    public override bool Equals(object? obj)
    {
        if (obj is not Sprite other)
            return false;

        return Equals(other);
    }

    public bool Equals(Sprite? other)
    {
        return FilePath == other?.FilePath
               && SpriteType == other.SpriteType
               && IsDefault == other.IsDefault
               && IsRandomSprite == other.IsRandomSprite;
    }

    public override int GetHashCode()
        => HashCode.Combine(FilePath, SpriteType);

    public override string ToString()
    {
        if (IsDefault)
        {
            return $"Default";
        }
        else if (IsRandomSprite)
        {
            return $"Random {SpriteType} Sprite";
        }
        return string.IsNullOrEmpty(Author) ? Name : $"{Name} by {Author}";
    }

}
