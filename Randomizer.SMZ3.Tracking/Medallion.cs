using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking
{
    public enum Medallion
    {
        None = 0,
        Bombos,
        Ether,
        Quake
    }

    public static class MedallionExtensions
    {
        public static ItemType ToItemType(this Medallion medallion) => medallion switch
        {
            Medallion.Bombos => ItemType.Bombos,
            Medallion.Ether => ItemType.Ether,
            Medallion.Quake => ItemType.Quake,
            _ => ItemType.Nothing
        };
    }
}
