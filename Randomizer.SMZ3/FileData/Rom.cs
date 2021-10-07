using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
        {
            const int header = 5;
            const int footer = 3;
            ips.Seek(header, SeekOrigin.Begin);
            while (ips.Position + footer < ips.Length)
            {
                var offset = ips.ReadByte() << 16 | ips.ReadByte() << 8 | ips.ReadByte();
                var size = ips.ReadByte() << 8 | ips.ReadByte();
                if (size > 0)
                {
                    ips.Read(rom, offset, size);
                }
                else
                {
                    var rleSize = ips.ReadByte() << 8 | ips.ReadByte();
                    var rleByte = (byte)ips.ReadByte();
                    Array.Fill(rom, rleByte, offset, rleSize);
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
            rom[0x7FDC] = 0xFF;
            rom[0x7FDD] = 0xFF;
            rom[0x7FDE] = 0x00;
            rom[0x7FDF] = 0x00;

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

            var checksum = (ushort)(sum & 0xFFFF);
            var complement = (ushort)(checksum ^ 0xFFFF); // Invert the checksum bits so it cancel itself out when re-calculating
            Debug.WriteLine($"Checksum {checksum:X}");
            rom[0x7FDC] = (byte)(complement & 0xFF);
            rom[0x7FDD] = (byte)(complement << 8);
            rom[0x7FDE] = (byte)(checksum & 0xFF);
            rom[0x7FDF] = (byte)(checksum << 8);

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
