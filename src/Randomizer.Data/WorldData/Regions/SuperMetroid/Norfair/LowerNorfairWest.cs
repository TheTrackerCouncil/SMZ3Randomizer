using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Norfair
{
    public class LowerNorfairWest : SMRegion
    {
        public LowerNorfairWest(World world, Config config, IMetadataService? metadata, TrackerState? trackerState) : base(world, config, metadata, trackerState)
        {
            BeforeGoldTorizo = new Location(this, 70, 0x8F8E6E, LocationType.Visible,
                name: "Missile (Gold Torizo)",
                alsoKnownAs: new[] { "Gold Torizo - Drop down" },
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanUsePowerBombs(items) && items.SpaceJump && items.Super,
                memoryAddress: 0x8,
                memoryFlag: 0x40,
                metadata: metadata,
                trackerState: trackerState);
            GoldTorizoCeiling = new Location(this, 71, 0x8F8E74, LocationType.Hidden,
                name: "Super Missile (Gold Torizo)",
                alsoKnownAs: new[] { "Golden Torizo - Ceiling" },
                vanillaItem: ItemType.Super,
                access: items => Logic.CanDestroyBombWalls(items)
                                 && (items.Super || items.Charge)
                                 && (Logic.CanWallJump(WallJumpDifficulty.Easy) || Logic.CanFly(items))
                                 && (Logic.CanAccessNorfairLowerPortal(items) || (items.SpaceJump && Logic.CanUsePowerBombs(items))),
                memoryAddress: 0x8,
                memoryFlag: 0x80,
                metadata: metadata,
                trackerState: trackerState);
            ScrewAttackRoom = new Location(this, 79, 0x8F9110, LocationType.Chozo,
                name: "Screw Attack",
                access: items => (Logic.CanDestroyBombWalls(items) || items.ScrewAttack) && (items.SpaceJump && Logic.CanUsePowerBombs(items) || Logic.CanAccessNorfairLowerPortal(items)),
                memoryAddress: 0x9,
                memoryFlag: 0x80,
                metadata: metadata,
                trackerState: trackerState);
            MickeyMouseClubhouse = new Location(this, 73, 0x8F8F30, LocationType.Visible,
                name: "Missile (Mickey Mouse room)",
                alsoKnownAs: new[] { "Mickey Mouse Clubhouse" },
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
                trackerState: trackerState);
            MemoryRegionId = 2;
            Metadata = metadata?.Region(GetType()) ?? new RegionInfo("Lower Norfair West");
        }

        public override string Name => "Lower Norfair, West";

        public override string Area => "Lower Norfair";

        public Location BeforeGoldTorizo { get; }

        public Location GoldTorizoCeiling { get; }

        public Location ScrewAttackRoom { get; }

        public Location MickeyMouseClubhouse { get; }

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

    }

}
