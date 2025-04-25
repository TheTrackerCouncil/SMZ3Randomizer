using System.Collections.Generic;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

/// <summary>
/// Represents an SMZ3 patch that changes the song the Etecoons sing
/// </summary>
/// <remarks>
/// We are editing Library 2, Sound 0x35 in the Super Metroid SPC engine.
/// The engine starts at $CF8108 in the vanilla ROM and $CF0108 in SMZ3.
/// The word at $CF2510 indicates how many voices the sound effect will use.
/// The list of voice pointers for the sound effect starts at $CF2BA7 and is followed by the instruction list for each
/// voice. Each instruction list must be terminated by a byte of 0xFF. In all, 128 (0x80) bytes are available, for a
/// maximum of 25 notes with one voice, unless loop instructions are used.
/// </remarks>
public class EtecoonsJinglePatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        // These values represent offsets into the SPC engine.
        // Convert a ROM address to an offset by subtracting $CF0108 for the starting location in the ROM and an
        // additional $1500 for the data block and echo buffer, after which the engine code itself starts.
        // (We'll make juggling these values easier when we add more jingles.)

        // Overwrite the setup call to use two voices at high priority.
        yield return new GeneratedPatch(Snes(0xCF2510), UshortBytes(0x39A8));

        // Write the two voice pointers.
        // For reference, this first value is at engine offset $3F9F.
        yield return new GeneratedPatch(Snes(0xCF2BA7), UshortBytes(0x3FA3));
        yield return new GeneratedPatch(Snes(0xCF2BA9), UshortBytes(0x3FA3 + 36));

        // Voice 0 (melody), 36 bytes
        yield return new GeneratedPatch(Snes(0xCF2BAB), [
            0x1D, 0x80, 0x0A, 0xAD, 0x0B,
            0x1D, 0x80, 0x0A, 0xAD, 0x18,
            0x1D, 0x80, 0x0A, 0xAD, 0x18,
            0x1D, 0x80, 0x0A, 0xA9, 0x0B,
            0x1D, 0x80, 0x0A, 0xAD, 0x1B,
            0x1D, 0x80, 0x0A, 0xB0, 0x30,
            0x1D, 0x80, 0x0A, 0xA4, 0x30,
            0xFF
        ]);

        // Voice 1 (bassline), 36 bytes
        yield return new GeneratedPatch(Snes(0xCF2BAB + 36), [
            0x1D, 0x70, 0x0A, 0x9F, 0x0B,
            0x1D, 0x70, 0x0A, 0x9F, 0x18,
            0x1D, 0x70, 0x0A, 0x9F, 0x18,
            0x1D, 0x70, 0x0A, 0x9F, 0x0B,
            0x1D, 0x70, 0x0A, 0x9F, 0x18,
            0x1D, 0x70, 0x0A, 0xA4, 0x30,
            0x1D, 0x70, 0x0A, 0x98, 0x30,
            0xFF
        ]);
    }
}
