using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[Order(-9)]
public class MetadataPatch : RomPatch
{
    private static readonly int s_addrMultiworldFlag = Snes(0xF47000);
    private static readonly int s_addrKeysanityFlag = Snes(0xF47006);
    private static readonly int s_addrPlayerNames = 0x385000;
    private static readonly int s_addrPlayerIndex = Snes(0x80FF50);
    private static readonly int s_addrRng = 0x420000;
    private static readonly int s_addrTitle1 = Snes(0x00FFC0);
    private static readonly int s_addrTitle2 = Snes(0xF47006);

    private GetPatchesRequest _data = null!;

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        _data = data;
        var patches = new List<GeneratedPatch>();

        patches.AddRange(WriteRngBlock());
        patches.AddRange(WritePlayerNames());
        patches.AddRange(WriteSeedData());
        patches.AddRange(WriteGameTitle());
        patches.AddRange(WriteCommonFlags());

        return patches;
    }

    public static bool IsRomMultiworldEnabled(byte[] rom)
    {
        return BitConverter.ToInt16(rom, s_addrMultiworldFlag) == 1;
    }

    public static bool IsRomKeysanityEnabled(byte[] rom)
    {
        return BitConverter.ToInt16(rom, s_addrKeysanityFlag) == 1;
    }

    public static string GetGameTitle(byte[] rom)
    {
        var nameBytes = rom.Skip(s_addrTitle1).Take(21).ToArray();
        return Encoding.ASCII.GetString(nameBytes);
    }

    public static List<string> GetPlayerNames(byte[] rom)
    {
        var toReturn = new List<string>();
        for (var i = 0; i < 128; i++)
        {
            var nameBytes = rom.Skip(s_addrPlayerNames + i * 16).Take(16).ToArray();
            var name = Encoding.ASCII.GetString(nameBytes);
            if (name == "123456789012\0\0\0\0")
            {
                break;
            }
            toReturn.Add(name.Trim());
        }

        return toReturn;
    }

    public static int GetPlayerIndex(byte[] rom)
    {
        return BitConverter.ToInt16(rom, s_addrPlayerIndex);
    }

    private IEnumerable<GeneratedPatch> WriteRngBlock()
    {
        // RNG Block
        yield return new GeneratedPatch(s_addrRng, Enumerable.Range(0, 1024).Select(_ => (byte)_data.Random.Next(0x100)).ToArray());
    }

    private IEnumerable<GeneratedPatch> WritePlayerNames()
    {
        foreach (var world in _data.Worlds)
        {
            yield return new GeneratedPatch(s_addrPlayerNames + (world.Id * 16), PlayerNameBytes(world.Player));
        }
        yield return new GeneratedPatch(s_addrPlayerNames + (_data.Worlds.Count * 16), PlayerNameBytes("Tracker"));
    }

    private byte[] PlayerNameBytes(string name)
    {
        name = name.Length > 12 ? name[..12].TrimEnd() : name;

        const int width = 12;
        var pad = (width - name.Length) / 2;
        name = name.PadLeft(name.Length + pad);
        name = name.PadRight(width);

        return AsAscii(name).Concat(UintBytes(0)).ToArray();
    }


    private IEnumerable<GeneratedPatch> WriteSeedData()
    {
        var configField =
            ((_data.World.Config.Race ? 1 : 0) << 15) |
            ((_data.World.Config.Keysanity ? 1 : 0) << 13) |
            ((GetPatchesRequest.EnableMultiworld ? 1 : 0) << 12) |
            (RandomizerVersion.Version.Major << 4) |
            (RandomizerVersion.Version.Minor << 0);

        yield return new GeneratedPatch(s_addrPlayerIndex, UshortBytes(_data.World.Id));
        yield return new GeneratedPatch(Snes(0x80FF52), UshortBytes(configField));
        yield return new GeneratedPatch(Snes(0x80FF54), UintBytes(_data.Seed));
        /* Reserve the rest of the space for future use */
        yield return new GeneratedPatch(Snes(0x80FF58), Enumerable.Repeat<byte>(0x00, 8).ToArray());
        yield return new GeneratedPatch(Snes(0x80FF60), AsAscii(_data.SeedGuid));
        yield return new GeneratedPatch(Snes(0x80FF80), AsAscii(_data.World.Guid));
    }

    private IEnumerable<GeneratedPatch> WriteGameTitle()
    {
        var title = AsAscii($"SMZ3 Cas' [{_data.Seed:X8}]".PadRight(21)[..21]);
        yield return new GeneratedPatch(s_addrTitle1, title);
        yield return new GeneratedPatch(s_addrTitle2, title);
    }

    private IEnumerable<GeneratedPatch> WriteCommonFlags()
    {
        /* Common Combo Configuration flags at [asm]/config.asm */
        // Enable multiworld (for cheats)
        yield return new GeneratedPatch(Snes(0xF47000), UshortBytes(0x0001));
        // Enable keysanity if applicable
        if (_data.World.Config.Keysanity)
            yield return new GeneratedPatch(Snes(0xF47006), UshortBytes(0x0001));
    }
}
