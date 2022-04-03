using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Msu
{
    public class ALinkToThePastMusicPack : MusicPack
    {
        public ALinkToThePastMusicPack(string? title, string? author, IDictionary<int, PcmTrackSet> tracks)
            : base(title, author, tracks)
        {
        }

        public virtual PcmTrackSet? this[ALttPSoundtrack track]
            => base[(int)track];

        public virtual PcmTrackSet? this[ALttpExtendedSoundtrack track]
            => base[(int)track];

        public static bool IsValidTrackNumber(int trackNumber)
            => Enum.IsDefined(typeof(ALttPSoundtrack), trackNumber)
                || Enum.IsDefined(typeof(ALttpExtendedSoundtrack), trackNumber);

        public override string ToString() => $"{base.ToString()} (ALttP)";
    }
}
