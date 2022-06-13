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
