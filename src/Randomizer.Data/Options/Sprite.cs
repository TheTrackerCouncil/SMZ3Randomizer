using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Randomizer.Data.Options
{
    public class Sprite : IEquatable<Sprite>
    {
        public static readonly Sprite DefaultSamus = new("Default", SpriteType.Samus);
        public static readonly Sprite DefaultLink = new("Default", SpriteType.Link);

        private readonly bool _isDefault = false;

        public Sprite(string name, string author, string filePath, SpriteType spriteType)
        {
            Name = name;
            Author = author;
            FilePath = filePath;
            SpriteType = spriteType;
            if (FilePath == null)
                _isDefault = true;
        }

        private Sprite(string name, SpriteType spriteType)
        {
            Name = name;
            SpriteType = spriteType;
            _isDefault = true;
        }

        public string DisplayName
        {
            get
            {
                if (_isDefault)
                    return Name;
                if (string.IsNullOrEmpty(Author))
                    return $"{Name} (author unknown)";
                return $"{Name} (by {Author})";
            }
        }

        public string Name { get; } = "";

        public string Author { get; } = "";

        public string FilePath { get; } = "";

        public SpriteType SpriteType { get; }

        public static Sprite LoadSprite(string path)
        {
            using var stream = File.OpenRead(path);
            var rdc = Rdc.Parse(stream);

            var spriteType = SpriteType.Unknown;
            if (rdc.Contains<LinkSprite>())
                spriteType = SpriteType.Link;
            else if (rdc.Contains<SamusSprite>())
                spriteType = SpriteType.Samus;

            var name = Path.GetFileName(path);
            var author = rdc.Author;
            if (rdc.TryParse<MetaDataBlock>(stream, out var block))
            {
                var title = block.Content.Value<string>("title");
                if (!string.IsNullOrEmpty(title))
                    name = title;

                var author2 = block.Content.Value<string>("author");
                if (string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(author2))
                    author = author2;
            }

            return new Sprite(name, author, path, spriteType);
        }

        public void ApplyTo(byte[] rom)
        {
            if (_isDefault) return;

            using var stream = File.OpenRead(FilePath);
            var rdc = Rdc.Parse(stream);

            if (rdc.TryParse<LinkSprite>(stream, out var linkSprite))
                linkSprite.Apply(rom);

            if (rdc.TryParse<SamusSprite>(stream, out var samusSprite))
                samusSprite.Apply(rom);
        }

        public static bool operator ==(Sprite a, Sprite b)
            => a is null && b is null || a?.Equals(b) == true;

        public static bool operator !=(Sprite a, Sprite b)
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
                && SpriteType == other?.SpriteType;
        }

        public override int GetHashCode()
            => HashCode.Combine(FilePath, SpriteType);
    }
}
