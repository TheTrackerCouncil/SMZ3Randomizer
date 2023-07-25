using System.Collections.Generic;

namespace Randomizer.Data.Graph.Regions.SuperMetroid.Crateria
{
    public class Crateria : Area
    {
        public Crateria()
        {
            Regions = new List<Region>
            {
                new WestCrateria(),
                new CentralCrateria()
            };

            Layout = new List<(Exit, Exit)>
            {
                ( Exit.CrateriaLandingSiteTopLeft, Exit.CrateriaGauntletEntranceRight ),
                ( Exit.CrateriaParlorAndAlcatrazTopLeft, Exit.CrateriaTerminatorRoomRight )
            };
        }
    }
}
