using System;
using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.FileData.Patches
{
    /// <summary>
    /// Represents a ROM patch that changes the menu speed in A Link to the
    /// Past.
    /// </summary>
    public class MenuSpeedPatch : RomPatch
    {
        private const int MenuSpeedAddress = 0x308048;
        private const int MenuDownChimeAddress = 0x0DDD9A;
        private const int MenuUpChimeAddress = 0x0DDF2A;
        private const int MenuUpChimeAddress2 = 0x0DE0E9;

        private const byte VwoopDown = 0x11;
        private const byte VwoopUp = 0x12;
        private const byte MenuChime = 0x20;

        private static readonly Dictionary<MenuSpeed, byte[]> s_menuSpeedValues = new()
        {
            [MenuSpeed.Slow] = new byte[] { 0x04 },
            [MenuSpeed.Default] = new byte[] { 0x08 },
            [MenuSpeed.Fast] = new byte[] { 0x10 },
            [MenuSpeed.Instant] = new byte[] { 0xE8 }
        };

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
            yield return (Snes(MenuSpeedAddress), s_menuSpeedValues[config.MenuSpeed]);
            yield return (Snes(MenuDownChimeAddress), new[] { config.MenuSpeed == MenuSpeed.Instant ? MenuChime : VwoopDown });
            yield return (Snes(MenuUpChimeAddress), new[] { config.MenuSpeed == MenuSpeed.Instant ? MenuChime : VwoopUp });
            yield return (Snes(MenuUpChimeAddress2), new[] { config.MenuSpeed == MenuSpeed.Instant ? MenuChime : VwoopUp });
        }
    }
}
