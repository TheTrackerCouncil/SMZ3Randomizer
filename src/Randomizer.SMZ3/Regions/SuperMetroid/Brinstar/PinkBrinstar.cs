using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Brinstar
{
    public class PinkBrinstar : SMRegion
    {
        public PinkBrinstar(World world, Config config) : base(world, config)
        {
            Weight = -4;
            SporeSpawnReward = new(this, 14, 0x8F84E4, LocationType.Chozo,
                name: "Super Missile (pink Brinstar)",
                alsoKnownAs: "Spore Spawn's item",
                vanillaItem: ItemType.Super,
                access: Logic switch
                {
                    Normal => items => items.CardBrinstarBoss && items.CanPassBombPassages() && items.Super,
                    _ => items => (items.CardBrinstarBoss || items.CardBrinstarL2) && items.CanPassBombPassages() && items.Super
                });
            PinkShaftTop = new(this, 21, 0x8F8608, LocationType.Visible,
                name: "Missile (pink Brinstar top)",
                alsoKnownAs: new[] { "Pink Shaft (top)", "Big Pink (top)" },
                vanillaItem: ItemType.Missile); // Grapple or WallJump
            PinkShaftBottom = new(this, 22, 0x8F860E, LocationType.Visible,
                name: "Missile (pink Brinstar bottom)",
                alsoKnownAs: new[] { "Pink Shaft (bottom)", "Big Pink (bottom)" },
                vanillaItem: ItemType.Missile);
            PinkShaftChozo = new(this, 23, 0x8F8614, LocationType.Chozo,
                name: "Charge Beam",
                alsoKnownAs: "Pink Shaft - Chozo",
                vanillaItem: ItemType.Charge,
                access: Logic switch
                {
                    _ => items => items.CanPassBombPassages()
                });
            MissionImpossible = new(this, 24, 0x8F865C, LocationType.Visible,
                name: "Power Bomb (pink Brinstar)",
                alsoKnownAs: new[] { "Mission: Impossible", "Pink Brinstar Power Bomb Room" },
                vanillaItem: ItemType.PowerBomb,
                access: Logic switch
                {
                    Normal => items => items.CanUsePowerBombs() && items.Super && items.HasEnergyReserves(1),
                    _ => items => items.CanUsePowerBombs() && items.Super
                });
            GreenHillZone = new(this, 25, 0x8F8676, LocationType.Visible,
                name: "Missile (green Brinstar pipe)",
                alsoKnownAs: new[] { "Green Hill Zone", "Jungle slope" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => items => items.Morph &&
                        (items.PowerBomb || items.Super || items.CanAccessNorfairUpperPortal())
                });
            Waterway = new(this, 33, 0x8F87FA, LocationType.Visible,
                name: "Energy Tank, Waterway",
                alsoKnownAs: "Waterway",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    _ => items => items.CanUsePowerBombs() && items.CanOpenRedDoors() && items.SpeedBooster &&
                        (items.HasEnergyReserves(1) || items.Gravity)
                });
            WaveBeamGlitchRoom = new(this, 35, 0x8F8824, LocationType.Visible,
                name: "Energy Tank, Brinstar Gate",
                alsoKnownAs: new[] { "Hoptank Room", "Wave Beam Glitch room" },
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    Normal => items => items.CardBrinstarL2 && items.CanUsePowerBombs() && items.Wave && items.HasEnergyReserves(1),
                    _ => items => items.CardBrinstarL2 && items.CanUsePowerBombs() && (items.Wave || items.Super)
                }); // Grapple or Walljump
        }

        public override string Name => "Pink Brinstar";

        public override string Area => "Brinstar";

        public Location SporeSpawnReward { get; }

        public Location PinkShaftTop { get; }

        public Location PinkShaftBottom { get; }

        public Location PinkShaftChozo { get; }

        public Location MissionImpossible { get; }

        public Location GreenHillZone { get; }

        public Location Waterway { get; }

        public Location WaveBeamGlitchRoom { get; }

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
