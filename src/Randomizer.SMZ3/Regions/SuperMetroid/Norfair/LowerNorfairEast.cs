using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Norfair
{
    public class LowerNorfairEast : SMRegion, IHasReward
    {
        public LowerNorfairEast(World world, Config config) : base(world, config)
        {
            SpringBallMaze = new(this, 74, 0x8F8FCA, LocationType.Visible,
                name: "Missile (lower Norfair above fire flea room)",
                alsoKnownAs: "Spring Ball Maze Room",
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    _ => items => CanExit(items)
                });
            EscapePowerBombRoom = new(this, 75, 0x8F8FD2, LocationType.Visible,
                name: "Power Bomb (lower Norfair above fire flea room)",
                alsoKnownAs: "Escape Power Bomb Room",
                vanillaItem: ItemType.PowerBomb,
                access: Logic switch
                {
                    Normal => items => CanExit(items),
                    _ => items => CanExit(items) && World.Logic.CanPassBombPassages(items)
                });
            PowerBombOfShame = new(this, 76, 0x8F90C0, LocationType.Visible,
                name: "Power Bomb (Power Bombs of shame)",
                alsoKnownAs: "Wasteland - Power Bomb of Shame",
                vanillaItem: ItemType.PowerBomb,
                access: Logic switch
                {
                    _ => items => CanExit(items) && World.Logic.CanUsePowerBombs(items)
                });
            ThreeMusketeersRoom = new(this, 77, 0x8F9100, LocationType.Visible,
                name: "Missile (lower Norfair near Wave Beam)",
                alsoKnownAs: new[] { "Three Musketeer's Room", "FrankerZ Missiles" },
                vanillaItem: ItemType.Missile,
                access: Logic switch
                {
                    Normal => items => CanExit(items),
                    _ => items => CanExit(items) && items.Morph && World.Logic.CanDestroyBombWalls(items)
                });
            RidleyTreasure = new(this, 78, 0x8F9108, LocationType.Hidden,
                name: "Energy Tank, Ridley",
                alsoKnownAs: "Ridley's Reliquary",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    _ => items => CanExit(items) && items.CardLowerNorfairBoss && World.Logic.CanUsePowerBombs(items) && items.Super
                });
            FirefleaRoom = new(this, 80, 0x8F9184, LocationType.Visible,
                name: "Energy Tank, Firefleas",
                alsoKnownAs: "Fireflea Room",
                vanillaItem: ItemType.ETank,
                access: Logic switch
                {
                    _ => items => CanExit(items)
                });
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

        public override bool CanEnter(Progression items) => Logic switch
        {
            Normal =>
                items.Varia && items.CardLowerNorfairL1 && (
                    (World.UpperNorfairEast.CanEnter(items) && World.Logic.CanUsePowerBombs(items) && items.SpaceJump && items.Gravity) ||
                    (World.Logic.CanAccessNorfairLowerPortal(items) && World.Logic.CanDestroyBombWalls(items) && items.Super && World.Logic.CanUsePowerBombs(items) && World.Logic.CanFly(items))
                ),
            _ =>
                items.Varia && items.CardLowerNorfairL1 && (
                    (World.UpperNorfairEast.CanEnter(items) && World.Logic.CanUsePowerBombs(items) && (items.HiJump || items.Gravity)) ||
                    (World.Logic.CanAccessNorfairLowerPortal(items) && World.Logic.CanDestroyBombWalls(items) && items.Super && (World.Logic.CanFly(items) || World.Logic.CanSpringBallJump(items) || items.SpeedBooster))
                ) &&
                (World.Logic.CanFly(items) || items.HiJump || World.Logic.CanSpringBallJump(items) || (items.Ice && items.Charge)) &&
                (World.Logic.CanPassBombPassages(items) || (items.ScrewAttack && items.SpaceJump))
        };

        public bool CanComplete(Progression items)
        {
            return RidleyTreasure.IsAvailable(items);
        }

        private bool CanExit(Progression items)
        {
            return Logic switch
            {
                Normal => items.CardNorfairL2 /*Bubble Mountain*/ ||
                    (items.Gravity && items.Wave /* Volcano Room and Blue Gate */ && (items.Grapple || items.SpaceJump /*Spikey Acid Snakes and Croc Escape*/)),
                _ => /*Vanilla LN Escape*/
                    (items.Morph && (items.CardNorfairL2 /*Bubble Mountain*/ || ((items.Missile || items.Super || items.Wave /* Blue Gate */) &&
                                     (items.SpeedBooster || World.Logic.CanFly(items) || items.Grapple || (items.HiJump &&
                                     (World.Logic.CanSpringBallJump(items) || items.Ice)) /*Frog Speedway or Croc Escape*/)))) ||
                     /*Reverse Amphitheater*/
                     World.Logic.HasEnergyReserves(items, 5),
            };
        }
    }
}
