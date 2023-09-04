using System.Collections.Generic;
using System.Linq;
using Randomizer.SMZ3.Generation;

namespace Randomizer.SMZ3.FileData.Patches;

public class MetadataPatch : RomPatch
{
    private PatcherServiceData _data = null!;

    public override IEnumerable<(int offset, byte[] data)> GetChanges(PatcherServiceData data)
    {
        _data = data;
        var patches = new List<(int offset, byte[] data)>();

        patches.AddRange(WriteRngBlock());
        patches.AddRange(WritePlayerNames());
        patches.AddRange( WriteSeedData());
        patches.AddRange(WriteGameTitle());
        patches.AddRange(WriteCommonFlags());

        return patches;
    }

    private IEnumerable<(int offset, byte[] data)> WriteRngBlock()
    {
        // RNG Block
        yield return (0x420000, Enumerable.Range(0, 1024).Select(_ => (byte)_data.Random.Next(0x100)).ToArray());
    }

    private IEnumerable<(int offset, byte[] data)> WritePlayerNames()
    {
        foreach (var world in _data.Worlds)
        {
            yield return (0x385000 + (world.Id * 16), PlayerNameBytes(world.Player));
        }
        yield return (0x385000 + (_data.Worlds.Count * 16), PlayerNameBytes("Tracker"));
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


    private IEnumerable<(int offset, byte[] data)> WriteSeedData()
    {
        var configField =
            ((_data.LocalWorld.Config.Race ? 1 : 0) << 15) |
            ((_data.LocalWorld.Config.Keysanity ? 1 : 0) << 13) |
            ((PatcherServiceData.EnableMultiworld ? 1 : 0) << 12) |
            (Smz3Randomizer.Version.Major << 4) |
            (Smz3Randomizer.Version.Minor << 0);

        yield return (Snes(0x80FF50), UshortBytes(_data.LocalWorld.Id));
        yield return (Snes(0x80FF52), UshortBytes(configField));
        yield return (Snes(0x80FF54), UintBytes(_data.Seed));
        /* Reserve the rest of the space for future use */
        yield return (Snes(0x80FF58), Enumerable.Repeat<byte>(0x00, 8).ToArray());
        yield return (Snes(0x80FF60), AsAscii(_data.SeedGuid));
        yield return (Snes(0x80FF80), AsAscii(_data.LocalWorld.Guid));
    }

    private IEnumerable<(int offset, byte[] data)> WriteGameTitle()
    {
        var title = AsAscii($"SMZ3 Cas' [{_data.Seed:X8}]".PadRight(21)[..21]);
        yield return (Snes(0x00FFC0), title);
        yield return (Snes(0x80FFC0), title);
    }

    private IEnumerable<(int offset, byte[] data)> WriteCommonFlags()
    {
        /* Common Combo Configuration flags at [asm]/config.asm */
        // Enable multiworld (for cheats)
        yield return (Snes(0xF47000), UshortBytes(0x0001));
        // Enable keysanity if applicable
        if (_data.LocalWorld.Config.Keysanity)
            yield return (Snes(0xF47006), UshortBytes(0x0001));
    }
}
