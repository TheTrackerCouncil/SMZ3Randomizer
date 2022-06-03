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
        private readonly AutoTrackerMessage _message;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public AutoTrackerMetroidState(AutoTrackerMessage message)
        {
            _message = message;
        }

        /// <summary>
        /// The overall room number the player is in
        /// </summary>
        public int CurrentRoom => _message.ReadUInt8(0x7E079B - 0x7E0750);

        /// <summary>
        /// The region room number the player is in
        /// </summary>
        public int CurrentRoomInRegion => _message.ReadUInt8(0x7E079D - 0x7E0750);

        /// <summary>
        /// The current region the player is in
        /// </summary>
        public int CurrentRegion => _message.ReadUInt8(0x7E079F - 0x7E0750);

        /// <summary>
        /// The amount of health
        /// </summary>
        public int Health => _message.ReadUInt8(0x7E09C2 - 0x7E0750);

        /// <summary>
        /// The amount currently in reserve tanks
        /// </summary>
        public int ReserveTanks => _message.ReadUInt8(0x7E09D6 - 0x7E0750);

        /// <summary>
        /// Samus's X Location
        /// </summary>
        public int SamusX => _message.ReadUInt8(0x7E0AF6 - 0x7E0750);

        /// <summary>
        /// Samus's Y Location
        /// </summary>
        public int SamusY => _message.ReadUInt8(0x7E0AFA - 0x7E0750);

        /// <summary>
        /// Prints debug data for the state
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"CurrentRoom: {CurrentRoom} | CurrentRoomInRegion: {CurrentRoomInRegion} | CurrentRegion: {CurrentRegion} | Health: {Health},{ReserveTanks} | X,Y {SamusX},{SamusY}";
        }
    }
}
