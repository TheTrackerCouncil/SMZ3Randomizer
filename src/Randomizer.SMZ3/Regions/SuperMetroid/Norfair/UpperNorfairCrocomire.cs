using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Norfair
{
    public class UpperNorfairCrocomire : SMRegion
    {
        public UpperNorfairCrocomire(World world, Config config) : base(world, config)
        {
            Crocomire = new(this, 52, 0x8F8BA4, LocationType.Visible,
                name: "Energy Tank, Crocomire",
                alsoKnownAs: "Crocomire's Pit",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    Normal => items => CanAccessCrocomire(items) && (World.AdvancedLogic.HasEnergyReserves(items, 1) || items.SpaceJump || items.Grapple),
                    _ => items => CanAccessCrocomire(items)
                });
            CrocomireEscape = new(this, 54, 0x8F8BC0, LocationType.Visible,
                name: "Missile (above Crocomire)",
                alsoKnownAs: "Crocomire Escape",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => World.AdvancedLogic.CanFly(items) || items.Grapple || (items.HiJump && items.SpeedBooster),
                    _ => items => (World.AdvancedLogic.CanFly(items) || items.Grapple || (items.HiJump &&
                        (items.SpeedBooster || World.AdvancedLogic.CanSpringBallJump(items) || (items.Varia && items.Ice)))) && World.AdvancedLogic.CanHellRun(items)
                });
            PostCrocPowerBombRoom = new(this, 57, 0x8F8C04, LocationType.Visible,
                name: "Power Bomb (Crocomire)",
                alsoKnownAs: "Post Crocomire Power Bomb Room",
                vanillaItem: ItemType.PowerBomb,
                access: Logic switch
                {
                    Normal => items => CanAccessCrocomire(items) && (World.AdvancedLogic.CanFly(items) || items.HiJump || items.Grapple),
                    _ => items => CanAccessCrocomire(items)
                });
            CosineRoom = new(this, 58, 0x8F8C14, LocationType.Visible,
                name: "Missile (below Crocomire)",
                alsoKnownAs: new[] { "Cosine Room", "Post Crocomire Missile Room" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => items => CanAccessCrocomire(items) && items.Morph
                });
            IndianaJonesRoom = new(this, 59, 0x8F8C2A, LocationType.Visible,
                name: "Missile (Grappling Beam)",
                alsoKnownAs: new[] { "Indiana Jones Room", "Pantry", "Post Crocomire Jump Room" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => CanAccessCrocomire(items) && items.Morph && (World.AdvancedLogic.CanFly(items) || (items.SpeedBooster && World.AdvancedLogic.CanUsePowerBombs(items))),
                    _ => items => CanAccessCrocomire(items) && (items.SpeedBooster || (items.Morph && (World.AdvancedLogic.CanFly(items) || items.Grapple)))
                });
            GrappleBeamRoom = new(this, 60, 0x8F8C36, LocationType.Chozo,
                name: "Grappling Beam",
                alsoKnownAs: "Grapple Beam Room",
                vanillaItem: ItemType.Grapple,
                access: Logic switch
                {
                    Normal => items => CanAccessCrocomire(items) && items.Morph && (World.AdvancedLogic.CanFly(items) || (items.SpeedBooster && World.AdvancedLogic.CanUsePowerBombs(items))),
                    _ => items => CanAccessCrocomire(items) && (items.SpaceJump || items.Morph || items.Grapple ||
                        (items.HiJump && items.SpeedBooster))
                });
        }

        public override string Name => "Upper Norfair, Crocomire";
        public override string Area => "Upper Norfair";

        public Location Crocomire { get; }

        public Location CrocomireEscape { get; }

        public Location PostCrocPowerBombRoom { get; }

        public Location CosineRoom { get; }

        public Location IndianaJonesRoom { get; }

        public Location GrappleBeamRoom { get; }

        public override bool CanEnter(Progression items)
        {
            return Logic switch
            {
                Normal => (
                        ((World.AdvancedLogic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph) ||
                        World.AdvancedLogic.CanAccessNorfairUpperPortal(items)
                    ) &&
                    items.Varia && (
                        /* Ice Beam -> Croc Speedway */
                        ((Config.Keysanity ? items.CardNorfairL1 : items.Super) && World.AdvancedLogic.CanUsePowerBombs(items) && items.SpeedBooster) ||
                        /* Frog Speedway */
                        (items.SpeedBooster && items.Wave) ||
                        /* Cathedral -> through the floor or Vulcano */
                        (World.AdvancedLogic.CanOpenRedDoors(items) && (Config.Keysanity ? items.CardNorfairL2 : items.Super) &&
                            (World.AdvancedLogic.CanFly(items) || items.HiJump || items.SpeedBooster) &&
                            (World.AdvancedLogic.CanPassBombPassages(items) || (items.Gravity && items.Morph)) && items.Wave)
                        ||
                        /* Reverse Lava Dive */
                        (World.AdvancedLogic.CanAccessNorfairLowerPortal(items) && items.ScrewAttack && items.SpaceJump && items.Super &&
                        items.Gravity && items.Wave && (items.CardNorfairL2 || items.Morph))
                      ),
                _ => (
                        ((World.AdvancedLogic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph) ||
                        World.AdvancedLogic.CanAccessNorfairUpperPortal(items)
                    ) && (
                        /* Ice Beam -> Croc Speedway */
                        ((Config.Keysanity ? items.CardNorfairL1 : items.Super) && World.AdvancedLogic.CanUsePowerBombs(items) &&
                            items.SpeedBooster && (World.AdvancedLogic.HasEnergyReserves(items, 3) || items.Varia)) ||
                        /* Frog Speedway */
                        (items.SpeedBooster && (World.AdvancedLogic.HasEnergyReserves(items, 2) || items.Varia) &&
                            (items.Missile || items.Super || items.Wave)) /* Blue Gate */ ||
                        /* Cathedral -> through the floor or Vulcano */
                        (World.AdvancedLogic.CanHellRun(items) && World.AdvancedLogic.CanOpenRedDoors(items) && (Config.Keysanity ? items.CardNorfairL2 : items.Super) &&
                            (World.AdvancedLogic.CanFly(items) || items.HiJump || items.SpeedBooster || World.AdvancedLogic.CanSpringBallJump(items) || (items.Varia && items.Ice)) &&
                            (World.AdvancedLogic.CanPassBombPassages(items) || (items.Varia && items.Morph)) &&
                            (items.Missile || items.Super || items.Wave)) /* Blue Gate */
                        ) ||
                        /* Reverse Lava Dive */
                        (World.AdvancedLogic.CanAccessNorfairLowerPortal(items) && items.ScrewAttack && items.SpaceJump && items.Varia && items.Super &&
                        World.AdvancedLogic.HasEnergyReserves(items, 2) && (items.CardNorfairL2 || items.Morph))
            };
        }

        private bool CanAccessCrocomire(Progression items)
        {
            return Config.Keysanity ? items.CardNorfairBoss : items.Super;
        }
    }
}
