using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.FileData.Patches
{
    internal sealed class RomPatchFactory
    {
        public RomPatchFactory()
        {
        }

        public IEnumerable<RomPatch> GetPatches()
        {
            // Base Randomizer Patches
            yield return new MedallionPatch();
            yield return new ZeldaRewardsPatch();
            yield return new DungeonMusicPatch();
            yield return new MetadataPatch();
            yield return new DiggingGamePatch();
            yield return new ZeldaPrizesPatch();
            yield return new FairyPondTradePatch();
            yield return new UncleEquipmentPatch();
            yield return new SaveAndQuitFromBossRoomPatch();
            yield return new ZeldaTextsPatch();
            yield return new LocationsPatch();
            yield return new MetroidKeysanityPatch();
            yield return new ZeldaKeysanityPatch();

            // Additional settings patches
            yield return new HeartColorPatch();
            yield return new LowHealthPatch();
            yield return new InfiniteSpaceJumpPatch();
            yield return new MenuSpeedPatch();
            yield return new FlashRemovalPatch();
            yield return new NoBozoSoftlock();
            yield return new GoalsPatch();
            yield return new MetroidControlsPatch();
            yield return new StartingEquipmentPatch();
            yield return new MetroidAutoSavePatch();
        }
    }
}
