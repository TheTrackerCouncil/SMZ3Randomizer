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
            yield return new InfiniteSpaceJumpPatch();
            yield return new MenuSpeedPatch();
            yield return new FlashRemovalPatch();
            yield return new NoBozoSoftlock();
            yield return new GoalsPatch();
            yield return new MetroidControlsPatch();
            yield return new StartingEquipmentPatch();
        }
    }
}
