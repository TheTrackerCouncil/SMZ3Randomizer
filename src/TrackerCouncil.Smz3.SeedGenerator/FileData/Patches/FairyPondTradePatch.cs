using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

[Order(-6)]
public class FairyPondTradePatch : RomPatch
{
    public override IEnumerable<GeneratedPatch> GetChanges(GetPatchesRequest data)
    {
        if (!data.World.Config.CasPatches.RandomizedBottles) yield break;

        var tradeOptions = Enum.GetValues<ItemType>().Where(x => x.IsInCategory(ItemCategory.Bottle)).ToList();

        var waterfallTrade = data.PlandoConfig.WaterfallFairyTrade?.IsInCategory(ItemCategory.Bottle) == true
            ? data.PlandoConfig.WaterfallFairyTrade
            : tradeOptions.Random(data.Random);

        var pyramidTrade = data.PlandoConfig.PyramidFairyTrade?.IsInCategory(ItemCategory.Bottle) == true
            ? data.PlandoConfig.PyramidFairyTrade
            : tradeOptions.Random(data.Random);

        yield return new GeneratedPatch(Snes(0x6C8FF), [(byte)(waterfallTrade ?? ItemType.Bottle)]); // Waterfall Fairy
        yield return new GeneratedPatch(Snes(0x6C93B), [(byte)(pyramidTrade ?? ItemType.Bottle)]); // Pyramid Fairy
    }
}
