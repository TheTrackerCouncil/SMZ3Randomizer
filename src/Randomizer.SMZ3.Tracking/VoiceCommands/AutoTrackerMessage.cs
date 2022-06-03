using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Class used for communicating between the emulator and tracker
    /// </summary>
    public class AutoTrackerMessage
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
        /// The game this message is for
        /// </summary>
        public Game Game { get; set; } = Game.Both;

        /// <summary>
        /// If this message should be sent based on the game the player is currently in
        /// </summary>
        /// <param name="currentGame">The game the player is currently in</param>
        /// <param name="hasStartedGame">If the player has actually started the game</param>
        /// <returns>True if the message should be sent.</returns>
        public bool ShouldSend(Game currentGame, bool hasStartedGame)
        {
            return (!hasStartedGame && Game == Game.Neither) || (hasStartedGame && Game != Game.Neither && (Game == Game.Both || Game == currentGame));
        }

        private byte[]? _bytes { get; set; }

        /// <summary>
        /// Returns the byte array from the base 64 string
        /// </summary>
        /// <returns>The byte array</returns>
        public byte[] GetByteArray()
        {
            if (_bytes == null && Bytes != null)
            {
                _bytes = Convert.FromBase64String(Bytes);
            }
            return _bytes ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Returns the memory value at a location
        /// </summary>
        /// <param name="location">The offset location to check</param>
        /// <returns>The value from the byte array at that location</returns>
        public byte ReadUInt8(int location)
        {
            return GetByteArray()[location];
        }

        /// <summary>
        /// Gets the memory value for a location and returns if it matches a given flag
        /// </summary>
        /// <param name="location">The offset location to check</param>
        /// <param name="flag">The flag to check against</param>
        /// <returns>True if the flag is set for the memory location.</returns>
        public bool CheckUInt8(int location, int flag)
        {
            return (ReadUInt8(location) & flag) == flag;
        }

        /// <summary>
        /// Checks if a value in memory matches a flag or has been increased to denote obtaining an item
        /// </summary>
        /// <param name="previousMessage">The message to compare</param>
        /// <param name="location">The offset location to check</param>
        /// <param name="flag">The flag to check against</param>
        /// <returns>True if the value in memory was set or increased</returns>
        public bool CompareUInt8(AutoTrackerMessage? previousMessage, int location, int? flag)
        {
            var prevValue = previousMessage != null && previousMessage.GetByteArray().Length > location ? previousMessage.ReadUInt8(location) : -1;
            var newValue = ReadUInt8(location);

            if (newValue > prevValue)
            {
                if (flag != null)
                {
                    if ((newValue & flag) == flag)
                    {
                        return true;
                    }
                }
                else if (newValue > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the memory value at a location
        /// </summary>
        /// <param name="location">The offset location to check</param>
        /// <returns>The value from the byte array at that location</returns>
        public int ReadUInt16(int location)
        {
            var bytes = GetByteArray();
            return bytes[location + 1] * 256 + bytes[location];
        }

        /// <summary>
        /// Gets the memory value for a location and returns if it matches a given flag
        /// </summary>
        /// <param name="location">The offset location to check</param>
        /// <param name="flag">The flag to check against</param>
        /// <returns>True if the flag is set for the memory location.</returns>
        public bool CheckUInt16(int location, int flag)
        {
            var data = ReadUInt16(location);
            var adjustedFlag = 1 << flag;
            var temp = data & adjustedFlag;
            return temp == adjustedFlag;
        }

        /// <summary>
        /// Returns if the value in memory matches between messages
        /// </summary>
        /// <param name="other">The other message</param>
        /// <returns>True if the two are equal</returns>
        public bool IsMemoryEqualTo(AutoTrackerMessage other)
        {
            if (other == null || other.GetByteArray() == null) return false;
            return Enumerable.SequenceEqual(other.GetByteArray(), GetByteArray());
        }
    }

    /// <summary>
    /// Which game(s) the message should be sent to the emulator in
    /// </summary>
    public enum Game
    {
        /// <summary>
        /// Send if the player has not started the game
        /// </summary>
        Neither,

        /// <summary>
        /// Send if the player is in Super Metroid
        /// </summary>
        SM,

        /// <summary>
        /// Send if the player is in Zelda
        /// </summary>
        Zelda,

        /// <summary>
        /// Send if the player is in either game
        /// </summary>
        Both
    }
}
