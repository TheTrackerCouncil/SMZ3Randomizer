using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using System.Collections.Generic;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Norfair
{
    public class LowerNorfairWest : SMRegion
    {
        public LowerNorfairWest(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            GoldenTorizos = new GoldenTorizosRoom(this, metadata, trackerState);
            MickeyMouse = new MickeyMouseRoom(this, metadata, trackerState);
            ScrewAttack = new ScrewAttackRoom(this, metadata, trackerState);
            MemoryRegionId = 2;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Lower Norfair West");
            MapName = "Norfair";
        }

        public override string Name => "Lower Norfair, West";

        public override string Area => "Lower Norfair";

        public GoldenTorizosRoom GoldenTorizos { get; }

        public MickeyMouseRoom MickeyMouse { get; }

        public ScrewAttackRoom ScrewAttack { get; }

        // Todo: account for Croc Speedway once Norfair Upper East also do so, otherwise it would be inconsistent to do so here
        public override bool CanEnter(Progression items, bool requireRewards)
        {
            return items.Varia && (
                        World.UpperNorfairEast.CanEnter(items, requireRewards) && Logic.CanUsePowerBombs(items) && Logic.CanFly(items) && items.Gravity && (
                            /* Trivial case, Bubble Mountain access */
                            items.CardNorfairL2 ||
                            /* Frog Speedway -> UN Farming Room gate */
                            items.SpeedBooster && items.Wave
                        ) ||
                        Logic.CanAccessNorfairLowerPortal(items) && Logic.CanDestroyBombWalls(items)
                    );
        }

        public class GoldenTorizosRoom : Room
        {
            public GoldenTorizosRoom(LowerNorfairWest region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Golden Torizo's Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.LowerNorfairGoldenTorizoVisible, 0x8F8E6E, LocationType.Visible,
                        name: "Missile (Gold Torizo)",
                        vanillaItem: ItemType.Missile,
                        access: items => Logic.CanUsePowerBombs(items) && items.SpaceJump && items.Super,
                        memoryAddress: 0x8,
                        memoryFlag: 0x40,
                        metadata: metadata,
                        trackerState: trackerState),
                    new Location(this, LocationId.LowerNorfairGoldenTorizoHidden, 0x8F8E74, LocationType.Hidden,
                        name: "Super Missile (Gold Torizo)",
                        vanillaItem: ItemType.Super,
                        access: items => Logic.CanDestroyBombWalls(items)
                                         && (items.Super || items.Charge)
                                         && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items))
                                         && (Logic.CanAccessNorfairLowerPortal(items) || (items.SpaceJump && Logic.CanUsePowerBombs(items))),
                        memoryAddress: 0x8,
                        memoryFlag: 0x80,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class MickeyMouseRoom : Room
        {
            public MickeyMouseRoom(LowerNorfairWest region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Mickey Mouse Room", metadata, "Mickey Mouse Clubhouse")
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.LowerNorfairMickeyMouse, 0x8F8F30, LocationType.Visible,
                        name: "Missile (Mickey Mouse room)",
                        vanillaItem: ItemType.Missile,
                        access: items => items.Morph && items.Super && (
                                    Logic.CanWallJump(WallJumpDifficulty.Insane) ||
                                    (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Hard)) ||
                                    Logic.CanFly(items)
                                ) &&
                                /*Exit to Upper Norfair*/
                                (((items.CardLowerNorfairL1 || items.Gravity /*Vanilla or Reverse Lava Dive*/) && items.CardNorfairL2) /*Bubble Mountain*/ ||
                                (items.Gravity && items.Wave /* Volcano Room and Blue Gate */ && (items.Grapple || items.SpaceJump)) /*Spikey Acid Snakes and Croc Escape*/ ||
                                /*Exit via GT fight and Portal*/
                                (Logic.CanUsePowerBombs(items) && items.SpaceJump && (items.Super || items.Charge))),
                        memoryAddress: 0x9,
                        memoryFlag: 0x2,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }

        public class ScrewAttackRoom : Room
        {
            public ScrewAttackRoom(LowerNorfairWest region, IMetadataService? metadata, TrackerState? trackerState)
                : base(region, "Screw Attack Room", metadata)
            {
                Locations = new List<Location>
                {
                    new Location(this, LocationId.LowerNorfairScrewAttack, 0x8F9110, LocationType.Chozo,
                        name: "Screw Attack",
                        access: items => (Logic.CanDestroyBombWalls(items) || items.ScrewAttack) && (items.SpaceJump && Logic.CanUsePowerBombs(items) || Logic.CanAccessNorfairLowerPortal(items)),
                        memoryAddress: 0x9,
                        memoryFlag: 0x80,
                        metadata: metadata,
                        trackerState: trackerState)
                };
            }
        }
    }
}
