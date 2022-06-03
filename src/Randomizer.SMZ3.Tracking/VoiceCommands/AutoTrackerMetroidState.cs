using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Used to retrieve certain states based on the memory in Metroid
    /// Seee https://jathys.zophar.net/supermetroid/kejardon/RAMMap.txt for details on the memory
    /// </summary>
    public class AutoTrackerMetroidState
    {
        private AutoTrackerMessage _message;

        public AutoTrackerMetroidState(AutoTrackerMessage message)
        {
            _message = message;
        }

        public int CurrentRoom => _message.ReadUInt8(0x7E079B - 0x7E0750);

        public int CurrentRoomInRegion => _message.ReadUInt8(0x7E079D - 0x7E0750);

        public int CurrentRegion => _message.ReadUInt8(0x7E079F - 0x7E0750);

        public int Health => _message.ReadUInt8(0x7E09C2 - 0x7E0750);

        public int ReserveTanks => _message.ReadUInt8(0x7E09D6 - 0x7E0750);

        public int SamusX => _message.ReadUInt8(0x7E0AF6 - 0x7E0750);

        public int SamusY => _message.ReadUInt8(0x7E0AFA - 0x7E0750);


        public override string ToString()
        {
            return $"CurrentRoom: {CurrentRoom} | CurrentRoomInRegion: {CurrentRoomInRegion} | CurrentRegion: {CurrentRegion} | Health: {Health},{ReserveTanks} | X,Y {SamusX},{SamusY}";
        }
    }
}
