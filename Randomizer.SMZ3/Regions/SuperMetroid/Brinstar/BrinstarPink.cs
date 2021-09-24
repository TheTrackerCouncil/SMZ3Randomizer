using System.Collections.Generic;

using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class BrinstarPink : SMRegion
    {
        public BrinstarPink(World world, Config config) : base(world, config)
        {
            Weight = -4;
            Locations = new List<Location>
            {
                SporeSpawnReward,
                PinkShaftTop,
                PinkShaftBottom,
                PinkShaftChozo,
                MissionImpossible,
                GreenHillZone,
                Waterway,
                WaveBeamGlitchRoom
            };
        }

        public override string Name => "Brinstar Pink";
        public override string Area => "Brinstar";

        public Location SporeSpawnReward => new(this, 14, 0x8F84E4, LocationType.Chozo,
            name: "Super Missile (pink Brinstar)",
            alsoKnownAs: "Spore Spawn's item",
            vanillaItem: ItemType.Super,
            access: Logic switch
            {
                Normal => new Requirement(items => items.CardBrinstarBoss && items.CanPassBombPassages() && items.Super),
                _ => new Requirement(items => (items.CardBrinstarBoss || items.CardBrinstarL2) && items.CanPassBombPassages() && items.Super)
            });

        public Location PinkShaftTop => new(this, 21, 0x8F8608, LocationType.Visible,
            name: "Missile (pink Brinstar top)",
            alsoKnownAs: new[] { "Pink Shaft (top)", "Big Pink (top)" },
            vanillaItem: ItemType.Missile); // Grapple or WallJump

        public Location PinkShaftBottom => new(this, 22, 0x8F860E, LocationType.Visible,
            name: "Missile (pink Brinstar bottom)",
            alsoKnownAs: new[] { "Pink Shaft (bottom)", "Big Pink (bottom)" },
            vanillaItem: ItemType.Missile);

        public Location PinkShaftChozo => new(this, 23, 0x8F8614, LocationType.Chozo,
            name: "Charge Beam",
            alsoKnownAs: "Pink Shaft - Chozo",
            vanillaItem: ItemType.Charge,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanPassBombPassages())
            });

        public Location MissionImpossible => new(this, 24, 0x8F865C, LocationType.Visible,
            name: "Power Bomb (pink Brinstar)",
            alsoKnownAs: new[] { "Mission: Impossible", "Pink Brinstar Power Bomb Room" },
            vanillaItem: ItemType.PowerBomb,
            access: Logic switch
            {
                Normal => items => items.CanUsePowerBombs() && items.Super && items.HasEnergyReserves(1),
                _ => new Requirement(items => items.CanUsePowerBombs() && items.Super)
            });

        public Location GreenHillZone => new(this, 25, 0x8F8676, LocationType.Visible,
            name: "Missile (green Brinstar pipe)",
            alsoKnownAs: new[] { "Green Hill Zone", "Jungle slope" },
            vanillaItem: ItemType.Missile,
            access: Logic switch
            {
                _ => new Requirement(items => items.Morph &&
                    (items.PowerBomb || items.Super || items.CanAccessNorfairUpperPortal()))
            });

        public Location Waterway => new(this, 33, 0x8F87FA, LocationType.Visible,
            name: "Energy Tank, Waterway",
            alsoKnownAs: "Waterway",
            vanillaItem: ItemType.ETank,
            access: Logic switch
            {
                _ => new Requirement(items => items.CanUsePowerBombs() && items.CanOpenRedDoors() && items.SpeedBooster &&
                    (items.HasEnergyReserves(1) || items.Gravity))
            });

        public Location WaveBeamGlitchRoom => new(this, 35, 0x8F8824, LocationType.Visible,
            name: "Energy Tank, Brinstar Gate",
            alsoKnownAs: new[] { "Hoptank Room", "Wave Beam Glitch room" },
            vanillaItem: ItemType.ETank,
            access: Logic switch
            {
                Normal => items => items.CardBrinstarL2 && items.CanUsePowerBombs() && items.Wave && items.HasEnergyReserves(1),
                _ => new Requirement(items => items.CardBrinstarL2 && items.CanUsePowerBombs() && (items.Wave || items.Super))
            }); // Grapple or Walljump

        public override bool CanEnter(Progression items) => Logic switch
        {
            Normal =>
                (items.CanOpenRedDoors() && (items.CanDestroyBombWalls() || items.SpeedBooster)) ||
                items.CanUsePowerBombs() ||
                (items.CanAccessNorfairUpperPortal() && items.Morph && items.Wave &&
                    (items.Ice || items.HiJump || items.SpaceJump)),
            _ =>
                (items.CanOpenRedDoors() && (items.CanDestroyBombWalls() || items.SpeedBooster)) ||
                items.CanUsePowerBombs() ||
                (items.CanAccessNorfairUpperPortal() && items.Morph && (items.CanOpenRedDoors() || items.Wave) &&
                    (items.Ice || items.HiJump || items.CanSpringBallJump() || items.CanFly()))
        };
    }
}
