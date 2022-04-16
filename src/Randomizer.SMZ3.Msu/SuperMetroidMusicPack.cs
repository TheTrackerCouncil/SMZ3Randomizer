using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Msu
{
    public class SuperMetroidMusicPack : MusicPack
    {
        public SuperMetroidMusicPack(string? title, string? author, IEnumerable<PcmTrack> tracks)
            : base(title, author, tracks)
        {
            Game = MsuGame.SuperMetroid;
        }

        public PcmTrack? this[SuperMetroidSoundtrack track]
            => base[(int)track];

        public static bool IsValidTrackNumber(int trackNumber)
            => Enum.IsDefined(typeof(SuperMetroidSoundtrack), trackNumber);

        public override string ToString() => $"{base.ToString()} (Super Metroid)";
    }
}
