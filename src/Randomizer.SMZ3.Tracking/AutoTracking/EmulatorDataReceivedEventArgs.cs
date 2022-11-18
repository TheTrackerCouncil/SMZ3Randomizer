namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Event arguments for when connector has received data from the emulator
    /// </summary>
    public class EmulatorDataReceivedEventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <param name="romName"></param>
        /// <param name="romHash"></param>
        public EmulatorDataReceivedEventArgs(int address, EmulatorMemoryData data, string? romName = null, string? romHash = null)
        {
            Address = address;
            Data = data;
            RomName = romName;
            RomHash = romHash;
        }

        /// <summary>
        /// The memory address that this response is for
        /// </summary>
        public int Address { get; }

        /// <summary>
        /// The data provided by the emulator
        /// </summary>
        public EmulatorMemoryData Data { get; }

        /// <summary>
        /// The filename of the current rom (BizHawk Lua connection only)
        /// </summary>
        public string? RomName { get; }

        /// <summary>
        /// The hash of the current rom (BizHawk Lua connection only)
        /// </summary>
        public string? RomHash { get; }
    }
}
