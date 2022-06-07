using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Class used to serialize JSON requests to USB2SNES
    /// </summary>
    public class USB2SNESRequest
    {
        public string Opcode { get; set; }
        public string Space { get; set; }
        public ICollection<string> Operands { get; set; }
    }
}
