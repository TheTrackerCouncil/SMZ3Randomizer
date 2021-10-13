﻿using System.Collections.Generic;

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
                    _ => new Requirement(items => items.Gravity && items.SpeedBooster)
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
                    Normal => items => items.CanOpenRedDoors() && (items.CanFly() || items.SpeedBooster || items.Grapple),
                    _ => new Requirement(items => items.CanOpenRedDoors() && (
                        items.CanFly() || items.SpeedBooster || items.Grapple ||
                        (items.CanSpringBallJump() && (items.Gravity || items.HiJump))
                    ))
                });
            MamaTurtleWallItem = new(this, 139, 0x8FC483, LocationType.Hidden,
                name: "Missile (green Maridia tatori)",
                alsoKnownAs: "Mama Turtle Room - Wall item",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => new Requirement(items => items.CanOpenRedDoors())
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
                        (World.UpperNorfairWest.CanEnter(items) && items.CanUsePowerBombs()) ||
                        (items.CanAccessMaridiaPortal(World) && items.CardMaridiaL1 && items.CardMaridiaL2 && (items.CanPassBombPassages() || items.ScrewAttack))
                    ),
                _ =>
                    (World.UpperNorfairWest.CanEnter(items) && items.CanUsePowerBombs() &&
                        (items.Gravity || (items.HiJump && (items.CanSpringBallJump() || items.Ice)))) ||
                    (items.CanAccessMaridiaPortal(World) && items.CardMaridiaL1 && items.CardMaridiaL2 && (
                        items.CanPassBombPassages() ||
                        (items.Gravity && items.ScrewAttack) ||
                        (items.Super && (items.Gravity || (items.HiJump && (items.CanSpringBallJump() || items.Ice))))
                    ))
            };
        }

    }

}