using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Msu
{
    public class PcmTrack
    {
        public PcmTrack(int trackNumber, string? title, string fileName)
        {
            TrackNumber = trackNumber;
            Title = title;
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public int TrackNumber { get; }

        public string? Title { get; }

        public string FileName { get; }

        public override string ToString() => $"Track {TrackNumber} {Title}";
    }
}
