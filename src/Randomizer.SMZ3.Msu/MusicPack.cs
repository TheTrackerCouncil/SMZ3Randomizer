using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Randomizer.SMZ3.Msu
{
    public class MusicPack
    {
        public MusicPack()
        {
            Tracks = new Dictionary<int, PcmTrackSet>();
        }

        public MusicPack(string? title, string? author, IDictionary<int, PcmTrackSet> tracks)
        {
            Title = title;
            Author = author;
            Tracks = tracks ?? throw new ArgumentNullException(nameof(tracks));
        }

        public string? Title { get; set; }

        public string? Author { get; set; }

        public IDictionary<int, PcmTrackSet> Tracks { get; set; }

        public PcmTrackSet? this[int trackNumber]
            => Tracks.TryGetValue(trackNumber, out var trackSet)
                ? trackSet
                : null;

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
