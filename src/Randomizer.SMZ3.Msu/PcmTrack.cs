using System;
using System.ComponentModel;

namespace Randomizer.SMZ3.Msu
{
    public class PcmTrack
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PcmTrack()
        {
            FileName = null!;
        }

        public PcmTrack(string fileName)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public int TrackNumber { get; init; }

        public string? Title { get; init; }

        public string FileName { get; init; }

        public bool IsDefault { get; init; }

        public override string ToString() => $"Track {TrackNumber}: {Title}";
    }
}
