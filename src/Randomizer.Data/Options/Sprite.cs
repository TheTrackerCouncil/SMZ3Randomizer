using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Randomizer.Data.Options
{
    public class Sprite : IEquatable<Sprite>
    {
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

        public Sprite(string name, string author, string filePath, SpriteType spriteType, string previewPath, SpriteOptions spriteOption)
        {
            Name = name;
            Author = author;
            FilePath = filePath;
            SpriteType = spriteType;
            PreviewPath = previewPath;
            SpriteOption = spriteOption;
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
            PreviewPath = Path.Combine(SpritePath, s_folderNames[spriteType], sprite);
        }

        public string Name { get; }

        public string Author { get; }

        public string FilePath { get; } = "";

        public SpriteType SpriteType { get; }

        public string PreviewPath { get; }

        public bool IsDefault { get; set; }

        public bool IsRandomSprite { get; set; }

        public SpriteOptions SpriteOption { get; set; }

        public bool MatchesFilter(string searchTerm, SpriteFilter spriteFilter) => (string.IsNullOrEmpty(searchTerm) ||
                Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) &&
            ((spriteFilter == SpriteFilter.Default && SpriteOption != SpriteOptions.Hide) ||
             (spriteFilter == SpriteFilter.Favorited && SpriteOption == SpriteOptions.Favorite) ||
             (spriteFilter == SpriteFilter.Hidden && SpriteOption == SpriteOptions.Hide) ||
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

        public static string SpritePath
        {
            get
            {
#if DEBUG
                var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

                while (directory != null && !directory.GetFiles("*.sln").Any())
                {
                    directory = directory.Parent;
                }

                return Path.Combine(directory!.FullName, "src", "Randomizer.Sprites");
#else

            return Path.Combine(
                Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? "")!,
                "Sprites");
#endif
            }

        }
    }
}
