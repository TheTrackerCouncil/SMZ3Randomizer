using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Msu
{
    public class SuperMetroidMusicPack : MusicPack
    {
        public SuperMetroidMusicPack(string? title, string? author, IDictionary<int, PcmTrackSet> tracks)
            : base(title, author, tracks)
        {
        }

        public virtual PcmTrackSet? this[SuperMetroidSoundtrack track]
            => base[(int)track];

        public static bool IsValidTrackNumber(int trackNumber)
            => Enum.IsDefined(typeof(SuperMetroidSoundtrack), trackNumber);

        public override string ToString() => $"{base.ToString()} (Super Metroid)";
    }
}
