using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Class used to serialize JSON responses from USB2SNES 
    /// </summary>
    public class USB2SNESResponse
    {
        public ICollection<string> Results { get; set; }
    }
}
