using System.Collections.Generic;
using Randomizer.Data.Options;
using Randomizer.SMZ3.Generation;

namespace Randomizer.SMZ3.FileData.Patches;

/// <summary>
/// Represents patches for setting the SM button mappings and moon walk
/// </summary>
public class MetroidControlsPatch : RomPatch
{
    public override IEnumerable<(int offset, byte[] data)> GetChanges(PatcherServiceData data)
    {
        yield return (Snes(0x81B331), UshortBytes((int)data.Config.MetroidControls.Shoot));
        yield return (Snes(0x81B325), UshortBytes((int)data.Config.MetroidControls.Jump));
        yield return (Snes(0x81B32B), UshortBytes((int)data.Config.MetroidControls.Dash));
        yield return (Snes(0x81B33D), UshortBytes((int)data.Config.MetroidControls.ItemSelect));
        yield return (Snes(0x81B337), UshortBytes((int)data.Config.MetroidControls.ItemCancel));
        yield return (Snes(0x81B343), UshortBytes((int)data.Config.MetroidControls.AimUp));
        yield return (Snes(0x81B349), UshortBytes((int)data.Config.MetroidControls.AimDown));
        yield return (Snes(0x81EE80), UshortBytes(data.Config.MetroidControls.MoonWalk ? 0x0001 : 0x0000));
        yield return (Snes(0x90FF50), UshortBytes(data.Config.MetroidControls.ItemCancelBehavior == ItemCancelBehavior.Hold ? 0x0001 : 0x0000));
    }
}
