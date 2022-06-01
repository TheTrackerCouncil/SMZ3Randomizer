using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class AutoTrackerMessage
    {
        public string Action { get; set; }
        public int Address { get; set; }
        public int Length { get; set; }
        public string Domain { get; set; }
        public string Bytes { get; set; }

        private byte[]? _bytes { get; set; }

        public Game Game { get; set; } = Game.Both;

        public bool ShouldSend(Game currentGame, bool hasStartedGame)
        {
            return (!hasStartedGame && Game == Game.Neither) || (hasStartedGame && Game != Game.Neither && (Game == Game.Both || Game == currentGame));
        }

        public byte[] GetByteArray()
        {
            if (_bytes == null && Bytes != null)
            {
                _bytes = Convert.FromBase64String(Bytes);
            }
            return _bytes;
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
        /// <param name="address">The origin location in memory</param>
        /// <param name="location">The offset location to check</param>
        /// <param name="flag">The flag to check against</param>
        /// <returns>True if the value in memory was set or increased</returns>
        public bool CompareUInt8(AutoTrackerMessage? previousMessage, int address, int location, int? flag)
        {
            var prevValue = previousMessage != null && previousMessage.GetByteArray() != null && previousMessage.GetByteArray().Length > location ? previousMessage.ReadUInt8(location) : -1;
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

        public bool IsMemoryEqualTo(AutoTrackerMessage other)
        {
            if (other == null || other.GetByteArray() == null) return false;
            return Enumerable.SequenceEqual(other.GetByteArray(), GetByteArray());
        }
    }

    public enum Game
    {
        Neither,
        SM,
        Zelda,
        Both
    }
}
