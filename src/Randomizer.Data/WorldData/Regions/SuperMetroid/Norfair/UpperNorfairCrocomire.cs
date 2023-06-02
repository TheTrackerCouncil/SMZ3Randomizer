using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Norfair
{
    public class UpperNorfairCrocomire : SMRegion
    {
        public UpperNorfairCrocomire(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            Crocomires = new CrocomiresRoom(this, metadata, trackerState);
            CrocomireEscape = new CrocomireEscapeRoom(this, metadata, trackerState);
            PostCrocomirePowerBomb = new PostCrocomirePowerBombRoom(this, metadata, trackerState);
            PostCrocomireMissile = new PostCrocomireMissileRoom(this, metadata, trackerState);
            PostCrocomireJump = new PostCrocomireJumpRoom(this, metadata, trackerState);
            GrappleBeam = new GrappleBeamRoom(this, metadata, trackerState);
            MemoryRegionId = 2;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Upper Norfair Crocomire");
        }

        public override string Name => "Upper Norfair, Crocomire";
        public override string Area => "Upper Norfair";

        public CrocomiresRoom Crocomires { get; }

        public CrocomireEscapeRoom CrocomireEscape { get; }

        public PostCrocomirePowerBombRoom PostCrocomirePowerBomb { get; }

        public PostCrocomireMissileRoom PostCrocomireMissile { get; }

        public PostCrocomireJumpRoom PostCrocomireJump { get; }

        public GrappleBeamRoom GrappleBeam { get; }

        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return (
                        ((Logic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph) ||
                        Logic.CanAccessNorfairUpperPortal(items)
                    ) &&
                    items.Varia && (
                        /* Ice Beam -> Croc Speedway */
                        ((Config.MetroidKeysanity ? items.CardNorfairL1 : items.Super) && Logic.CanUsePowerBombs(items) && items.SpeedBooster) ||
                        /* Frog Speedway */
                        (items.SpeedBooster && items.Wave) ||
                        /* Cathedral -> through the floor or Vulcano */
                        (Logic.CanOpenRedDoors(items) && (Config.MetroidKeysanity ? items.CardNorfairL2 : items.Super) &&
                            (Logic.CanFly(items) || items.HiJump || items.SpeedBooster) &&
                            (Logic.CanPassBombPassages(items) || (items.Gravity && items.Morph)) && items.Wave)
                        ||
                        /* Reverse Lava Dive */
                        (Logic.CanAccessNorfairLowerPortal(items) && items.ScrewAttack && items.SpaceJump && items.Super &&
                        items.Gravity && items.Wave && (items.CardNorfairL2 || items.Morph))
                      );
        }

        private bool CanAccessCrocomire(Progression items)
        {
            return (Config.MetroidKeysanity ? items.CardNorfairBoss : items.Super) && (items.HiJump || items.SpeedBooster || Logic.CanFly(items) || Logic.CanWallJump(WallJumpDifficulty.Easy));
        }

        public class CrocomiresRoom : Room
        {
            public CrocomiresRoom(UpperNorfairCrocomire region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Crocomire's Room", metadata)
            {
                Crocomire = new Location(this, LocationId.Crocomire, 0x8F8BA4, LocationType.Visible,
                    name: "Energy Tank, Crocomire",
                    vanillaItem: ItemType.ETank,
                    access: items => region.CanAccessCrocomire(items) && ((Logic.HasEnergyReserves(items, 1) && Logic.CanWallJump(WallJumpDifficulty.Easy)) || items.SpaceJump || items.Grapple),
                    memoryAddress: 0x6,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location Crocomire { get; }
        }

        public class CrocomireEscapeRoom : Room
        {
            public CrocomireEscapeRoom(UpperNorfairCrocomire region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Crocomire Escape", metadata)
            {
                CrocomireEscape = new Location(this, LocationId.CrocomireEscape, 0x8F8BC0, LocationType.Visible,
                    name: "Missile (above Crocomire)",
                    vanillaItem: ItemType.Missile,
                    access: items => Logic.CanFly(items) || items.Grapple || (items.HiJump && items.SpeedBooster && Logic.CanWallJump(WallJumpDifficulty.Hard)),
                    memoryAddress: 0x6,
                    memoryFlag: 0x40,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location CrocomireEscape { get; }
        }

        public class PostCrocomirePowerBombRoom : Room
        {
            public PostCrocomirePowerBombRoom(UpperNorfairCrocomire region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Post Crocomire Power Bomb Room", metadata)
            {
                PostCrocPowerBombRoom = new Location(this, LocationId.PostCrocomirePowerBomb, 0x8F8C04, LocationType.Visible,
                    name: "Power Bomb (Crocomire)",
                    vanillaItem: ItemType.PowerBomb,
                    access: items => region.CanAccessCrocomire(items) && (Logic.CanFly(items) || items.HiJump || items.Grapple),
                    memoryAddress: 0x7,
                    memoryFlag: 0x2,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location PostCrocPowerBombRoom { get; }
        }

        public class PostCrocomireMissileRoom : Room
        {
            public PostCrocomireMissileRoom(UpperNorfairCrocomire region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Post Crocomire Missile Room", metadata, "Cosine Room")
            {
                CosineRoom = new Location(this, LocationId.PostCrocomireMissile, 0x8F8C14, LocationType.Visible,
                    name: "Missile (below Crocomire)",
                    vanillaItem: ItemType.Missile,
                    access: items =>
                        // Can access item
                        region.CanAccessCrocomire(items) && items.Morph &&
                        // Can return
                        (Logic.CanFly(items) || Logic.CanWallJump(WallJumpDifficulty.Medium) || (items.SpeedBooster && Logic.CanUsePowerBombs(items) && items.HiJump && items.Grapple)),
                    memoryAddress: 0x7,
                    memoryFlag: 0x4,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location CosineRoom { get; }
        }

        public class PostCrocomireJumpRoom : Room
        {
            public PostCrocomireJumpRoom(UpperNorfairCrocomire region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Post Crocomire Jump Room", metadata, "Indiana Jones Room", "Pantry")
            {
                IndianaJonesRoom = new Location(this, LocationId.PostCrocomireJump, 0x8F8C2A, LocationType.Visible,
                    name: "Missile (Grappling Beam)",
                    vanillaItem: ItemType.Missile,
                    access: items =>
                        // Can access item
                        region.CanAccessCrocomire(items) && items.Morph && (Logic.CanFly(items) || (items.SpeedBooster && Logic.CanUsePowerBombs(items))) &&
                        // Can return
                        (Logic.CanFly(items) || Logic.CanWallJump(WallJumpDifficulty.Medium) || (items.HiJump && items.Grapple)),
                    memoryAddress: 0x7,
                    memoryFlag: 0x8,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location IndianaJonesRoom { get; }
        }

        public class GrappleBeamRoom : Room
        {
            public GrappleBeamRoom(UpperNorfairCrocomire region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Grapple Beam Room", metadata)
            {
                GrappleBeam = new Location(this, LocationId.GrappleBeam, 0x8F8C36, LocationType.Chozo,
                    name: "Grappling Beam",
                    vanillaItem: ItemType.Grapple,
                    access: items =>
                        // Can access item
                        region.CanAccessCrocomire(items) && items.Morph && (Logic.CanFly(items) || (items.SpeedBooster && Logic.CanUsePowerBombs(items))) &&
                        // Can return
                        (Logic.CanFly(items) || Logic.CanWallJump(WallJumpDifficulty.Medium) || (items.HiJump && items.Grapple)),
                    memoryAddress: 0x7,
                    memoryFlag: 0x10,
                    metadata: metadata,
                    trackerState: trackerState);
            }

            public Location GrappleBeam { get; }
        }
    }
}
