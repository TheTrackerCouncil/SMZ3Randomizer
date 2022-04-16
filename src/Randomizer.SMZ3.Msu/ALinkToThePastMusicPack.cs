using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Msu
{
    public class ALinkToThePastMusicPack : MusicPack
    {
        public ALinkToThePastMusicPack(string? title, string? author, IEnumerable<PcmTrack> tracks)
            : base(title, author, tracks)
        {
            Game = MsuGame.ALinkToThePast;
        }

        public PcmTrack? this[ALttPSoundtrack track]
            => base[(int)track];

        public PcmTrack? this[ALttpExtendedSoundtrack track]
            => base[(int)track];

        public static bool IsValidTrackNumber(int trackNumber)
            => Enum.IsDefined(typeof(ALttPSoundtrack), trackNumber)
                || Enum.IsDefined(typeof(ALttpExtendedSoundtrack), trackNumber);

        public override string ToString() => $"{base.ToString()} (ALttP)";
    }
}
