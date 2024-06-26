using System;
using System.Collections.Generic;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Shared;
using static TrackerCouncil.Smz3.Data.Options.DropPrize;

namespace TrackerCouncil.Smz3.Data.Options;

public enum DropPrize : byte
{
    Heart = 0xD8,
    GreenRupee = 0xD9,
    BlueRupee = 0xDA,
    RedRupee = 0xDB,
    Bomb1 = 0xDC,
    Bomb4 = 0xDD,
    Bomb8 = 0xDE,
    Magic = 0xDF,
    FullMagic = 0xE0,
    Arrow5 = 0xE1,
    Arrow10 = 0xE2,
    Fairy = 0xE3,
}

public class DropPrizes
{
    private static readonly IEnumerable<DropPrize> s_defaultPool = new[]
    {
        Heart, Heart, Heart, Heart, GreenRupee, Heart, Heart, GreenRupee, // pack 1
        BlueRupee, GreenRupee, BlueRupee, RedRupee, BlueRupee, GreenRupee, BlueRupee, BlueRupee, // pack 2
        FullMagic, Magic, Magic, BlueRupee, FullMagic, Magic, Heart, Magic, // pack 3
        Bomb1, Bomb1, Bomb1, Bomb4, Bomb1, Bomb1, Bomb8, Bomb1, // pack 4
        Arrow5, Heart, Arrow5, Arrow10, Arrow5, Heart, Arrow5, Arrow10, // pack 5
        Magic, GreenRupee, Heart, Arrow5, Magic, Bomb1, GreenRupee, Heart, // pack 6
        Heart, Fairy, FullMagic, RedRupee, Bomb8, Heart, RedRupee, Arrow10, // pack 7
        GreenRupee, BlueRupee, RedRupee, // from pull trees
        GreenRupee, RedRupee, // from prize crab
        GreenRupee, // stunned prize
        RedRupee, // saved fish prize
    };

    private static readonly IEnumerable<DropPrize> s_easyPool = new[] {
        Heart, Fairy, FullMagic, RedRupee, Bomb8, Heart, RedRupee, Arrow10,         // pack 1
        Heart, Fairy, FullMagic, RedRupee, Bomb8, Heart, RedRupee, Arrow10,         // pack 2
        Heart, Fairy, FullMagic, RedRupee, Bomb8, Heart, RedRupee, Arrow10,         // pack 3
        Heart, Fairy, FullMagic, RedRupee, Bomb8, Heart, RedRupee, Arrow10,         // pack 4
        Heart, Fairy, FullMagic, RedRupee, Bomb8, Heart, RedRupee, Arrow10,         // pack 5
        Heart, Fairy, FullMagic, RedRupee, Bomb8, Heart, RedRupee, Arrow10,         // pack 6
        Heart, Fairy, FullMagic, RedRupee, Bomb8, Heart, RedRupee, Arrow10,         // pack 7
        Bomb4, Arrow10, FullMagic, // from pull trees
        RedRupee, RedRupee, // from prize crab
        Fairy, // stunned prize
        RedRupee, // saved fish prize
    };

    private static readonly IEnumerable<DropPrize> s_difficultPool = new[] {
        Heart, Heart, BlueRupee, BlueRupee, GreenRupee, BlueRupee, BlueRupee, GreenRupee,         // pack 1
        BlueRupee, GreenRupee, BlueRupee, RedRupee, BlueRupee, GreenRupee, BlueRupee, BlueRupee,                // pack 2
        Magic, Magic, Magic, BlueRupee, Magic, Magic, Heart, Magic,  // pack 3
        Bomb1, Bomb1, Bomb1, Bomb1, Bomb1, Bomb1, Bomb4, Bomb1,         // pack 4
        Arrow5, Heart, Arrow5, Arrow5, Arrow5, BlueRupee, Arrow5, Arrow5, // pack 5
        Magic, GreenRupee, Heart, Arrow5, Magic, Bomb1, GreenRupee, BlueRupee,        // pack 6
        Heart, RedRupee, Magic, RedRupee, Bomb4, BlueRupee, RedRupee, Arrow5,       // pack 7
        GreenRupee, BlueRupee, RedRupee, // from pull trees
        GreenRupee, RedRupee, // from prize crab
        GreenRupee, // stunned prize
        RedRupee, // saved fish prize
    };

    public static IEnumerable<DropPrize> GetPool(ZeldaDrops setting, Random? random = null)
    {
        switch (setting)
        {
            case ZeldaDrops.Vanilla:
                return DropPrizes.s_defaultPool;
            case ZeldaDrops.Easy:
                return s_easyPool;
            case ZeldaDrops.Difficult:
                return s_difficultPool;
            case ZeldaDrops.Randomized:
            default:
                {
                    random ??= new Random().Sanitize();
                    return s_defaultPool.Shuffle(random);
                }
        }
    }
}
