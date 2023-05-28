using System;
using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.FileData.Patches
{
    /// <summary>
    /// Represents an SMZ3 ROM patch that enables auto saving upon Metroid deaths
    /// </summary>
    public class MetroidAutoSavePatch : RomPatch
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
            // Updates the value set in config.asm
            if (config.CasPatches.MetroidAutoSave)
                yield return (Snes(0xF4700C), UshortBytes(0x0001));
            else
                yield return (Snes(0xF4700C), UshortBytes(0x0000));
        }
    }
}
