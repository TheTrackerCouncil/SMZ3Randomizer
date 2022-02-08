using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.FileData.Patches
{
    /// <summary>
    /// Represents an SMZ3 ROM patch that makes Super Metroid more casual.
    /// </summary>
    public class CasualSMPatch : RomPatch
    {
        /// <summary>
        /// Returns the changes to be applied to an SMZ3 ROM file.
        /// </summary>
        /// <param name="config">The configuration for the seed.</param>
        /// <returns>
        /// A collection of changes, represented by the data to overwrite at the
        /// specified ROM offset.
        /// </returns>
        public override IEnumerable<(int offset, byte[] data)> GetChanges(Config config)
        {
            if (!config.CasualSMPatches)
                yield break;

            // Infinite Space Jump
            // See: https://github.com/theonlydude/RandomMetroidSolver/blob/master/patches/common/patches.py#L97
            yield return (0x382493, new byte[] { 0x80, 0x0D });
        }
    }
}
