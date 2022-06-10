using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// The type of memory
    /// </summary>
    public enum MemoryDomain
    {
        /// <summary>
        /// SNES Memory
        /// </summary>
        WRAM,

        /// <summary>
        /// Cartridge Memory / Save File (AKA SRAM)
        /// </summary>
        CartRAM,

        /// <summary>
        /// Game data saved on cartridge
        /// </summary>
        CartROM
    }
}
