using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[Order(-9)]
public class MetadataPatch : RomPatch
{
    private GetPatchesRequest _data = null!;

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        _data = data;
        var patches = new List<GeneratedPatch>();

        patches.AddRange(WriteRngBlock());
        patches.AddRange(WritePlayerNames());
        patches.AddRange( WriteSeedData());
        patches.AddRange(WriteGameTitle());
        patches.AddRange(WriteCommonFlags());

        return patches;
    }

    private IEnumerable<GeneratedPatch> WriteRngBlock()
    {
        // RNG Block
        yield return new GeneratedPatch(0x420000, Enumerable.Range(0, 1024).Select(_ => (byte)_data.Random.Next(0x100)).ToArray());
    }

    private IEnumerable<GeneratedPatch> WritePlayerNames()
    {
        foreach (var world in _data.Worlds)
        {
            yield return new GeneratedPatch(0x385000 + (world.Id * 16), PlayerNameBytes(world.Player));
        }
        yield return new GeneratedPatch(0x385000 + (_data.Worlds.Count * 16), PlayerNameBytes("Tracker"));
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

        yield return new GeneratedPatch(Snes(0x80FF50), UshortBytes(_data.World.Id));
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
        yield return new GeneratedPatch(Snes(0x00FFC0), title);
        yield return new GeneratedPatch(Snes(0x80FFC0), title);
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
