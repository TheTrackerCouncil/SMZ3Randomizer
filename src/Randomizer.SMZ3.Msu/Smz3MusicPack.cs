using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace Randomizer.SMZ3.Msu
{
    public class Smz3MusicPack : MusicPack
    {
        public Smz3MusicPack(string? title, string? author, IEnumerable<PcmTrack> tracks)
            : base(title, author, tracks)
        {
        }

        [YamlIgnore]
        public PcmTrack? ComboCredits => base[99];

        public PcmTrack? this[SuperMetroidSoundtrack track]
            => base[(int)track];

        public PcmTrack? this[ALttPSoundtrack track]
            => base[(int)track + 100];

        public PcmTrack? this[ALttpExtendedSoundtrack track]
            => base[(int)track + 100];

        public static bool IsValidTrackNumber(int trackNumber)
            => trackNumber == 99
                || Enum.IsDefined(typeof(SuperMetroidSoundtrack), trackNumber)
                || (trackNumber > 100 && Enum.IsDefined(typeof(ALttPSoundtrack), trackNumber - 100))
                || (trackNumber > 100 && Enum.IsDefined(typeof(ALttpExtendedSoundtrack), trackNumber - 100));

        public override string ToString() => $"{base.ToString()} (SMZ3)";
    }
}
