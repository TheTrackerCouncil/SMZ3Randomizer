using System.Collections.Generic;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

public class UncleEquipmentPatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        var item = data.World.HyruleCastle.LinksUncle.Item;
        if (item.Type != ItemType.ProgressiveSword)
        {
            yield return new GeneratedPatch(Snes(0xDD263), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD26B), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD293), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD29B), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x00, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD2B3), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x02, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD2BB), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x02, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD2E3), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x02, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD2EB), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x02, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD31B), new byte[] { 0x00, 0x00, 0xE4, 0xFF, 0x08, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD323), new byte[] { 0x00, 0x00, 0xE4, 0xFF, 0x08, 0x0E });
        }
        if (item.Type != ItemType.ProgressiveShield)
        {
            yield return new GeneratedPatch(Snes(0xDD253), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD25B), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD283), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x00, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD28B), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x00, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD2CB), new byte[] { 0x00, 0x00, 0xF6, 0xFF, 0x02, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD2FB), new byte[] { 0x00, 0x00, 0xF7, 0xFF, 0x02, 0x0E });
            yield return new GeneratedPatch(Snes(0xDD313), new byte[] { 0x00, 0x00, 0xE4, 0xFF, 0x08, 0x0E });
        }
    }
}
