using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Msu
{
    public class PcmTrackSet
    {
        public PcmTrackSet(int trackNumber, IEnumerable<PcmTrack> tracks)
        {
            TrackNumber = trackNumber;
            Tracks = new List<PcmTrack>(tracks);
        }

        public int TrackNumber { get; }

        public PcmTrack Track => Tracks.First();

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public ICollection<PcmTrack> Tracks { get; }

        public ICollection<PcmTrack> Alternatives => Tracks.Skip(1).ToList();

        public override string ToString()
        {
            if (Tracks.Count > 1)
            {
                return $"Track {TrackNumber} (+{Alternatives.Count})";
            }

            return $"Track {TrackNumber}";
        }
    }
}
