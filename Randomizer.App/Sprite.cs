using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Randomizer.SMZ3.FileData;

namespace Randomizer.App
{
    public class Sprite
    {
        public static readonly Sprite DefaultSamus = new("Default", SpriteType.Samus);
        public static readonly Sprite DefaultLink = new("Default", SpriteType.Link);

        private readonly bool _isDefault = false;

        protected Sprite(string name, string author, string filePath, SpriteType spriteType)
        {
            Name = name;
            Author = author;
            FilePath = filePath;
            SpriteType = spriteType;
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

        public string Name { get; }

        public string Author { get; }

        public string FilePath { get; }

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
            if (rdc.TryParse<MetaDataBlock>(stream, out var block))
            {
                var title = block.Content.Value<string>("title");
                if (!string.IsNullOrEmpty(title))
                    name = title;
            }

            return new Sprite(name, rdc.Author, path, spriteType);
        }
    }
}
