using System.Collections.Generic;
using Randomizer.Abstractions;

namespace Randomizer.SMZ3.FileData.Patches
{
    /// <summary>
    /// Represents an SMZ3 ROM patch that makes jumping in Super Metroid more casual.
    /// </summary>
    public class InfiniteSpaceJumpPatch : RomPatch
    {
        /// <summary>
        /// Returns the changes to be applied to an SMZ3 ROM file.
        /// </summary>
        /// <param name="data">Patcher Data with the world and config information</param>
        /// <returns>
        /// A collection of changes, represented by the data to overwrite at the
        /// specified ROM offset.
        /// </returns>
        public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
        {
            if (!data.Config.CasPatches.InfiniteSpaceJump)
                yield break;

            // Infinite Space Jump
            // See: https://github.com/theonlydude/RandomMetroidSolver/blob/master/patches/common/patches.py#L97
            yield return new GeneratedPatch(Rom.TranslateSuperMetroidOffset(0x82493), new byte[] { 0x80, 0x0D });
        }
    }
}
