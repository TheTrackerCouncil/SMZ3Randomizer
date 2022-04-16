using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

using YamlDotNet.Serialization;

namespace Randomizer.SMZ3.Msu
{
    public class MusicPack
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method should only be called by deserializers, never directly.", error: true)]
        public MusicPack()
        {
            MsuFileName = null!;
            Tracks = new List<PcmTrack>();
        }

        public MusicPack(string msuFileName, string? title, string? author, IEnumerable<PcmTrack> tracks)
        {
            MsuFileName = msuFileName;
            Title = title;
            Author = author;
            Tracks = new List<PcmTrack>(tracks);
        }

        public string? Title { get; set; }

        public string? Author { get; set; }

        [YamlIgnore]
        public string? FileName { get; set; }

        public string MsuFileName { get; init; }

        public MsuGame Game { get; set; }

        public IList<PcmTrack> Tracks { get; init; }

        public PcmTrack? GetTrack(int trackNumber)
        {
            var tracks = GetTracks(trackNumber);
            return tracks.FirstOrDefault(x => x.IsDefault)
                ?? tracks.FirstOrDefault();
        }

        public virtual IEnumerable<PcmTrack> GetTracks(int trackNumber)
            => Tracks.Where(x => x.TrackNumber == trackNumber);

        public PcmTrack? this[int trackNumber]
            => GetTrack(trackNumber);

        public override string? ToString()
        {
            if (!string.IsNullOrEmpty(Author))
            {
                return $"{Title} by {Author}";
            }

            return Title;
        }
    }
}
