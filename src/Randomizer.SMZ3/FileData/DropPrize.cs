using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static System.Linq.Enumerable;
using Randomizer.SMZ3.Text;
using static Randomizer.SMZ3.FileData.DropPrize;

namespace Randomizer.SMZ3.FileData
{

    public enum DropPrize : byte
    {
        Heart = 0xD8,
        Green = 0xD9,
        Blue = 0xDA,
        Red = 0xDB,
        Bomb1 = 0xDC,
        Bomb4 = 0xDD,
        Bomb8 = 0xDE,
        Magic = 0xDF,
        FullMagic = 0xE0,
        Arrow5 = 0xE1,
        Arrow10 = 0xE2,
        Fairy = 0xE3,
    }

}
