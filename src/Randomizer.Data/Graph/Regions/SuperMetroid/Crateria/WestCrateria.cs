using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.Data.Graph.Regions.SuperMetroid.Crateria
{
    public class WestCrateria : Region
    {
        public WestCrateria()
        {
            Rooms = new List<Room>
            {
                new Room(
                    name: "Gauntlet Energy Tank Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaGauntletEnergyTankRoomLeft, ExitType.BlueDoor },
                        { Exit.CrateriaGauntletEnergyTankRoomRight, ExitType.BlueDoor }
                    },
                    locations: new List<Location>
                    {
                        new Location(
                            id: LocationId.CrateriaGauntletEnergyTank,
                            name: "Energy Tank, Gauntlet",
                            vanillaItem: ItemType.ETank
                        )
                    }
                ),
                new Room(
                    name: "Gauntlet Entrance",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaGauntletEntranceLeft, ExitType.BlueDoor },
                        { Exit.CrateriaGauntletEntranceRight, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "Green Brinstar Elevator Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaGreenBrinstarElevatorRoomDown, ExitType.Elevator },
                        { Exit.CrateriaGreenBrinstarElevatorRoomRight, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "Green Pirates Shaft",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaGreenPiratesShaftTopRight, ExitType.BlueDoor },
                        { Exit.CrateriaGreenPiratesShaftMiddleRight, ExitType.BlueDoor },
                        { Exit.CrateriaGreenPiratesShaftBottomRight, ExitType.RedDoor },
                        { Exit.CrateriaGreenPiratesShaftLeft, ExitType.BlueDoor }
                    },
                    locations: new List<Location>
                    {
                        new Location(
                            id: LocationId.CrateriaGauntletShaftLeft,
                            name: "Gauntlet Shaft Left",
                            vanillaItem: ItemType.Missile
                        ),
                        new Location(
                            id: LocationId.CrateriaGauntletShaftRight,
                            name: "Gauntlet Shaft Right",
                            vanillaItem: ItemType.Missile
                        )
                    }
                ),
                new Room(
                    name: "Mushroom Kingdom",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaMushroomKingdomLeft, ExitType.BlueDoor },
                        { Exit.CrateriaMushroomKingdomRight, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "Statues Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaStatuesRoomLeft, ExitType.BlueDoor }, // kind of
                        { Exit.CrateriaStatuesRoomDown, ExitType.Elevator }
                    }
                ),
                new Room(
                    name: "Statues Hallway",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaStatuesHallwayLeft, ExitType.BlueDoor },
                        { Exit.CrateriaStatuesHallwayRight, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "Terminator Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaTerminatorRoomLeft, ExitType.BlueDoor },
                        { Exit.CrateriaTerminatorRoomRight, ExitType.BlueDoor }
                    },
                    locations: new List<Location>
                    {
                        new Location(
                            id: LocationId.CrateriaTerminator,
                            name: "Energy Tank, Terminator",
                            vanillaItem: ItemType.ETank
                        )
                    }
                )
            };

            Layout = new List<(Exit, Exit)>
            {
                ( Exit.CrateriaGreenBrinstarElevatorRoomRight, Exit.CrateriaMushroomKingdomLeft ),
                ( Exit.CrateriaMushroomKingdomRight, Exit.CrateriaGreenPiratesShaftLeft ),
                ( Exit.CrateriaGreenPiratesShaftTopRight, Exit.CrateriaGauntletEnergyTankRoomLeft ),
                ( Exit.CrateriaGreenPiratesShaftMiddleRight, Exit.CrateriaTerminatorRoomLeft ),
                ( Exit.CrateriaGreenPiratesShaftBottomRight, Exit.CrateriaStatuesHallwayLeft ),
                ( Exit.CrateriaStatuesHallwayRight, Exit.CrateriaStatuesRoomLeft ),
                ( Exit.CrateriaGauntletEnergyTankRoomRight, Exit.CrateriaGauntletEntranceLeft )
            };
        }
    }
}
