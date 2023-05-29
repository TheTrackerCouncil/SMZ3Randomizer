using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Brinstar
{
    public class GreenBrinstar : SMRegion
    {
        public GreenBrinstar(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            Weight = -6;
            GreenBrinstarMainShaft = new GreenBrinstarMainShaftRoom(this, metadata, trackerState);
            EtecoonEnergyTank = new EtecoonEnergyTankRoom(this, metadata, trackerState);
            EtecoonSuper = new EtecoonSuperRoom(this, metadata, trackerState);
            MockballHall = new MockballHallRoom(this, metadata, trackerState);
            MockballHallHidden = new MockballHallHiddenRoom(this, metadata, trackerState);
            MemoryRegionId = 1;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Green Brinstar");
        }

        public override string Name => "Green Brinstar";

        public override string Area => "Brinstar";

        public GreenBrinstarMainShaftRoom GreenBrinstarMainShaft { get; }

        public EtecoonEnergyTankRoom EtecoonEnergyTank { get; }

        public EtecoonSuperRoom EtecoonSuper { get; }

        public MockballHallRoom MockballHall { get; }

        public MockballHallHiddenRoom MockballHallHidden { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return Logic.CanDestroyBombWalls(items) || Logic.CanParlorSpeedBoost(items);
        }

        public class GreenBrinstarMainShaftRoom : Room
        {
            public GreenBrinstarMainShaftRoom(GreenBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Green Brinstar Main Shaft", metadata)
            {
                PowerBomb = new Location(this, 13, 0x8F84AC, LocationType.Chozo,
                    name: "Power Bomb (green Brinstar bottom)",
                    access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items)
                                  && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                    memoryAddress: 0x1,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location PowerBomb { get; }
        }

        public class EtecoonEnergyTankRoom : Room
        {
            public EtecoonEnergyTankRoom(GreenBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Etecoon Energy Tank Room", metadata)
            {
                ETank = new Location(this, 30, 0x8F87C2, LocationType.Visible,
                    name: "Energy Tank, Etecoons",
                    vanillaItem: ItemType.ETank,
                    access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items)
                                  && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                    memoryAddress: 0x3,
                    memoryFlag: 0x40,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location ETank { get; }
        }

        public class EtecoonSuperRoom : Room
        {
            public EtecoonSuperRoom(GreenBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Etecon Super Room", metadata)
            {
                BottomSuperMissile = new Location(this, 31, 0x8F87D0, LocationType.Visible,
                    name: "Super Missile (green Brinstar bottom)",
                    vanillaItem: ItemType.Super,
                    access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items) && items.Super
                                  && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                    memoryAddress: 0x3,
                    memoryFlag: 0x80,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location BottomSuperMissile { get; }
        }

        public class MockballHallRoom : Room
        {
            public MockballHallRoom(GreenBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Mockball Hall", metadata, "Early Supers Room")
            {
                MissileBelowSuperMissile = new Location(this, 15, 0x8F8518, LocationType.Visible,
                    name: "Missile (green Brinstar below super missile)",
                    vanillaItem: ItemType.Missile,
                    access: items => Logic.CanPassBombPassages(items) && Logic.CanOpenRedDoors(items),
                    memoryAddress: 0x1,
                    memoryFlag: 0x80,
                    metadata: metadata,
                    trackerState: trackerState);

                TopSuperMissile = new Location(this, 16, 0x8F851E, LocationType.Visible,
                    name: "Super Missile (green Brinstar top)",
                    vanillaItem: ItemType.Super,
                    access: items => Logic.CanOpenRedDoors(items) && Logic.CanMoveAtHighSpeeds(items),
                    memoryAddress: 0x2,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location MissileBelowSuperMissile { get; }

            public Location TopSuperMissile { get; }
        }

        public class MockballHallHiddenRoom : Room
        {
            public MockballHallHiddenRoom(GreenBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Mockball Hall Hidden Room", metadata, "Brinstar Reserve Tank Room")
            {
                ReserveTank = new Location(this, 17, 0x8F852C, LocationType.Chozo,
                    name: "Reserve Tank, Brinstar",
                    vanillaItem: ItemType.ReserveTank,
                    access: CanEnter,
                    memoryAddress: 0x2,
                    memoryFlag: 0x2,
                    metadata: metadata,
                    trackerState: trackerState);

                HiddenItem = new Location(this, 18, 0x8F8532, LocationType.Hidden,
                    name: "Hidden Item",
                    vanillaItem: ItemType.Missile,
                    access: items => CanEnter(items) && Logic.CanPassBombPassages(items),
                    memoryAddress: 0x2,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);

                MainItem = new Location(this, 19, 0x8F8538, LocationType.Visible,
                    name: "Main Item",
                    vanillaItem: ItemType.Missile,
                    access: items => CanEnter(items) && items.Morph,
                    memoryAddress: 0x2,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location ReserveTank { get; }

            public Location MainItem { get; }

            public Location HiddenItem { get; }

            public bool CanEnter(Progression items)
            {
                return Logic.CanOpenRedDoors(items) && Logic.CanMoveAtHighSpeeds(items);
            }
        }
    }

}
