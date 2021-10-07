using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
