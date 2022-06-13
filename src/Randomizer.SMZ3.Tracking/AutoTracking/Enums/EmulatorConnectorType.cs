using System.ComponentModel;

namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Enum for the type of connector to use
    /// </summary>
    public enum EmulatorConnectorType
    {
        [Description("None")]
        None,
        [Description("Lua Script")]
        Lua,
        [Description("USB2SNES")]
        USB2SNES
    }
}
