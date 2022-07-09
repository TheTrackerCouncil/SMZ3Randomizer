namespace Randomizer.SMZ3.Tracking.AutoTracking
{
    /// <summary>
    /// Class for requests/responses from the Lua script
    /// </summary>
    public class LuaMessage
    {
        /// <summary>
        /// The action to be done by the emulator (read/write/etc)
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// The starting memory address
        /// </summary>
        public int Address { get; set; }

        /// <summary>
        /// The number of bytes to capture from the emulator
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The type of memory to read or modify (WRAM, CARTRAM, CARTROM)
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// The base 64 byte string
        /// </summary>
        public string? Bytes { get; set; }

        /// <summary>
        /// A single uint8 or uint16 value
        /// </summary>
        public int? Value { get; set; }
    }
}
