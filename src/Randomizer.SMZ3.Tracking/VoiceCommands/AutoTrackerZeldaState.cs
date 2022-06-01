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
        private AutoTrackerMessage _message;

        public AutoTrackerZeldaState(AutoTrackerMessage message)
        {
            _message = message;
        }

        public int CurrentRoom => _message.ReadUInt8(0xA0);

        public int PreviousRoom => _message.ReadUInt8(0xA2);

        public int State => _message.ReadUInt8(0x10);

        public int Substate => _message.ReadUInt8(0x11);

        public override string ToString()
        {
            return $"CurrentRoom: {CurrentRoom} | PreviousRoom: {PreviousRoom} | State: {State} | Substate: {Substate}";
        }
    }
}
