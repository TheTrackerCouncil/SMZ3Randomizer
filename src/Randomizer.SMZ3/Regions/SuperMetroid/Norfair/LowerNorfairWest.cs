using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Norfair
{
    public class LowerNorfairWest : SMRegion
    {
        public LowerNorfairWest(World world, Config config) : base(world, config)
        {
            BeforeGoldTorizo = new(this, 70, 0x8F8E6E, LocationType.Visible,
                name: "Missile (Gold Torizo)",
                alsoKnownAs: "Gold Torizo - Drop down",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.CanUsePowerBombs() && items.SpaceJump && items.Super,
                    _ => new Requirement(items => items.CanUsePowerBombs() && items.SpaceJump && items.Varia && (
                        items.HiJump || items.Gravity ||
                        items.CanAccessNorfairLowerPortal() && (items.CanFly() || items.CanSpringBallJump() || items.SpeedBooster) && items.Super))
                });
            GoldTorizoCeiling = new(this, 71, 0x8F8E74, LocationType.Hidden,
                name: "Super Missile (Gold Torizo)",
                alsoKnownAs: "Golden Torizo - Ceiling",
                vanillaItem: ItemType.Super,
                access: Logic switch
                {
                    Normal => items => items.CanDestroyBombWalls() && (items.Super || items.Charge) &&
                        (items.CanAccessNorfairLowerPortal() || items.SpaceJump && items.CanUsePowerBombs()),
                    _ => new Requirement(items => items.CanDestroyBombWalls() && items.Varia && (items.Super || items.Charge))
                });
            ScrewAttackRoom = new(this, 79, 0x8F9110, LocationType.Chozo,
                name: "Screw Attack",
                access: Logic switch
                {
                    Normal => items => items.CanDestroyBombWalls() && (items.SpaceJump && items.CanUsePowerBombs() || items.CanAccessNorfairLowerPortal()),
                    _ => new Requirement(items => items.CanDestroyBombWalls() && (items.Varia || items.CanAccessNorfairLowerPortal()))
                });
            MickeyMouseClubhouse = new(this, 73, 0x8F8F30, LocationType.Visible,
                name: "Missile (Mickey Mouse room)",
                alsoKnownAs: "Mickey Mouse Clubhouse",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.CanFly() && items.Morph && items.Super &&
                        /*Exit to Upper Norfair*/
                        ((items.CardLowerNorfairL1 || items.Gravity /*Vanilla or Reverse Lava Dive*/) && items.CardNorfairL2 /*Bubble Mountain*/ ||
                        items.Gravity && items.Wave /* Volcano Room and Blue Gate */ && (items.Grapple || items.SpaceJump) /*Spikey Acid Snakes and Croc Escape*/ ||
                        /*Exit via GT fight and Portal*/
                        items.CanUsePowerBombs() && items.SpaceJump && (items.Super || items.Charge)),
                    _ => new Requirement(items =>
                         items.Morph && items.Varia && items.Super && ((items.CanFly() || items.CanSpringBallJump() && items.CanPassBombPassages() ||
                                         (items.CardNorfairL2 && items.CanUsePowerBombs() && (items.HiJump || items.Gravity) || items.SpeedBooster)
                                           && (items.HiJump && items.CanUsePowerBombs() || items.Charge && items.Ice)) &&
                         /*Exit to Upper Norfair*/
                         (items.CardNorfairL2 || items.SpeedBooster || items.CanFly() || items.Grapple || items.HiJump &&
                        (items.CanSpringBallJump() || items.Ice)) ||
                         /*Return to Portal*/
                         items.CanUsePowerBombs()))
                });
        }

        public override string Name => "Lower Norfair, West";

        public override string Area => "Lower Norfair";

        public Location BeforeGoldTorizo { get; }

        public Location GoldTorizoCeiling { get; }

        public Location ScrewAttackRoom { get; }

        public Location MickeyMouseClubhouse { get; }

        // Todo: account for Croc Speedway once Norfair Upper East also do so, otherwise it would be inconsistent to do so here
        public override bool CanEnter(Progression items)
        {
            return Logic switch
            {
                Normal =>
                    items.Varia && (
                        World.UpperNorfairEast.CanEnter(items) && items.CanUsePowerBombs() && items.SpaceJump && items.Gravity && (
                            /* Trivial case, Bubble Mountain access */
                            items.CardNorfairL2 ||
                            /* Frog Speedway -> UN Farming Room gate */
                            items.SpeedBooster && items.Wave
                        ) ||
                        items.CanAccessNorfairLowerPortal() && items.CanDestroyBombWalls()
                    ),
                _ =>
                    World.UpperNorfairEast.CanEnter(items) && items.CanUsePowerBombs() && items.Varia && (items.HiJump || items.Gravity) && (
                        /* Trivial case, Bubble Mountain access */
                        items.CardNorfairL2 ||
                        /* Frog Speedway -> UN Farming Room gate */
                        items.SpeedBooster && (items.Missile || items.Super || items.Wave) /* Blue Gate */
                    ) ||
                    items.CanAccessNorfairLowerPortal() && items.CanDestroyBombWalls()
            };
        }

    }

}
