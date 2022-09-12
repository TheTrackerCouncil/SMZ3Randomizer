using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Data.Options
{
    public record ShipSprite(string DisplayName, string FileName)
    {
        public static readonly ShipSprite DefaultShip = new("Default", null);
    }
}
