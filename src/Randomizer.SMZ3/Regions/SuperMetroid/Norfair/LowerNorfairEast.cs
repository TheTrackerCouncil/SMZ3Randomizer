﻿using Randomizer.Shared;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Norfair
{
    public class LowerNorfairEast : SMRegion, IHasReward
    {
        public LowerNorfairEast(World world, Config config) : base(world, config)
        {
            SpringBallMaze = new Location(this, 74, 0x8F8FCA, LocationType.Visible,
                name: "Missile (lower Norfair above fire flea room)",
                alsoKnownAs: new[] { "Spring Ball Maze Room" },
                vanillaItem: ItemType.Missile,
                access: items => CanExit(items),
                memoryAddress: 0x9,
                memoryFlag: 0x4);
            EscapePowerBombRoom = new Location(this, 75, 0x8F8FD2, LocationType.Visible,
                name: "Power Bomb (lower Norfair above fire flea room)",
                alsoKnownAs: new[] { "Escape Power Bomb Room" },
                vanillaItem: ItemType.PowerBomb,
                access: items => CanExit(items),
                memoryAddress: 0x9,
                memoryFlag: 0x8);
            PowerBombOfShame = new Location(this, 76, 0x8F90C0, LocationType.Visible,
                name: "Power Bomb (Power Bombs of shame)",
                alsoKnownAs: new[] { "Wasteland - Power Bomb of Shame" },
                vanillaItem: ItemType.PowerBomb,
                access: items => CanExit(items) && Logic.CanUsePowerBombs(items),
                memoryAddress: 0x9,
                memoryFlag: 0x10);
            ThreeMusketeersRoom = new Location(this, 77, 0x8F9100, LocationType.Visible,
                name: "Missile (lower Norfair near Wave Beam)",
                alsoKnownAs: new[] { "Three Musketeer's Room", "FrankerZ Missiles" },
                vanillaItem: ItemType.Missile,
                access: items => CanExit(items),
                memoryAddress: 0x9,
                memoryFlag: 0x20);
            RidleyTreasure = new Location(this, 78, 0x8F9108, LocationType.Hidden,
                name: "Energy Tank, Ridley",
                alsoKnownAs: new[] { "Ridley's Reliquary" },
                vanillaItem: ItemType.ETank,
                access: items => CanExit(items) && items.CardLowerNorfairBoss && Logic.CanUsePowerBombs(items) && items.Super,
                memoryAddress: 0x9,
                memoryFlag: 0x40);
            FirefleaRoom = new Location(this, 80, 0x8F9184, LocationType.Visible,
                name: "Energy Tank, Firefleas",
                alsoKnownAs: new[] { "Fireflea Room" },
                vanillaItem: ItemType.ETank,
                access: items => CanExit(items),
                memoryAddress: 0xA,
                memoryFlag: 0x1);
            BossReward = new Location(this, -1, -1, LocationType.MetroidBoss,
                name: "Ridley Reward",
                vanillaItem: ItemType.GoldenFourBoss,
                access: items => CanComplete(items),
                memoryAddress: null,
                memoryFlag: null);
            MemoryRegionId = 2;
        }

        public override string Name => "Lower Norfair, East";

        public override string Area => "Lower Norfair";

        public Reward Reward { get; set; } = Reward.GoldenFourBoss;

        public Location SpringBallMaze { get; }

        public Location EscapePowerBombRoom { get; }

        public Location PowerBombOfShame { get; }

        public Location ThreeMusketeersRoom { get; }

        public Location RidleyTreasure { get; }

        public Location FirefleaRoom { get; }

        public Location BossReward { get; }

        public Location RewardLocation => BossReward;

        public override bool CanEnter(Progression items) => items.Varia && items.CardLowerNorfairL1 && (
                    // Access via elevator from upper norfair east past Ridley's mouth
                    (World.UpperNorfairEast.CanEnter(items) && Logic.CanUsePowerBombs(items) && Logic.CanFly(items) && items.Gravity) ||
                    // Access via Zelda portal and passing worst room in the game
                    (Logic.CanAccessNorfairLowerPortal(items) && Logic.CanDestroyBombWalls(items) && items.Super && Logic.CanUsePowerBombs(items) && (
                        Logic.CanWallJump(WallJumpDifficulty.Insane) ||
                        (items.HiJump && Logic.CanWallJump(WallJumpDifficulty.Hard)) ||
                        Logic.CanFly(items)
                    ))
                );

        public bool CanComplete(Progression items)
        {
            return RidleyTreasure.IsAvailable(items);
        }

        private bool CanExit(Progression items)
        {
            return items.CardNorfairL2 /*Bubble Mountain*/ ||
                    (items.Gravity && items.Wave /* Volcano Room and Blue Gate */ && (items.Grapple || items.SpaceJump /*Spikey Acid Snakes and Croc Escape*/));
        }
    }
}
