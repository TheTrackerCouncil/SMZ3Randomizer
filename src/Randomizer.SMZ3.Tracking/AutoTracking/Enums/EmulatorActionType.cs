namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// The type of action
    /// </summary>
    public enum EmulatorActionType
    {
        /// <summary>
        /// Read a block from memory
        /// </summary>
        ReadBlock,

        /// <summary>
        /// Write a uint8 value to memory
        /// </summary>
        WriteUInt8,

        /// <summary>
        /// Write a uint16 value to memory
        /// </summary>
        WriteUInt16
    }
}
