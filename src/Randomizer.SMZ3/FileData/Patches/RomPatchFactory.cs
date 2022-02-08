using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.FileData.Patches
{
    public class RomPatchFactory
    {
        public RomPatchFactory()
        {
        }

        public virtual IEnumerable<RomPatch> GetPatches()
        {
            yield return new HeartColorPatch();
            yield return new LowHealthPatch();
            yield return new CasualSMPatch();
            yield return new MenuSpeedPatch();
        }
    }
}
