using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Event arguments for when connector has received data from the emulator
    /// </summary>
    public class EmulatorDataReceivedEventArgs
    {
        public EmulatorDataReceivedEventArgs(int address, EmulatorMemoryData data)
        {
            Address = address;
            Data = data;
        }

        public int Address { get; }
        public EmulatorMemoryData Data { get; }
    }
}
