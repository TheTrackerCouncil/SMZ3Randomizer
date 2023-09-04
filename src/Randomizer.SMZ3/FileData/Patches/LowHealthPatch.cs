using System.Collections.Generic;
using Randomizer.Data.Options;

namespace Randomizer.SMZ3.FileData.Patches
{
    public class LowHealthPatch : RomPatch
    {
        private static readonly Dictionary<LowHealthBeepSpeed, byte> s_speedValues = new()
        {
            [LowHealthBeepSpeed.Off] = 0x00,
            [LowHealthBeepSpeed.Double] = 0x10,
            [LowHealthBeepSpeed.Normal] = 0x20,
            [LowHealthBeepSpeed.Half] = 0x40,
            [LowHealthBeepSpeed.Quarter] = 0x80,
        };

        public override IEnumerable<(int offset, byte[] data)> GetChanges(PatcherServiceData data)
        {
            // A Link to the Past
            yield return (0x400033, new[] { s_speedValues[data.Config.LowHealthBeepSpeed] });

            // Super Metroid
            if (data.Config.DisableLowEnergyBeep)
            {
                yield return (Snes(0x90EA9B), new byte[] { 0x80 });
                yield return (Snes(0x90F337), new byte[] { 0x80 });
                yield return (Snes(0x91E6D5), new byte[] { 0x80 });
            }
        }
    }
}
