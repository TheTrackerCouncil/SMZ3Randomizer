using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData;

namespace Randomizer.SMZ3.Contracts
{
    public interface IGameHintGenerator
    {
        IEnumerable<string> GetHints(World world, ICollection<World> allWorlds, Playthrough playthrough, int hintCount, int seed);
    }
}
