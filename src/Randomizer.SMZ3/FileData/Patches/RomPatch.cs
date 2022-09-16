using System;
using System.Collections.Generic;
using System.Text;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.FileData.Patches
{
    /// <summary>
    /// Represents one or more changes that can be applied to a generated SMZ3
    /// ROM.
    /// </summary>
    public abstract class RomPatch
    {
        /// <summary>
        /// Returns the PC offset for the specified SNES address.
        /// </summary>
        /// <param name="addr">The SNES address to convert.</param>
        /// <returns>
        /// The PC offset equivalent to the SNES <paramref name="addr"/>.
        /// </returns>
        public static int Snes(int addr)
        {
            addr = addr switch
            {
                /* Redirect hi bank $30 access into ExHiRom lo bank $40 */
                _ when (addr & 0xFF8000) == 0x308000 => 0x400000 | (addr & 0x7FFF),
                /* General case, add ExHi offset for banks < $80, and collapse mirroring */
                _ => (addr < 0x800000 ? 0x400000 : 0) | (addr & 0x3FFFFF),
            };
            if (addr > 0x600000)
                throw new InvalidOperationException($"Unmapped pc address target ${addr:X}");
            return addr;
        }

        /// <summary>
        /// Returns a byte array representing the specified 32-bit unsigned
        /// integer.
        /// </summary>
        /// <param name="value">The 32-bit unsigned integer.</param>
        /// <returns>
        /// A new byte array containing the 32-bit unsigned integer.
        /// </returns>
        public static byte[] UintBytes(int value) => BitConverter.GetBytes((uint)value);

        /// <summary>
        /// Returns a byte array representing the specified 16-bit unsigned
        /// integer.
        /// </summary>
        /// <param name="value">The 16-bit unsigned integer.</param>
        /// <returns>
        /// A new byte array containing the 16-bit unsigned integer.
        /// </returns>
        public static byte[] UshortBytes(int value) => BitConverter.GetBytes((ushort)value);

        /// <summary>
        /// Returns a byte array representing the specified ASCII-encoded text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        /// A new byte array containing the ASCII representation of the
        /// <paramref name="text"/>.
        /// </returns>
        public static byte[] AsAscii(string text) => Encoding.ASCII.GetBytes(text);

        /// <summary>
        /// Returns the changes to be applied to an SMZ3 ROM file.
        /// </summary>
        /// <param name="config">The configuration for the seed.</param>
        /// <returns>
        /// A collection of changes, represented by the data to overwrite at the
        /// specified ROM offset.
        /// </returns>
        public abstract IEnumerable<(int offset, byte[] data)> GetChanges(Config config);
    }
}
