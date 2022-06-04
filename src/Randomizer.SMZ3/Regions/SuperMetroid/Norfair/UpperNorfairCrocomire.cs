using Randomizer.Shared;

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
                access: items => CanAccessCrocomire(items) && (Logic.HasEnergyReserves(items, 1) || items.SpaceJump || items.Grapple),
                memoryAddress: 0x6,
                memoryFlag: 0x10);
            CrocomireEscape = new(this, 54, 0x8F8BC0, LocationType.Visible,
                name: "Missile (above Crocomire)",
                alsoKnownAs: "Crocomire Escape",
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanFly(items) || items.Grapple || (items.HiJump && items.SpeedBooster),
                memoryAddress: 0x6,
                memoryFlag: 0x40);
            PostCrocPowerBombRoom = new(this, 57, 0x8F8C04, LocationType.Visible,
                name: "Power Bomb (Crocomire)",
                alsoKnownAs: "Post Crocomire Power Bomb Room",
                vanillaItem: ItemType.PowerBomb,
                access: items => CanAccessCrocomire(items) && (Logic.CanFly(items) || items.HiJump || items.Grapple),
                memoryAddress: 0x7,
                memoryFlag: 0x2);
            CosineRoom = new(this, 58, 0x8F8C14, LocationType.Visible,
                name: "Missile (below Crocomire)",
                alsoKnownAs: new[] { "Cosine Room", "Post Crocomire Missile Room" },
                vanillaItem: ItemType.Missile,
                access: items => CanAccessCrocomire(items) && items.Morph,
                memoryAddress: 0x7,
                memoryFlag: 0x4);
            IndianaJonesRoom = new(this, 59, 0x8F8C2A, LocationType.Visible,
                name: "Missile (Grappling Beam)",
                alsoKnownAs: new[] { "Indiana Jones Room", "Pantry", "Post Crocomire Jump Room" },
                vanillaItem: ItemType.Missile,
                access: items => CanAccessCrocomire(items) && items.Morph && (Logic.CanFly(items) || (items.SpeedBooster && Logic.CanUsePowerBombs(items))),
                memoryAddress: 0x7,
                memoryFlag: 0x8);
            GrappleBeamRoom = new(this, 60, 0x8F8C36, LocationType.Chozo,
                name: "Grappling Beam",
                alsoKnownAs: "Grapple Beam Room",
                vanillaItem: ItemType.Grapple,
                access: items => CanAccessCrocomire(items) && items.Morph && (Logic.CanFly(items) || (items.SpeedBooster && Logic.CanUsePowerBombs(items))),
                memoryAddress: 0x7,
                memoryFlag: 0x10);
            MemoryRegionId = 2;
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
            return (
                        ((Logic.CanDestroyBombWalls(items) || items.SpeedBooster) && items.Super && items.Morph) ||
                        Logic.CanAccessNorfairUpperPortal(items)
                    ) &&
                    items.Varia && (
                        /* Ice Beam -> Croc Speedway */
                        ((Config.Keysanity ? items.CardNorfairL1 : items.Super) && Logic.CanUsePowerBombs(items) && items.SpeedBooster) ||
                        /* Frog Speedway */
                        (items.SpeedBooster && items.Wave) ||
                        /* Cathedral -> through the floor or Vulcano */
                        (Logic.CanOpenRedDoors(items) && (Config.Keysanity ? items.CardNorfairL2 : items.Super) &&
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
            return Config.Keysanity ? items.CardNorfairBoss : items.Super;
        }
    }
}
