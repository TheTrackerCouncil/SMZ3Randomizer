using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Randomizer.SMZ3.Msu
{
    public class MusicPack
    {
        public MusicPack(string? title, string? author, IEnumerable<PcmTrack> tracks)
        {
            Title = title;
            Author = author;
            Tracks = new List<PcmTrack>(tracks);
        }

        public string? Title { get; set; }

        public string? Author { get; set; }

        public MsuGame Game { get; set; }

        public IList<PcmTrack> Tracks { get; }


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
