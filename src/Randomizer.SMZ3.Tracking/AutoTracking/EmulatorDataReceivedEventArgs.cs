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
        public EmulatorDataReceivedEventArgs(int address, EmulatorMemoryData data)
        {
            Address = address;
            Data = data;
        }

        /// <summary>
        /// The memory address that this response is for
        /// </summary>
        public int Address { get; }

        /// <summary>
        /// The data provided by the emulator
        /// </summary>
        public EmulatorMemoryData Data { get; }
    }
}
