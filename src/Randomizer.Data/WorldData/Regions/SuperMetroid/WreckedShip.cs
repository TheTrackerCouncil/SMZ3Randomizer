using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid
{
    public class WreckedShip : SMRegion, IHasBoss
    {
        public WreckedShip(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            MainShaft = new MainShaftRoom(this, metadata, trackerState);
            BowlingAlley = new BowlingAlleyRoom(this, metadata, trackerState);
            AssemblyLine = new AssemblyLineRoom(this, metadata, trackerState);
            EnergyTank = new EnergyTankRoom(this, metadata, trackerState);
            WestSuper = new WestSuperRoom(this, metadata, trackerState);
            EastSuper = new EastSuperRoom(this, metadata, trackerState);
            GravitySuit = new GravitySuitRoom(this, metadata, trackerState);
            MemoryRegionId = 3;
            Boss = new Boss(Shared.Enums.BossType.Phantoon, world, this, metadata, trackerState);
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Wrecked Ship");
        }

        public override string Name => "Wrecked Ship";

        public override string Area => "Wrecked Ship";

        public Boss Boss { get; set; }

        public MainShaftRoom MainShaft { get; }

        public BowlingAlleyRoom BowlingAlley { get; }

        public AssemblyLineRoom AssemblyLine { get; }

        public EnergyTankRoom EnergyTank { get; }

        public WestSuperRoom WestSuper { get; }

        public EastSuperRoom EastSuper { get; }

        public GravitySuitRoom GravitySuit { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.Super && (
                        /* Over the Moat */
                        ((Config.MetroidKeysanity ? items.CardCrateriaL2 : Logic.CanUsePowerBombs(items)) && (
                            Logic.CanMoatSpeedBoost(items) || items.Grapple || items.SpaceJump ||
                            (items.Gravity && (Logic.CanIbj(items) || (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy))))
                            || Logic.CanWallJump(WallJumpDifficulty.Insane)
                        )) ||
                        /* Through Maridia -> Forgotten Highway */
                        (Logic.CanUsePowerBombs(items) && CanPassReverseForgottenHighway(items)) ||
                        /* From Maridia portal -> Forgotten Highway */
                        (Logic.CanAccessMaridiaPortal(items, requireRewards) && CanPassReverseForgottenHighway(items) && (
                            (Logic.CanDestroyBombWalls(items) && items.CardMaridiaL2) ||
                            World.InnerMaridia.SpaceJump.DraygonTreasure.IsAvailable(items)
                        ))
                    );
        }

        public bool CanAccessShutDownRooms(Progression items, bool requireRewards) =>
            items.Phantoon || (!requireRewards && CanUnlockShip(items));

        private bool CanViewConcert(Progression items, bool requireRewards) =>
            CanAccessShutDownRooms(items, requireRewards) && items.CardWreckedShipL1 &&
            (items.Grapple || items.SpaceJump || (items.Varia && Logic.HasEnergyReserves(items, 2)) || Logic.HasEnergyReserves(items, 3));

        private bool CanAccessWreckedPool(Progression items, bool requireRewards) =>
            CanAccessShutDownRooms(items, requireRewards) &&
            ((items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Easy)) || items.SpaceJump || items.SpeedBooster || items.Gravity);

        public bool CanPassReverseForgottenHighway(Progression items)
        {
            return items.Gravity && (
                Logic.CanFly(items) ||
                Logic.CanWallJump(WallJumpDifficulty.Easy) ||
                (items.HiJump && items.Ice)
            );
        }

        public bool CanBeatBoss(Progression items) => CanEnter(items, true) && CanUnlockShip(items);

        public bool CanUnlockShip(Progression items)
        {
            return items.CardWreckedShipBoss && Logic.CanPassBombPassages(items);
        }

        public class MainShaftRoom : Room
        {
            public MainShaftRoom(WreckedShip region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Wrecked Ship Main Shaft", metadata)
            {
                MainShaftSideRoom = new Location(this, LocationId.WreckedShipMainShaft, 0x8FC265, LocationType.Visible,
                    name: "Missile (Wrecked Ship middle)",
                    vanillaItem: ItemType.Missile,
                    access: items => Logic.CanPassBombPassages(items),
                    memoryAddress: 0x10,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location MainShaftSideRoom { get; }
        }

        public class BowlingAlleyRoom : Room
        {
            public BowlingAlleyRoom(WreckedShip region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Bowling Alley", metadata, "Post Chozo Concert")
            {
                PostChozoConcertSpeedBoosterItem = new Location(this, LocationId.WreckedShipBowlingAlleyTop, 0x8FC2E9, LocationType.Chozo,
                    name: "Reserve Tank, Wrecked Ship",
                    vanillaItem: ItemType.ReserveTank,
                    access: items => region.CanViewConcert(items, requireRewards: true) && items.SpeedBooster && Logic.CanUsePowerBombs(items),
                    relevanceRequirement: items => region.CanViewConcert(items, requireRewards: false) && items.SpeedBooster && Logic.CanUsePowerBombs(items),
                    memoryAddress: 0x10,
                    memoryFlag: 0x2,
                    metadata: metadata,
                    trackerState: trackerState);
                PostChozoConcertBreakableChozo = new Location(this, LocationId.WreckedShipBowlingAlleyBottom, 0x8FC2EF, LocationType.Visible,
                    name: "Missile (Gravity Suit)",
                    vanillaItem: ItemType.Missile,
                    access: items => region.CanViewConcert(items, requireRewards: true),
                    relevanceRequirement: items => region.CanViewConcert(items, requireRewards: false),
                    memoryAddress: 0x10,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location PostChozoConcertSpeedBoosterItem { get; }

            public Location PostChozoConcertBreakableChozo { get; }
        }

        public class AssemblyLineRoom : Room
        {
            public AssemblyLineRoom(WreckedShip region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Assembly Line", metadata)
            {
                AtticAssemblyLine = new Location(this, LocationId.WreckedShipAssemblyLine, 0x8FC319, LocationType.Visible,
                    name: "Missile (Wrecked Ship top)",
                    vanillaItem: ItemType.Missile,
                    access: items => region.CanAccessShutDownRooms(items, requireRewards: true),
                    relevanceRequirement: items => region.CanAccessShutDownRooms(items, requireRewards: false),
                    memoryAddress: 0x10,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location AtticAssemblyLine { get; }
        }

        public class EnergyTankRoom : Room
        {
            public EnergyTankRoom(WreckedShip region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Wrecked Ship Energy Tank Room", metadata, "Wrecked Pool")
            {
                WreckedPool = new Location(this, LocationId.WreckedShipEnergyTank, 0x8FC337, LocationType.Visible,
                    name: "Energy Tank, Wrecked Ship",
                    vanillaItem: ItemType.ETank,
                    access: items => region.CanAccessWreckedPool(items, requireRewards: true),
                    relevanceRequirement: items => region.CanAccessWreckedPool(items, requireRewards: false),
                    memoryAddress: 0x10,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location WreckedPool { get; }
        }

        public class WestSuperRoom : Room
        {
            public WestSuperRoom(WreckedShip region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Wrecked Ship West Super Room", metadata)
            {
                LeftSuperMissileChamber = new Location(this, LocationId.WreckedShipWestSuper, 0x8FC357, LocationType.Visible,
                    name: "Super Missile (Wrecked Ship left)",
                    vanillaItem: ItemType.Super,
                    access: items => region.CanAccessShutDownRooms(items, requireRewards: true),
                    relevanceRequirement: items => region.CanAccessShutDownRooms(items, requireRewards: false),
                    memoryAddress: 0x10,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location LeftSuperMissileChamber { get; }
        }

        public class EastSuperRoom : Room
        {
            public EastSuperRoom(WreckedShip region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Wrecked Ship East Super Room", metadata)
            {
                RightSuperMissileChamber = new Location(this, LocationId.WreckedShipEastSuper, 0x8FC365, LocationType.Visible,
                    name: "Right Super, Wrecked Ship",
                    vanillaItem: ItemType.Super,
                    access: items => region.CanAccessShutDownRooms(items, requireRewards: true),
                    relevanceRequirement: items => region.CanAccessShutDownRooms(items, requireRewards: false),
                    memoryAddress: 0x10,
                    memoryFlag: 0x40,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location RightSuperMissileChamber { get; }
        }

        public class GravitySuitRoom : Room
        {
            public GravitySuitRoom(WreckedShip region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Gravity Suit Room", metadata)
            {
                PostChozoConcertGravitySuitChamber = new Location(this, LocationId.WreckedShipGravitySuit, 0x8FC36D, LocationType.Chozo,
                    name: "Gravity Suit",
                    vanillaItem: ItemType.Gravity,
                    access: items => region.CanViewConcert(items, requireRewards: true),
                    relevanceRequirement: items => region.CanViewConcert(items, requireRewards: false),
                    memoryAddress: 0x10,
                    memoryFlag: 0x80,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location PostChozoConcertGravitySuitChamber { get; }
        }
    }
}
