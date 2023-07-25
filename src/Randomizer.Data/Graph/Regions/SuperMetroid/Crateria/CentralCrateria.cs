using System.Collections.Generic;
using Randomizer.Shared;

namespace Randomizer.Data.Graph.Regions.SuperMetroid.Crateria
{
    public class CentralCrateria : Region
    {
        public CentralCrateria()
        {
            Rooms = new List<Room>
            {
                new Room(
                    name: "Parlor and Alcatraz",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaParlorAndAlcatrazTopLeft, ExitType.BlueDoor },
                        { Exit.CrateriaParlorAndAlcatrazMiddleLeft, ExitType.BlueDoor },
                        { Exit.CrateriaParlorAndAlcatrazBottomLeft, ExitType.BlueDoor },
                        { Exit.CrateriaParlorAndAlcatrazDown, ExitType.BlueDoor },
                        { Exit.CrateriaParlorAndAlcatrazTopRight, ExitType.BlueDoor },
                        { Exit.CrateriaParlorAndAlcatrazMiddleRight, ExitType.BlueDoor },
                        { Exit.CrateriaParlorAndAlcatrazBottomRight, ExitType.RedDoor }
                    }
                ),
                new Room(
                    name: "Crateria Save Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaSaveRoomRight, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "The Final Missile",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaTheFinalMissileRight, ExitType.BlueDoor }
                    },
                    locations: new List<Location>
                    {
                        new Location(
                            id: LocationId.CrateriaFinalMissile,
                            name: "Missile (Crateria middle)",
                            vanillaItem: ItemType.Missile
                        )
                    }
                ),
                new Room(
                    name: "Final Missile Bombway",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaFinalMissileBombwayLeft, ExitType.BlueDoor },
                        { Exit.CrateriaFinalMissileBombwayRight, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "Flyway",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaFlywayLeft, ExitType.BlueDoor },
                        { Exit.CrateriaFlywayRight, ExitType.RedDoor }
                    }
                ),
                new Room(
                    name: "Bomb Torizo Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaBombTorizoRoomLeft, ExitType.EnemyDoor }
                    },
                    locations: new List<Location>
                    {
                        new Location(
                            id: LocationId.CrateriaBombTorizo,
                            name: "Bomb Torizo",
                            vanillaItem: ItemType.Bombs
                        )
                    }
                ),
                new Room(
                    name: "Pre-Map Flyway",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaPreMapFlywayLeft, ExitType.BlueDoor },
                        { Exit.CrateriaPreMapFlywayRight, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "Crateria Map Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaMapRoomLeft, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "Climb",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaClimbUp, ExitType.BlueDoor },
                        { Exit.CrateriaClimbLeft, ExitType.GrayDoor },
                        { Exit.CrateriaClimbTopRight, ExitType.GrayDoor },
                        { Exit.CrateriaClimbMiddleRight, ExitType.YellowDoor },
                        { Exit.CrateriaClimbBottomRight, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "Crateria Super Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaSuperRoomTopLeft, ExitType.BlueDoor },
                        { Exit.CrateriaSuperRoomBottomLeft, ExitType.BlueDoor }
                    }
                ),
                new Room(
                    name: "Pit Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        // In vanilla, this is a gray door until you have the Morph Ball.
                        { Exit.CrateriaPitRoomLeft, ExitType.EnemyDoor },
                        // In vanilla, this is a blue door until you have the Morph Ball.
                        { Exit.CrateriaPitRoomRight, ExitType.EnemyDoor }
                    },
                    locations: new List<Location>
                    {
                        new Location(
                            id: LocationId.CrateriaPit,
                            name: "Missile (Crateria bottom)",
                            vanillaItem: ItemType.Missile
                        )
                    }
                ),
                new Room(
                    name: "Blue Brinstar Elevator Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaBlueBrinstarElevatorRoomLeft, ExitType.BlueDoor },
                        { Exit.CrateriaBlueBrinstarElevatorRoomDown, ExitType.Elevator }
                    }
                ),
                new Room(
                    name: "Landing Site",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaLandingSiteTopLeft, ExitType.BlueDoor },
                        { Exit.CrateriaLandingSiteBottomLeft, ExitType.BlueDoor },
                        { Exit.CrateriaLandingSiteTopRight, ExitType.YellowDoor },
                        { Exit.CrateriaLandingSiteBottomRight, ExitType.GreenDoor }
                    }
                ),
                new Room(
                    name: "Crateria Power Bomb Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaPowerBombRoomLeft, ExitType.BlueDoor }
                    },
                    locations: new List<Location>
                    {
                        new Location(
                            id: LocationId.CrateriaPowerBomb,
                            name: "Power Bomb (Crateria surface)",
                            vanillaItem: ItemType.PowerBomb
                        )
                    }
                ),
                new Room(
                    name: "Crateria Tube",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaTubeLeft, ExitType.Open },
                        { Exit.CrateriaTubeRight, ExitType.Open }
                    }
                ),
                new Room(
                    name: "Crateria Kihunter Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaKihunterRoomLeft, ExitType.BlueDoor },
                        { Exit.CrateriaKihunterRoomRight, ExitType.YellowDoor },
                        { Exit.CrateriaKihunterRoomDown, ExitType.YellowDoor }
                    }
                ),
                new Room(
                    name: "Red Brinstar Elevator Room",
                    exits: new Dictionary<Exit, ExitType>
                    {
                        { Exit.CrateriaRedBrinstarElevatorRoomUp, ExitType.BlueDoor },
                        { Exit.CrateriaRedBrinstarElevatorRoomDown, ExitType.Elevator }
                    }
                )
            };

            Layout = new List<(Exit, Exit)>
            {
                ( Exit.CrateriaParlorAndAlcatrazMiddleLeft, Exit.CrateriaSaveRoomRight ),
                ( Exit.CrateriaParlorAndAlcatrazBottomLeft, Exit.CrateriaFinalMissileBombwayRight ),
                ( Exit.CrateriaFinalMissileBombwayLeft, Exit.CrateriaTheFinalMissileRight ),
                ( Exit.CrateriaParlorAndAlcatrazDown, Exit.CrateriaClimbUp ),
                ( Exit.CrateriaClimbTopRight, Exit.CrateriaSuperRoomTopLeft ),
                ( Exit.CrateriaClimbMiddleRight, Exit.CrateriaSuperRoomBottomLeft ),
                ( Exit.CrateriaClimbBottomRight, Exit.CrateriaPitRoomLeft ),
                ( Exit.CrateriaPitRoomRight, Exit.CrateriaBlueBrinstarElevatorRoomLeft ),
                ( Exit.CrateriaParlorAndAlcatrazBottomRight, Exit.CrateriaPreMapFlywayLeft ),
                ( Exit.CrateriaPreMapFlywayRight, Exit.CrateriaMapRoomLeft ),
                ( Exit.CrateriaParlorAndAlcatrazMiddleRight, Exit.CrateriaFlywayLeft ),
                ( Exit.CrateriaFlywayRight, Exit.CrateriaBombTorizoRoomLeft ),
                ( Exit.CrateriaParlorAndAlcatrazTopRight, Exit.CrateriaLandingSiteBottomLeft ),
                ( Exit.CrateriaLandingSiteTopRight, Exit.CrateriaPowerBombRoomLeft ),
                ( Exit.CrateriaLandingSiteBottomRight, Exit.CrateriaTubeLeft ),
                ( Exit.CrateriaTubeRight, Exit.CrateriaKihunterRoomLeft ),
                ( Exit.CrateriaKihunterRoomDown, Exit.CrateriaRedBrinstarElevatorRoomUp )
            };
        }
    }
}
