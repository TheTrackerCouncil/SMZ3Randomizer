using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Used to retrieve certain states based on the memory in Metroid
    /// Seee http://alttp.run/hacking/index.php?title=RAM:_Bank_0x7E:_Page_0x00 for details on the memory
    /// </summary>
    public class AutoTrackerZeldaState
    {
        private readonly AutoTrackerMessage _message;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public AutoTrackerZeldaState(AutoTrackerMessage message)
        {
            _message = message;
        }

        /// <summary>
        /// The current room the player is in
        /// </summary>
        public int CurrentRoom => _message.ReadUInt8(0xA0);

        /// <summary>
        /// The previous room the player was in
        /// </summary>
        public int PreviousRoom => _message.ReadUInt8(0xA2);

        /// <summary>
        /// The state of the game (Overworld, Dungeon, etc.)
        /// </summary>
        public int State => _message.ReadUInt8(0x10);

        /// <summary>
        /// The secondary state value
        /// </summary>
        public int Substate => _message.ReadUInt8(0x11);

        /// <summary>
        /// The player's Y location
        /// </summary>
        public int LinkY => _message.ReadUInt8(0x20);

        /// <summary>
        /// The player's X Location
        /// </summary>
        public int LinkX => _message.ReadUInt8(0x22);

        /// <summary>
        /// What the player is currently doing
        /// </summary>
        public int LinkState => _message.ReadUInt8(0x5D);

        /// <summary>
        /// Value used to determine if the player is in the light or dark world
        /// Apparently this is used for other calculations as well, so need to be a bit careful
        /// Transitioning from Super Metroid also seems to break this until you go through a portal
        /// </summary>
        public int OverworldValue => _message.ReadUInt8(0x7B);

        /// <summary>
        /// True if Link is on the bottom half of the current room
        /// </summary>
        public bool IsOnBottomHalfOfroom => _message.ReadUInt8(0xAA) == 2;

        /// <summary>
        /// True if Link is on the right half of the current room
        /// </summary>
        public bool IsOnRightHalfOfRoom => _message.ReadUInt8(0xA9) == 1;

        /// <summary>
        /// The overworld screen that the player is on
        /// </summary>
        public int OverworldScreen => _message.ReadUInt8(0x8A);

        /// <summary>
        /// Get debug string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Room: {PreviousRoom}->{CurrentRoom} | State: {State}/{Substate} | X,Y: {LinkX},{LinkY} | LinkState: {LinkState} | OW: {OverworldValue} | {_message.ReadUInt8(0x8A)}";
        }
    }
}
