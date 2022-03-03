using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3.Regions.SuperMetroid.Crateria
{
    public class EastCrateria : SMRegion
    {
        public EastCrateria(World world, Config config) : base(world, config)
        {
            FloodedCavernUnderWater = new(this, 1, 0x8F81E8, LocationType.Visible,
                name: "Missile (outside Wrecked Ship bottom)",
                alsoKnownAs: new[] { "Flooded Cavern - under water", "West Ocean - under water" },
                vanillaItem: ItemType.Missile,
                access: items => items.Morph && (
                        items.SpeedBooster
                        || items.Grapple
                        || items.SpaceJump
                        || (items.Gravity && (Logic.CanIbj(items) || items.HiJump))
                        || World.WreckedShip.CanEnter(items)));
            SkyMissile = new(this, 2, 0x8F81EE, LocationType.Hidden,
                name: "Missile (outside Wrecked Ship top)",
                alsoKnownAs: "Sky Missile",
                vanillaItem: ItemType.Missile,
                access: items => World.WreckedShip.CanEnter(items)
                                                  && (!Config.Keysanity || items.CardWreckedShipBoss)
                                                  && Logic.CanPassBombPassages(items));
            MorphBallMaze = new(this, 3, 0x8F81F4, LocationType.Visible,
                name: "Missile (outside Wrecked Ship middle)",
                alsoKnownAs: "Morph Ball Maze",
                vanillaItem: ItemType.Missile,
                access: items => World.WreckedShip.CanEnter(items)
                                                  && (!Config.Keysanity || items.CardWreckedShipBoss)
                                                  && Logic.CanPassBombPassages(items));
            Moat = new(this, 4, 0x8F8248, LocationType.Visible,
                name: "Missile (Crateria moat)",
                alsoKnownAs: new[] { "The Moat", "Interior Lake" },
                vanillaItem: ItemType.Missile);
        }

        public override string Name => "East Crateria";

        public override string Area => "Crateria";

        public Location FloodedCavernUnderWater { get; }

        public Location SkyMissile { get; }

        public Location MorphBallMaze { get; }

        public Location Moat { get; }

        public override bool CanEnter(Progression items)
        {
            return
                    /* Ship -> Moat */
                    ((Config.Keysanity ? items.CardCrateriaL2 : Logic.CanUsePowerBombs(items)) && items.Super) ||
                    /* UN Portal -> Red Tower -> Moat */
                    ((Config.Keysanity ? items.CardCrateriaL2 : Logic.CanUsePowerBombs(items)) && Logic.CanAccessNorfairUpperPortal(items) &&
                        (items.Ice || items.HiJump || items.SpaceJump)) ||
                    /*Through Maridia From Portal*/
                    (Logic.CanAccessMaridiaPortal(items) && items.Gravity && items.Super && (
                        /* Oasis -> Forgotten Highway */
                        (items.CardMaridiaL2 && Logic.CanDestroyBombWalls(items)) ||
                        /* Draygon -> Cactus Alley -> Forgotten Highway */
                        World.InnerMaridia.DraygonTreasure.IsAvailable(items))) ||
                    /*Through Maridia from Pipe*/
                    (Logic.CanUsePowerBombs(items) && items.Super && items.Gravity);
        }
    }
}
