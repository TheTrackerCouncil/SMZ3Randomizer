using System;
using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.FileData.Patches
{
    /// <summary>
    /// Represents an SMZ3 ROM patch that updates the Bozo Door Time
    /// </summary>
    public class NoBozoSoftlock : RomPatch
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
            // Updates the value set by bomb_torizo.asm
            if (config.CasPatches.NoBozoSoftlock)
                yield return (Snes(0x84BA54), new byte[] { 0x28 });
            else
                yield return (Snes(0x84BA54), new byte[] { 0x28 * 2 });
        }
    }
}
