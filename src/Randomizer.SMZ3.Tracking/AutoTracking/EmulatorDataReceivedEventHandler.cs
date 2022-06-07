using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Event for when the connector has received data from the emulator
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void EmulatorDataReceivedEventHandler(object sender, EmulatorDataReceivedEventArgs e);
}
