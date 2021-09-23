using System.Collections.Generic;

using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Crateria
{
    public class CrateriaCentral : SMRegion
    {
        // Considered part of West according to the speedrun wiki

        public CrateriaCentral(World world, Config config)
            : base(world, config)
        {
            Locations = new List<Location>
            {
                PowerBombRoom,
                FinalMissileBombWay,
                MotherBrainTreasure,
                SuperMissile,
                BombTorizo
            };
        }
        public override string Name => "Crateria Central";
        public override string Area => "Crateria";

        public Location PowerBombRoom => new(this, 0, 0x8F81CC, LocationType.Visible,
            name: "Power Bomb (Crateria surface)",
            alsoKnownAs: "Chozo Ruins entrance", // Referring to Metroid Zero Mission, I guess?
            vanillaItem: ItemType.PowerBomb,
            access: Logic switch
            {
                _ => new Requirement(items => (Config.Keysanity ? items.CardCrateriaL1 : items.CanUsePowerBombs()) && (items.SpeedBooster || items.CanFly()))
            });

        public Location FinalMissileBombWay => new(this, 12, 0x8F8486, LocationType.Visible,
            name: "Missile (Crateria middle)",
            alsoKnownAs: new[] { "Final Missile Bombway", "The Final Missile", "Dental Plan Missiles" },
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanPassBombPassages())
            });

        public Location MotherBrainTreasure => new(this, 6, 0x8F83EE, LocationType.Visible,
            name: "Missile (Crateria bottom)",
            alsoKnownAs: new[] { "Mother Brain's reliquary", "Pit Room" },
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanDestroyBombWalls())
            });

        public Location SuperMissile => new(this, 11, 0x8F8478, LocationType.Visible,
            name: "Super Missile (Crateria)",
            alsoKnownAs: "Old Tourian launchpad",
            vanillaItem: ItemType.Super,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanUsePowerBombs() && items.HasEnergyReserves(2) && items.SpeedBooster)
            });

        public Location BombTorizo => new(this, 7, 0x8F8404, LocationType.Chozo,
            name: "Bombs",
            alsoKnownAs: new[] { "Bomb Torizo room", "Alcatraz", "Flyway" }, // Alcatraz is the right part of the cave room, followed by Flyway, followed by Bomb Torizo room
            vanillaItem: ItemType.Bombs,
            access: Logic switch
            {
                Normal => items => (Config.Keysanity ? items.CardCrateriaBoss : items.CanOpenRedDoors()) && items.CanPassBombPassages(),
                _ => new Requirement(items => (Config.Keysanity ? items.CardCrateriaBoss : items.CanOpenRedDoors()) && items.Morph)
            });
    }
}
