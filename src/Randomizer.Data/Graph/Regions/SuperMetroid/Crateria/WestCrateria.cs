namespace Randomizer.Data.Graph.Regions.SuperMetroid.Crateria
{
    public class WestCrateria
    {
        public WestCrateria()
        {
            Rooms = new List<Room>
            {
                new Room(
                    name: "Gauntlet Energy Tank",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaGauntletEnergyTankLeft, ExitType.BlueDoor },
                        { Exit.CrateriaGauntletEnergyTankRight, ExitType.BlueDoor }
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
                    name: "Green Brinstar Elevator",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaGreenBrinstarElevatorDown, ExitType.Elevator },
                        { Exit.CrateriaGreenBrinstarElevatorRight, ExitType.BlueDoor }
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
                            vanillaItem: ItemType.Missiles
                        ),
                        new Location(
                            id: LocationId.CrateriaGauntletShaftRight,
                            name: "Gauntlet Shaft Right",
                            vanillaItem: ItemType.Missiles
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
                    name: "Statues",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaStatuesLeft, ExitType.BlueDoor }, // kind of
                        { Exit.CrateriaStatuesDown, ExitType.Elevator }
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
                    name: "Terminator",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaTerminatorLeft, ExitType.BlueDoor },
                        { Exit.CrateriaTerminatorRight, ExitType.BlueDoor }
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

            Layout = new List<Tuple<Exit, Exit>>
            {
                ( Exit.CrateriaGreenBrinstarElevatorRight, Exit.CrateriaMushroomKingdomLeft ),
                ( Exit.CrateriaMushroomKingdomRight, Exit.CrateriaGreenPiratesShaftLeft ),
                ( Exit.CrateriaGreenPiratesShaftTopRight, Exit.CrateriaGauntletEnergyTankLeft ),
                ( Exit.CrateriaGreenPiratesShaftMiddleRight, Exit.CrateriaTerminatorLeft ),
                ( Exit.CrateriaGreenPiratesShaftBottomRight, Exit.CrateriaStatuesHallwayLeft ),
                ( Exit.CrateriaStatuesHallwayRight, Exit.CrateriaStatuesLeft ),
                ( Exit.CrateriaGauntletEnergyTankRight, Exit.CrateriaGauntletEntranceLeft )
            };
        }
    }
}
