using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

        public static readonly Sprite DefaultSamus = new("Default Samus", SpriteType.Samus);
        public static readonly Sprite DefaultLink = new("Default Link", SpriteType.Link);
        public static readonly Sprite DefaultShip = new("Default Ship", SpriteType.Ship);

        public Sprite(string name, string author, string? filePath, SpriteType spriteType, string previewPath)
        {
            Name = name;
            Author = author;
            FilePath = filePath ?? "";
            SpriteType = spriteType;
            PreviewPath = previewPath;
            if (string.IsNullOrEmpty(filePath))
                IsDefault = true;
        }

        private Sprite(string name, SpriteType spriteType)
        {
            Name = name;
            Author = "Nintendo";
            SpriteType = spriteType;
            IsDefault = true;
            PreviewPath = Path.Combine(
                Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? "")!,
                "Sprites", s_folderNames[spriteType], "default.png");
        }

        public string Name { get; }

        public string Author { get; }

        public string FilePath { get; } = "";

        public SpriteType SpriteType { get; }

        public string PreviewPath { get; }

        public bool IsDefault { get; }

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
                && SpriteType == other.SpriteType;
        }

        public override int GetHashCode()
            => HashCode.Combine(FilePath, SpriteType);
    }
}
