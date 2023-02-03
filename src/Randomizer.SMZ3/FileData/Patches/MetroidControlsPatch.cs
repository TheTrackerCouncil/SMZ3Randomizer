using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.FileData.Patches;

/// <summary>
/// Represents patches for setting the SM button mappings and moon walk
/// </summary>
public class MetroidControlsPatch : RomPatch
{
    public override IEnumerable<(int offset, byte[] data)> GetChanges(Config config)
    {
        yield return (Patcher.Snes(0x81B331), Patcher.UshortBytes((int)config.MetroidControls.Shoot));
        yield return (Patcher.Snes(0x81B325), Patcher.UshortBytes((int)config.MetroidControls.Jump));
        yield return (Patcher.Snes(0x81B32B), Patcher.UshortBytes((int)config.MetroidControls.Dash));
        yield return (Patcher.Snes(0x81B33D), Patcher.UshortBytes((int)config.MetroidControls.ItemSelect));
        yield return (Patcher.Snes(0x81B337), Patcher.UshortBytes((int)config.MetroidControls.ItemCancel));
        yield return (Patcher.Snes(0x81B343), Patcher.UshortBytes((int)config.MetroidControls.AimUp));
        yield return (Patcher.Snes(0x81B349), Patcher.UshortBytes((int)config.MetroidControls.AimDown));
        yield return (Patcher.Snes(0x81EE80), Patcher.UshortBytes(config.MetroidControls.MoonWalk ? 0x0001 : 0x0000));
        yield return (Patcher.Snes(0x90FF50), Patcher.UshortBytes(config.MetroidControls.ItemCancelBehavior == ItemCancelBehavior.Hold ? 0x0001 : 0x0000));
    }
}
