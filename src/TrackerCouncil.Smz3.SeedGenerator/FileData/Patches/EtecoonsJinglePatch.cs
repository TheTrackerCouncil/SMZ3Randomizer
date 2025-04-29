using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared;

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
    internal class Jingle
    {
        public required Voice Voice1 { get; init; }
        public Voice? Voice2 { get; init; }
    }

    internal class Note
    {
        public required byte Duration { get; init; }
        public byte Pan { get; init; } = 0x0A;
        public required NoteValue Value { get; init; }
        public required byte Volume { get; init; }

        public List<byte> ToBytes()
        {
            return
            [
                0x1D, // Etecoon cry
                Volume,
                Pan,
                (byte)Value,
                Duration
            ];
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal enum NoteValue : byte
    {
        C1 = 0x85, Cs1, D1, Ds1, E1, F1, Fs1, G1, Gs1, A1, As1, B1,
        C2, Cs2, D2, Ds2, E2, F2, Fs2, G2, Gs2, A2, As2, B2,
        C3, Cs3, D3, Ds3, E3, F3, Fs3, G3, Gs3, A3, As3, B3,
        C4, Cs4, D4, Ds4, E4, F4, Fs4, G4, Gs4, A4, As4, B4,
        C5, Cs5, D5, Ds5, E5, F5, Fs5, G5, Gs5, A5, As5, B5,
        C6, Cs6, D6, Ds6, E6, F6, Fs6, G6, Gs6, A6, As6, B6,
        C7, Cs7, D7, Ds7, E7, F7, Fs7, G7, Gs7, A7, As7, B7
    }

    internal class Voice
    {
        public required List<Note> Notes { get; init; }

        public List<byte> ToBytes()
        {
            var bytes = new List<byte>();

            foreach (var note in Notes)
            {
                bytes.AddRange(note.ToBytes());
            }

            bytes.Add(0xFF); // Instruction list terminator

            return bytes;
        }
    }

    private static readonly Dictionary<EtecoonsJingle, Jingle> s_jingles = new()
    {
        {
            EtecoonsJingle.Vanilla, new Jingle
            {
                Voice1 = new Voice
                {
                    Notes =
                    [
                        new Note { Value = NoteValue.C4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.C4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.F4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.F4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.G4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.G4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.A4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.A4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.B4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.B4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.G4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.G4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.D4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.D4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.G4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.G4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.C5, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.C5, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.A4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.A4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.F4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.F4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.D4, Duration = 0x07, Volume = 0x70 },
                        new Note { Value = NoteValue.D4, Duration = 0x07, Volume = 0x20 },
                        new Note { Value = NoteValue.E4, Duration = 0x20, Volume = 0x70 }
                    ]
                }
            }
        },
        {
            EtecoonsJingle.BlueBadger, new Jingle
            {
                Voice1 = new Voice
                {
                    Notes =
                    [
                        new Note { Value = NoteValue.D4, Duration = 0x08, Volume = 0x70, Pan = 0x08 },
                        new Note { Value = NoteValue.E4, Duration = 0x08, Volume = 0x70, Pan = 0x08 },
                        new Note { Value = NoteValue.Fs4, Duration = 0x08, Volume = 0x70, Pan = 0x09 },
                        new Note { Value = NoteValue.G4, Duration = 0x08, Volume = 0x70, Pan = 0x09 },
                        new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70, Pan = 0x0A },
                        new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70, Pan = 0x0A },

                        new Note { Value = NoteValue.D5, Duration = 0x10, Volume = 0x70, Pan = 0x0B },
                        new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70, Pan = 0x0A },
                        new Note { Value = NoteValue.A4, Duration = 0x08, Volume = 0x70, Pan = 0x09 },
                        new Note { Value = NoteValue.G4, Duration = 0x08, Volume = 0x70, Pan = 0x09 },
                        new Note { Value = NoteValue.Fs4, Duration = 0x10, Volume = 0x70, Pan = 0x08 },

                        new Note { Value = NoteValue.D4, Duration = 0x08, Volume = 0x70, Pan = 0x08 },
                        new Note { Value = NoteValue.E4, Duration = 0x08, Volume = 0x70, Pan = 0x08 },
                        new Note { Value = NoteValue.Fs4, Duration = 0x08, Volume = 0x70, Pan = 0x09 },
                        new Note { Value = NoteValue.G4, Duration = 0x08, Volume = 0x70, Pan = 0x09 },
                        new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70, Pan = 0x0A },
                        new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70, Pan = 0x0A },

                        new Note { Value = NoteValue.D5, Duration = 0x08, Volume = 0x70, Pan = 0x0B },
                        new Note { Value = NoteValue.A4, Duration = 0x08, Volume = 0x70, Pan = 0x0A },
                        new Note { Value = NoteValue.Fs4, Duration = 0x08, Volume = 0x70, Pan = 0x09 },
                        new Note { Value = NoteValue.G4, Duration = 0x08, Volume = 0x70, Pan = 0x09 },
                        new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70, Pan = 0x08 }

                        // The rest of the jingle would take too long and wouldn't fit besides.
                        // new Note { Value = NoteValue.D4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.E4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.Fs4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.G4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70 },
                        // new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70 },
                        //
                        // new Note { Value = NoteValue.D5, Duration = 0x10, Volume = 0x70 },
                        // new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70 },
                        // new Note { Value = NoteValue.A4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.G4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.Fs4, Duration = 0x10, Volume = 0x70 },
                        //
                        // new Note { Value = NoteValue.G4, Duration = 0x10, Volume = 0x70 },
                        // new Note { Value = NoteValue.G4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.A4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.B4, Duration = 0x10, Volume = 0x70 },
                        // new Note { Value = NoteValue.B4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.Cs5, Duration = 0x08, Volume = 0x70 },
                        //
                        // new Note { Value = NoteValue.D5, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.Fs4, Duration = 0x08, Volume = 0x70 },
                        // new Note { Value = NoteValue.A4, Duration = 0x10, Volume = 0x70 },
                        // new Note { Value = NoteValue.D4, Duration = 0x10, Volume = 0x70 }
                    ]
                }
                // We can do two measures with a bassline or four without. Choosing four (for now?).
                // Voice2 = new Voice
                // {
                //     Notes =
                //     [
                //         new Note { Value = NoteValue.A3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D4, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //
                //         new Note { Value = NoteValue.A3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D4, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //
                //         new Note { Value = NoteValue.A3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D4, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //
                //         new Note { Value = NoteValue.A3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D4, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.C4, Duration = 0x08, Volume = 0x70 },
                //         new Note { Value = NoteValue.B3, Duration = 0x08, Volume = 0x70 },
                //
                //         new Note { Value = NoteValue.A3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D4, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //
                //         new Note { Value = NoteValue.A3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D4, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //
                //         new Note { Value = NoteValue.A3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.A3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D4, Duration = 0x10, Volume = 0x70 },
                //
                //         new Note { Value = NoteValue.Fs3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.E3, Duration = 0x10, Volume = 0x70 },
                //         new Note { Value = NoteValue.D3, Duration = 0x10, Volume = 0x70 }
                //     ]
                // }
            }
        },
        {
            EtecoonsJingle.SMB1, new Jingle
            {
                Voice1 = new Voice
                {
                    Notes =
                    [
                        new Note { Value = NoteValue.E4, Duration = 0x08, Volume = 0x80 },
                        new Note { Value = NoteValue.E4, Duration = 0x18, Volume = 0x80 },
                        new Note { Value = NoteValue.E4, Duration = 0x18, Volume = 0x80 },
                        new Note { Value = NoteValue.C4, Duration = 0x08, Volume = 0x80 },
                        new Note { Value = NoteValue.E4, Duration = 0x18, Volume = 0x80 },
                        new Note { Value = NoteValue.G4, Duration = 0x30, Volume = 0x80 },
                        new Note { Value = NoteValue.G3, Duration = 0x30, Volume = 0x80 }
                    ]
                },
                Voice2 = new Voice
                {
                    Notes =
                    [
                        new Note { Value = NoteValue.D3, Duration = 0x08, Volume = 0x70 },
                        new Note { Value = NoteValue.D3, Duration = 0x18, Volume = 0x70 },
                        new Note { Value = NoteValue.D3, Duration = 0x18, Volume = 0x70 },
                        new Note { Value = NoteValue.D3, Duration = 0x08, Volume = 0x70 },
                        new Note { Value = NoteValue.D3, Duration = 0x18, Volume = 0x70 },
                        new Note { Value = NoteValue.G3, Duration = 0x30, Volume = 0x70 },
                        new Note { Value = NoteValue.G2, Duration = 0x30, Volume = 0x70 }
                    ]
                }
            }
        },
        {
            EtecoonsJingle.ZeldaSecret, new Jingle
            {
                Voice1 = new Voice
                {
                    Notes =
                    [
                        new Note { Value = NoteValue.E4, Duration = 0x12, Volume = 0x70, Pan = 0x80 },
                        new Note { Value = NoteValue.C4, Duration = 0x12, Volume = 0x70, Pan = 0x80 },
                        new Note { Value = NoteValue.F3, Duration = 0x12, Volume = 0x70, Pan = 0x80 },
                        new Note { Value = NoteValue.F4, Duration = 0x12, Volume = 0x70, Pan = 0x80 }
                    ]
                },
                Voice2 = new Voice
                {
                    Notes =
                    [
                        new Note { Value = NoteValue.Ds4, Duration = 0x0A, Volume = 0x00, Pan = 0xB0 },
                        new Note { Value = NoteValue.Ds4, Duration = 0x12, Volume = 0x70, Pan = 0xB0 },
                        new Note { Value = NoteValue.Fs3, Duration = 0x12, Volume = 0x70, Pan = 0xB0 },
                        new Note { Value = NoteValue.Cs4, Duration = 0x12, Volume = 0x70, Pan = 0xB0 },
                        new Note { Value = NoteValue.A4, Duration = 0x12, Volume = 0x70, Pan = 0xB0 },
                        new Note { Value = NoteValue.A4, Duration = 0x12, Volume = 0x20, Pan = 0xB0 }
                    ]
                }
            }
        }
    };

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var jingle = GetJingle(data.Config.EtecoonsJingle);

        return GetPatch(jingle);
    }

    internal Jingle GetJingle(EtecoonsJingle jingle)
    {
        if (jingle == EtecoonsJingle.Random)
        {
            var random = new Random().Sanitize();

            return s_jingles.Values.Random(random)!;
        }

        return s_jingles[jingle];
    }

    internal IEnumerable<GeneratedPatch> GetPatch(Jingle jingle)
    {
        var voice1 = jingle.Voice1.ToBytes();
        var voice2 = jingle.Voice2?.ToBytes();

        // Make room for the second voice pointer, if necessary.
        var voice1Offset = voice2 is null ? 0xCF2BA9 : 0xCF2BAB;

        if (voice1Offset + voice1.Count + (voice2?.Count ?? 0) >= 0xCF2C29)
        {
            throw new Exception("Etecoons jingle does not fit in the available space!");
        }

        var bytes = new List<byte>();

        bytes.AddRange(jingle.Voice1.ToBytes());

        if (voice2 != null)
        {
            // Overwrite the setup call to use two voices at high priority.
            yield return new GeneratedPatch(Snes(0xCF2510), UshortBytes(0x39A8));

            // Write the two voice pointers.
            yield return new GeneratedPatch(Snes(0xCF2BA7), UshortBytes(0x3FA3));
            yield return new GeneratedPatch(Snes(0xCF2BA9), UshortBytes(0x3FA3 + voice1.Count));

            bytes.AddRange(jingle.Voice2!.ToBytes());
        }

        yield return new GeneratedPatch(Snes(voice1Offset), bytes.ToArray());

        // Uncomment these lines to make the uncharged power beam sound effect use the Etecoon jingle instead.
        // The last two lines should make the Green Brinstar theme play at Samus's ship, which loads the right sample.
        // The jingle may play louder than usual, so be sure to check how it sounds in the actual Hell room.
        // yield return new GeneratedPatch(Snes(0x90B8DE), [0xA9, 0x35, 0x00]); // LDA #$0035
        // yield return new GeneratedPatch(Snes(0x90B8E1), [0x22, 0xA3, 0x90, 0x80]); // JSL $8090A3
        // yield return new GeneratedPatch(Snes(0x8FE7E7), [0x3C, 0x93, 0xD3]); // Empty Crateria
        // yield return new GeneratedPatch(Snes(0x8FE7ED), [0x3C, 0x93, 0xD3]); // Upper Crateria
    }
}
