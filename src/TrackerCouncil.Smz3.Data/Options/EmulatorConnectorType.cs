using System.ComponentModel;

namespace TrackerCouncil.Smz3.Data.Options;

/// <summary>
/// Enum for the type of connector to use
/// </summary>
public enum EmulatorConnectorType
{
    /// <summary>
    /// No connector
    /// </summary>
    [Description("None")]
    None,
    /// <summary>
    /// Lua script for snes9x-rr and Bizhawk
    /// </summary>
    [Description("Lua Script")]
    Lua,
    /// <summary>
    /// USB2SNES or QUSB2SNES
    /// </summary>
    [Description("USB2SNES")]
    USB2SNES
}
