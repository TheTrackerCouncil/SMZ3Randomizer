using System.Collections.Generic;

namespace TrackerCouncil.Smz3.Data.Graph.Regions.SuperMetroid.Crateria;

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
