using System.Collections.Generic;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

public class HeartColorPatch : RomPatch
{
    private const int HudBase = 0xDFA1E;
    private const int FileSelect = 0x1BD6AA;

    private static readonly Dictionary<HeartColor, byte> s_hudValues = new()
    {
        [HeartColor.Red] = 0x24,
        [HeartColor.Yellow] = 0x28,
        [HeartColor.Green] = 0x3C,
        [HeartColor.Blue] = 0x2C
    };

    private static readonly Dictionary<HeartColor, byte[]> s_fileSelectValues = new()
    {
        [HeartColor.Red] = new byte[] { 0x18, 0x00 },
        [HeartColor.Yellow] = new byte[] { 0xBC, 0x02 },
        [HeartColor.Green] = new byte[] { 0x04, 0x17 },
        [HeartColor.Blue] = new byte[] { 0xC9, 0x69 }
    };

    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var hudValue = GetHudValue(data.Config.HeartColor);
        for (var i = 0; i < 20; i += 2)
        {
            yield return new GeneratedPatch(Snes(HudBase + i), new[] { hudValue });
        }

        var fileSelectValue = GetFileSelectValue(HeartColor.Red);
        yield return new GeneratedPatch(Snes(FileSelect), fileSelectValue);
    }

    protected virtual byte GetHudValue(HeartColor color)
        => s_hudValues[color];

    protected virtual byte[] GetFileSelectValue(HeartColor color)
        => s_fileSelectValues[color];
}
