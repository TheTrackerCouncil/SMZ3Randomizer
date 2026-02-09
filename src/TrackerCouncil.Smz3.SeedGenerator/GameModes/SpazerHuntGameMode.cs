using System;
using System.Collections.Generic;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.SeedGenerator.GameModes;

[GameModeType(GameModeType.SpazerHunt)]
public class SpazerHuntGameMode : GameModeBase
{
    public override void UpdateWorld(World world, int seed)
    {
        var rng = new Random(seed);
        for (var i = 0; i < (world.Id + 1) * 25; i++)
        {
            rng.Next();
        }

        var swaps = new List<SwapItemPoolRequest>();

        for (var i = 0; i < world.Config.GameModeOptions.NumSpazersInPool; i++)
        {
            swaps.Add(new SwapItemPoolRequest
            {
                FromItemType = ItemType.TwentyRupees,
                ToItemType = ItemType.Spazer,
                IsNiceToHave = true
            });
        }

        world.ItemPools.SwapItems(swaps);
    }
}
