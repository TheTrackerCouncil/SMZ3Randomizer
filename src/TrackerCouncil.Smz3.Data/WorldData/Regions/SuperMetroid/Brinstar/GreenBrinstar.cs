using System.Collections.Generic;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData.Regions.SuperMetroid.Brinstar;

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
        MapName = "Brinstar";
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
            Locations = new List<Location>
            {
                new Location(this, LocationId.GreenBrinstarMainShaft, 0x8F84AC, LocationType.Chozo,
                    name: "Power Bomb (green Brinstar bottom)",
                    access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items)
                                                          && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                    memoryAddress: 0x1,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class EtecoonEnergyTankRoom : Room
    {
        public EtecoonEnergyTankRoom(GreenBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Etecoon Energy Tank Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.GreenBrinstarEtecoonEnergyTank, 0x8F87C2, LocationType.Visible,
                    name: "Energy Tank, Etecoons",
                    vanillaItem: ItemType.ETank,
                    access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items)
                                                          && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                    memoryAddress: 0x3,
                    memoryFlag: 0x40,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class EtecoonSuperRoom : Room
    {
        public EtecoonSuperRoom(GreenBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Etecon Super Room", metadata)
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.GreenBrinstarEtecoonSuper, 0x8F87D0, LocationType.Visible,
                    name: "Super Missile (green Brinstar bottom)",
                    vanillaItem: ItemType.Super,
                    access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items) && items.Super
                                     && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                    memoryAddress: 0x3,
                    memoryFlag: 0x80,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class MockballHallRoom : Room
    {
        public MockballHallRoom(GreenBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Mockball Hall", metadata, "Early Supers Room")
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.GreenBrinstarEarlySupersBottom, 0x8F8518, LocationType.Visible,
                    name: "Missile (green Brinstar below super missile)",
                    vanillaItem: ItemType.Missile,
                    access: items => Logic.CanPassBombPassages(items) && Logic.CanOpenRedDoors(items),
                    memoryAddress: 0x1,
                    memoryFlag: 0x80,
                    metadata: metadata,
                    trackerState: trackerState),

                new Location(this, LocationId.GreenBrinstarEarlySupersTop, 0x8F851E, LocationType.Visible,
                    name: "Super Missile (green Brinstar top)",
                    vanillaItem: ItemType.Super,
                    access: items => Logic.CanOpenRedDoors(items) && Logic.CanMoveAtHighSpeeds(items),
                    memoryAddress: 0x2,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }
    }

    public class MockballHallHiddenRoom : Room
    {
        public MockballHallHiddenRoom(GreenBrinstar region, IMetadataService? metadata, TrackerState? trackerState)
            : base(region, "Mockball Hall Hidden Room", metadata, "Brinstar Reserve Tank Room")
        {
            Locations = new List<Location>
            {
                new Location(this, LocationId.GreenBrinstarReserveTankChozo, 0x8F852C, LocationType.Chozo,
                    name: "Reserve Tank, Brinstar",
                    vanillaItem: ItemType.ReserveTank,
                    access: CanEnter,
                    memoryAddress: 0x2,
                    memoryFlag: 0x2,
                    metadata: metadata,
                    trackerState: trackerState),

                new Location(this, LocationId.GreenBrinstarReserveTankHidden, 0x8F8532, LocationType.Hidden,
                    name: "Hidden Item",
                    vanillaItem: ItemType.Missile,
                    access: items => CanEnter(items) && Logic.CanPassBombPassages(items),
                    memoryAddress: 0x2,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState),

                new Location(this, LocationId.GreenBrinstarReserveTankVisible, 0x8F8538, LocationType.Visible,
                    name: "Main Item",
                    vanillaItem: ItemType.Missile,
                    access: items => CanEnter(items) && items.Morph,
                    memoryAddress: 0x2,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState)
            };
        }

        public bool CanEnter(Progression items)
        {
            return Logic.CanOpenRedDoors(items) && Logic.CanMoveAtHighSpeeds(items);
        }
    }
}
