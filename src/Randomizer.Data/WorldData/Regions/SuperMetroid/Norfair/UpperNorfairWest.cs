using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Norfair
{

    public class UpperNorfairWest : SMRegion
    {
        public UpperNorfairWest(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            Cathedral = new CathedralRoom(this, metadata, trackerState);
            IceBeam = new IceBeamRoom(this, metadata, trackerState);
            CrumbleShaft = new CrumbleShaftRoom(this, metadata, trackerState);
            HiJumpBoots = new HiJumpBootsRoom(this, metadata, trackerState);
            HiJumpEnergyTank = new HiJumpEnergyTankRoom(this, metadata, trackerState);
            MemoryRegionId = 2;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Upper Norfair West");
        }
        public override string Name => "Upper Norfair, West";
        public override string Area => "Upper Norfair";

        public CathedralRoom Cathedral { get; }

        public IceBeamRoom IceBeam { get; }

        public CrumbleShaftRoom CrumbleShaft { get; }

        public HiJumpBootsRoom HiJumpBoots { get; }

        public HiJumpEnergyTankRoom HiJumpEnergyTank { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return (Logic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph ||
                Logic.CanAccessNorfairUpperPortal(items);
        }

        public class CathedralRoom : Room
        {
            public CathedralRoom(UpperNorfairWest region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Cathedral", metadata, "Lava Room")
            {
                LavaRoom = new Location(this, 49, 0x8F8AE4, LocationType.Hidden,
                    name: "Missile (lava room)",
                    vanillaItem: ItemType.Missile,
                    access: items => items.Varia && (
                                (Logic.CanOpenRedDoors(items) && (Logic.CanFly(items) || items.HiJump || items.SpeedBooster)) ||
                                (World.UpperNorfairEast.CanEnter(items, true) && items.CardNorfairL2)
                            ) && items.Morph,
                    memoryAddress: 0x6,
                    memoryFlag: 0x2,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location LavaRoom { get; }
        }

        public class IceBeamRoom : Room
        {
            public IceBeamRoom(UpperNorfairWest region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Ice Beam Room", metadata)
            {
                IceBeam = new Location(this, 50, 0x8F8B24, LocationType.Chozo,
                    name: "Ice Beam",
                    vanillaItem: ItemType.Ice,
                    access: items => (Config.MetroidKeysanity ? items.CardNorfairL1 : items.Super)
                                     && Logic.CanPassBombPassages(items) && items.Varia && Logic.CanMoveAtHighSpeeds(items),
                    memoryAddress: 0x6,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location IceBeam { get; }
        }

        public class CrumbleShaftRoom : Room
        {
            public CrumbleShaftRoom(UpperNorfairWest region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Crumble Shaft Room", metadata)
            {
                CrumbleShaft = new Location(this, 51, 0x8F8B46, LocationType.Hidden,
                    name: "Missile (below Ice Beam)",
                    vanillaItem: ItemType.Missile,
                    access: items => (Config.MetroidKeysanity ? items.CardNorfairL1 : items.Super)
                                     && Logic.CanUsePowerBombs(items) && items.Varia && Logic.CanMoveAtHighSpeeds(items)
                                     && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items)),
                    memoryAddress: 0x6,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location CrumbleShaft { get; }
        }

        public class HiJumpBootsRoom : Room
        {
            public HiJumpBootsRoom(UpperNorfairWest region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Hi-Jump Boots Room", metadata)
            {
                HiJumpBoots = new Location(this, 53, 0x8F8BAC, LocationType.Chozo,
                    name: "Hi-Jump Boots",
                    vanillaItem: ItemType.HiJump,
                    access: items => Logic.CanOpenRedDoors(items) && Logic.CanPassBombPassages(items),
                    memoryAddress: 0x6,
                    memoryFlag: 0x20,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location HiJumpBoots { get; }
        }

        public class HiJumpEnergyTankRoom : Room
        {
            public HiJumpEnergyTankRoom(UpperNorfairWest region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Hi-Jump Energy Tank Room", metadata, "Hi-Jump Lobby")
            {
                HiJumpLobbyBack = new Location(this, 55, 0x8F8BE6, LocationType.Visible,
                    name: "Missile (Hi-Jump Boots)",
                    vanillaItem: ItemType.Missile,
                    access: items => Logic.CanOpenRedDoors(items) && items.Morph,
                    memoryAddress: 0x6,
                    memoryFlag: 0x80,
                    metadata: metadata,
                    trackerState: trackerState);
                HiJumpLobbyEntrance = new Location(this, 56, 0x8F8BEC, LocationType.Visible,
                    name: "Energy Tank (Hi-Jump Boots)",
                    vanillaItem: ItemType.ETank,
                    access: items => Logic.CanOpenRedDoors(items),
                    memoryAddress: 0x7,
                    memoryFlag: 0x1,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location HiJumpLobbyBack { get; }

            public Location HiJumpLobbyEntrance { get; }
        }
    }
}
