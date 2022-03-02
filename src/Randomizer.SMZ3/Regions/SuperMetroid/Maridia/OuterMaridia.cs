using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Maridia
{
    public class OuterMaridia : SMRegion
    {
        public OuterMaridia(World world, Config config) : base(world, config)
        {
            MainStreetCeiling = new(this, 136, 0x8FC437, LocationType.Visible,
                name: "Missile (green Maridia shinespark)",
                alsoKnownAs: new[] { "Main Street - Ceiling (Shinespark)", "Main Street Missiles" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => items.SpeedBooster,
                    _ => items => items.Gravity && items.SpeedBooster
                });
            MainStreetCrabSupers = new(this, 137, 0x8FC43D, LocationType.Visible,
                name: "Super Missile (green Maridia)",
                alsoKnownAs: "Main Street - Crab Supers",
                vanillaItem: ItemType.Super);
            MamaTurtleRoom = new(this, 138, 0x8FC47D, LocationType.Visible,
                name: "Energy Tank, Mama turtle",
                alsoKnownAs: "Mama Turtle Room",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    Normal => items => World.Logic.CanOpenRedDoors(items) && (World.Logic.CanFly(items) || items.SpeedBooster || items.Grapple),
                    _ => items => World.Logic.CanOpenRedDoors(items) && (
                        World.Logic.CanFly(items) || items.SpeedBooster || items.Grapple ||
                        (World.Logic.CanSpringBallJump(items) && (items.Gravity || items.HiJump))
                    )
                });
            MamaTurtleWallItem = new(this, 139, 0x8FC483, LocationType.Hidden,
                name: "Missile (green Maridia tatori)",
                alsoKnownAs: "Mama Turtle Room - Wall item",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => items => World.Logic.CanOpenRedDoors(items)
                });
        }

        public override string Name => "Outer Maridia";

        public override string Area => "Maridia";

        public Location MainStreetCeiling { get; }

        public Location MainStreetCrabSupers { get; }

        public Location MamaTurtleRoom { get; }

        public Location MamaTurtleWallItem { get; }

        public override bool CanEnter(Progression items)
        {
            return Logic switch
            {
                Normal => items.Gravity && (
                        (World.UpperNorfairWest.CanEnter(items) && World.Logic.CanUsePowerBombs(items)) ||
                        (World.Logic.CanAccessMaridiaPortal(items) && items.CardMaridiaL1 && items.CardMaridiaL2 && (World.Logic.CanPassBombPassages(items) || items.ScrewAttack))
                    ),
                _ =>
                    (World.UpperNorfairWest.CanEnter(items) && World.Logic.CanUsePowerBombs(items) &&
                        (items.Gravity || (items.HiJump && (World.Logic.CanSpringBallJump(items) || items.Ice)))) ||
                    (World.Logic.CanAccessMaridiaPortal(items) && items.CardMaridiaL1 && items.CardMaridiaL2 && (
                        World.Logic.CanPassBombPassages(items) ||
                        (items.Gravity && items.ScrewAttack) ||
                        (items.Super && (items.Gravity || (items.HiJump && (World.Logic.CanSpringBallJump(items) || items.Ice))))
                    ))
            };
        }

    }

}
