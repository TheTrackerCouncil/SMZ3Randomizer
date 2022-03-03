using Randomizer.Shared;

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
                access: items => items.CardBrinstarBoss && Logic.CanPassBombPassages(items) && items.Super);
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
                access: items => Logic.CanPassBombPassages(items));
            MissionImpossible = new(this, 24, 0x8F865C, LocationType.Visible,
                name: "Power Bomb (pink Brinstar)",
                alsoKnownAs: new[] { "Mission: Impossible", "Pink Brinstar Power Bomb Room" },
                vanillaItem: ItemType.PowerBomb,
                access: items => Logic.CanUsePowerBombs(items) && items.Super && Logic.HasEnergyReserves(items, 1));
            GreenHillZone = new(this, 25, 0x8F8676, LocationType.Visible,
                name: "Missile (green Brinstar pipe)",
                alsoKnownAs: new[] { "Green Hill Zone", "Jungle slope" },
                vanillaItem: ItemType.Missile,
                access: items => items.Morph &&
                        (items.PowerBomb || items.Super || Logic.CanAccessNorfairUpperPortal(items)));
            Waterway = new(this, 33, 0x8F87FA, LocationType.Visible,
                name: "Energy Tank, Waterway",
                alsoKnownAs: "Waterway",
                vanillaItem: ItemType.ETank,
                access: items => Logic.CanUsePowerBombs(items) && Logic.CanOpenRedDoors(items) && items.SpeedBooster &&
                        (Logic.HasEnergyReserves(items, 1) || items.Gravity));
            WaveBeamGlitchRoom = new(this, 35, 0x8F8824, LocationType.Visible,
                name: "Energy Tank, Brinstar Gate",
                alsoKnownAs: new[] { "Hoptank Room", "Wave Beam Glitch room" },
                vanillaItem: ItemType.ETank,
                access: items => items.CardBrinstarL2 && Logic.CanUsePowerBombs(items) && items.Wave && Logic.HasEnergyReserves(items, 1)); // Grapple or Walljump
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

        public override bool CanEnter(Progression items) =>
                (Logic.CanOpenRedDoors(items) && (Logic.CanDestroyBombWalls(items) || Logic.CanParlorSpeedBoost(items))) ||
                Logic.CanUsePowerBombs(items) ||
                (Logic.CanAccessNorfairUpperPortal(items) && items.Morph && items.Wave &&
                    (items.Ice || items.HiJump || items.SpaceJump));
    }
}
