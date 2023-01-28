using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Randomizer.SMZ3.FileData
{
    public static class Rom
    {
        public static byte[] CombineSMZ3Rom(Stream smRom, Stream z3Rom)
        {
            int pos;
            var combined = new byte[0x600000];

            /* SM hi bank */
            pos = 0;
            for (var i = 0; i < 0x40; i++)
            {
                smRom.Read(combined, pos + 0x8000, 0x8000);
                pos += 0x10000;
            }

            /* SM lo bank */
            pos = 0;
            for (var i = 0; i < 0x40; i++)
            {
                smRom.Read(combined, pos, 0x8000);
                pos += 0x10000;
            }

            /* Z3 hi bank*/
            pos = 0x400000;
            for (var i = 0; i < 0x20; i++)
            {
                z3Rom.Read(combined, pos + 0x8000, 0x8000);
                pos += 0x10000;
            }

            return combined;
        }

        public static byte[] ExpandRom(Stream rom, int size)
        {
            var expanded = new byte[size];
            rom.Read(expanded, 0, size);
            return expanded;
        }

        public static void ApplyIps(byte[] rom, Stream ips)
            => ApplyIps(rom, ips, offset => offset);

        // Applying a Super Metroid IPS to a combined SMZ3 ROM requires some switching around
        public static void ApplySuperMetroidIps(byte[] rom, Stream ips)
            => ApplyIps(rom, ips, TranslateSuperMetroidOffset);

        public static int TranslateSuperMetroidOffset(int offset)
        {
            if (offset >= 0x00300000)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Super Metroid offset must be below 0x00300000"); ;

            var isHiBank = offset < 0x200000;
            var baseOffset = isHiBank ? offset : offset - 0x200000;
            var i = baseOffset / 0x8000;
            if (isHiBank)
                i++;
            return baseOffset + (i * 0x8000);
        }

        public static int TranslateZeldaOffset(int offset)
        {
            var val = ((offset / 0x8000) + 0x1) * 0x8000 + offset;
            return val;
        }

        public static void ApplyIps(byte[] rom, Stream ips, Func<int, int> translateOffset)
        {
            const int header = 5;
            const int footer = 3;
            ips.Seek(header, SeekOrigin.Begin);
            while (ips.Position + footer < ips.Length)
            {
                var offset = (ips.ReadByte() << 16) | (ips.ReadByte() << 8) | ips.ReadByte();
                var size = (ips.ReadByte() << 8) | ips.ReadByte();
                if (size > 0)
                {
                    ips.Read(rom, translateOffset(offset), size);
                }
                else
                {
                    var rleSize = (ips.ReadByte() << 8) | ips.ReadByte();
                    var rleByte = (byte)ips.ReadByte();
                    Array.Fill(rom, rleByte, translateOffset(offset), rleSize);
                }
            }
        }

        public static void ApplySeed(byte[] rom, IDictionary<int, byte[]> patch)
        {
            foreach (var (offset, bytes) in patch)
                bytes.CopyTo(rom, offset);
        }

        public static void UpdateChecksum(byte[] rom)
        {
            var romHeader = 0x40FFB0; // 0x7FB0 (base) + 0x8000 (hirom) + 0x400000 (extended)
            var romHeaderComplement = romHeader + 0x2C;
            var romHeaderChecksum = romHeader + 0x2E;

            var origChecksum = BitConverter.ToUInt16(rom, romHeaderChecksum);
            var origComplement = BitConverter.ToUInt16(rom, romHeaderComplement);
            Debug.WriteLine($"Current checksum: {origChecksum:X4} (+ {origComplement:X4} = {origComplement + origChecksum:X4})");

            // Algorithm from snes9x memmap.cpp
            var sum = 0;
            if ((rom.Length & 0x7FFF) != 0)
            {
                sum += Sum(rom, rom.Length);
            }
            else
            {
                var length = rom.Length;
                sum += MirrorSum(rom, ref length);
            }

            // Invert the checksum bits so it cancel itself out when
            // re-calculating
            var checksum = (ushort)(sum & 0xFFFF);
            var complement = (ushort)(checksum ^ 0xFFFF);
            Debug.WriteLine($"Checksum {checksum:X4} (+ {complement:X4} = {checksum + complement:X4}");

            BitConverter.GetBytes(complement).CopyTo(rom, romHeaderComplement);
            BitConverter.GetBytes(checksum).CopyTo(rom, romHeaderChecksum);

            static ushort Sum(Span<byte> span, int length)
            {
                var sum = 0;
                for (var i = 0; i < length; i++)
                    sum += span[i];
                return unchecked((ushort)sum);
            }

            static ushort MirrorSum(Span<byte> span, ref int length, int mask = 0x800000)
            {
                while ((length & mask) == 0 && mask != 0)
                    mask >>= 1;

                var part1 = Sum(span, mask);
                var part2 = (ushort)0;

                var nextLength = length - mask;
                if (nextLength != 0)
                {
                    part2 = MirrorSum(span.Slice(mask), ref nextLength, mask >> 1);

                    while (nextLength < mask)
                    {
                        nextLength += nextLength;
                        part2 += part2;
                    }

                    length = mask + mask;
                }

                return unchecked((ushort)(part1 + part2));
            }
        }
    }
}
