using System.Collections.Generic;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Data.WorldData;
using Randomizer.Shared;

using static Randomizer.Data.Options.SMLogic;
using Randomizer.Data.Options;

namespace Randomizer.Data.WorldData.Regions.SuperMetroid.Crateria
{
    public class CentralCrateria : SMRegion
    {
        // Considered part of West according to the speedrun wiki

        public CentralCrateria(World world, Config config)
            : base(world, config)
        {
            PowerBombRoom = new Location(this, 0, 0x8F81CC, LocationType.Visible,
                name: "Power Bomb (Crateria surface)",
                alsoKnownAs: new[] { "Chozo Ruins entrance" }, // Referring to Metroid Zero Mission, I guess?
                vanillaItem: ItemType.PowerBomb,
                access: items => (Config.MetroidKeysanity ? items.CardCrateriaL1 : Logic.CanUsePowerBombs(items)) && (items.SpeedBooster || Logic.CanFly(items)),
                memoryAddress: 0x0,
                memoryFlag: 0x1);
            FinalMissileBombWay = new Location(this, 12, 0x8F8486, LocationType.Visible,
                name: "Missile (Crateria middle)",
                alsoKnownAs: new[] { "Final Missile Bombway", "The Final Missile", "Dental Plan Missiles" },
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanPassBombPassages(items),
                memoryAddress: 0x1,
                memoryFlag: 0x10);
            MotherBrainTreasure = new Location(this, 6, 0x8F83EE, LocationType.Visible,
                name: "Missile (Crateria bottom)",
                alsoKnownAs: new[] { "Mother Brain's reliquary", "Pit Room" },
                vanillaItem: ItemType.Missile,
                access: items => Logic.CanDestroyBombWalls(items),
                memoryAddress: 0x0,
                memoryFlag: 0x40);
            SuperMissile = new Location(this, 11, 0x8F8478, LocationType.Visible,
                name: "Super Missile (Crateria)",
                alsoKnownAs: new[] { "Old Tourian launchpad" },
                vanillaItem: ItemType.Super,
                access: items => Logic.CanUsePowerBombs(items) && Logic.HasEnergyReserves(items, 2) && items.SpeedBooster && (!World.Config.LogicConfig.LaunchPadRequiresIceBeam || items.Ice),
                memoryAddress: 0x1,
                memoryFlag: 0x8);
            BombTorizo = new Location(this, 7, 0x8F8404, LocationType.Chozo,
                name: "Bombs",
                alsoKnownAs: new[] { "Bomb Torizo room" },
                vanillaItem: ItemType.Bombs,
                access: items => (Config.MetroidKeysanity ? items.CardCrateriaBoss : Logic.CanOpenRedDoors(items))
                                 && (Logic.CanPassBombPassages(items) || Logic.CanWallJump(WallJumpDifficulty.Hard)),
                memoryAddress: 0x0,
                memoryFlag: 0x80);
            MemoryRegionId = 0;
        }

        public override string Name => "Central Crateria";

        public override string Area => "Crateria";

        public Location PowerBombRoom { get; }

        public Location FinalMissileBombWay { get; }

        public Location MotherBrainTreasure { get; }

        public Location SuperMissile { get; }

        public Location BombTorizo { get; }
    }
}
