using System;
using System.Linq;
using System.Collections.Generic;
using static Randomizer.SMZ3.ItemType;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Randomizer.SMZ3
{
    public class Item
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public bool Progression { get; set; }
        public World World { get; set; }

        private static readonly Regex dungeon = new("^(BigKey|Key|Map|Compass)");
        private static readonly Regex bigKey = new("^BigKey");
        private static readonly Regex key = new("^Key");
        private static readonly Regex map = new("^Map");
        private static readonly Regex compass = new("^Compass");
        private static readonly Regex keycard = new("^Card");

        public bool IsDungeonItem => dungeon.IsMatch(Type.ToString());
        public bool IsBigKey => bigKey.IsMatch(Type.ToString());
        public bool IsKey => key.IsMatch(Type.ToString());
        public bool IsMap => map.IsMatch(Type.ToString());
        public bool IsCompass => compass.IsMatch(Type.ToString());
        public bool IsKeycard => keycard.IsMatch(Type.ToString());

        public bool Is(ItemType type, World world)
            => Type == type && World == world;

        public bool IsNot(ItemType type, World world)
            => !Is(type, world);

        public Item(ItemType itemType)
        {
            Name = itemType.GetDescription();
            Type = itemType;
        }

        public Item(ItemType itemType, World world) : this(itemType)
        {
            World = world;
        }

        public static Item Nothing(World world)
        {
            return new Item(ItemType.Nothing, world);
        }

        public static List<Item> CreateProgressionPool(World world)
        {
            var itemPool = new List<Item> {
                new Item(ProgressiveShield),
                new Item(ProgressiveShield),
                new Item(ProgressiveShield),
                new Item(ProgressiveSword),
                new Item(ProgressiveSword),
                new Item(Bow),
                new Item(Hookshot),
                new Item(Mushroom),
                new Item(Powder),
                new Item(Firerod),
                new Item(Icerod),
                new Item(Bombos),
                new Item(Ether),
                new Item(Quake),
                new Item(Lamp),
                new Item(Hammer),
                new Item(Shovel),
                new Item(Flute),
                new Item(Bugnet),
                new Item(Book),
                new Item(Bottle),
                new Item(Somaria),
                new Item(Byrna),
                new Item(Cape),
                new Item(Mirror),
                new Item(Boots),
                new Item(ProgressiveGlove),
                new Item(ProgressiveGlove),
                new Item(Flippers),
                new Item(MoonPearl),
                new Item(HalfMagic),

                new Item(Grapple),
                new Item(Charge),
                new Item(Ice),
                new Item(Wave),
                new Item(Plasma),
                new Item(Varia),
                new Item(Gravity),
                new Item(Morph),
                new Item(Bombs),
                new Item(SpringBall),
                new Item(ScrewAttack),
                new Item(HiJump),
                new Item(SpaceJump),
                new Item(SpeedBooster),

                new Item(Missile),
                new Item(Super),
                new Item(PowerBomb),
                new Item(PowerBomb),
                new Item(ETank),
                new Item(ETank),
                new Item(ETank),
                new Item(ETank),
                new Item(ETank),

                new Item(ReserveTank),
                new Item(ReserveTank),
                new Item(ReserveTank),
                new Item(ReserveTank),
            };

            foreach (var item in itemPool)
            {
                item.Progression = true;
                item.World = world;
            }

            return itemPool;
        }

        public static List<Item> CreateNicePool(World world)
        {
            var itemPool = new List<Item> {
                new Item(ProgressiveTunic),
                new Item(ProgressiveTunic),
                new Item(ProgressiveSword),
                new Item(ProgressiveSword),
                new Item(SilverArrows),
                new Item(BlueBoomerang),
                new Item(RedBoomerang),
                new Item(Bottle),
                new Item(Bottle),
                new Item(Bottle),
                new Item(HeartContainerRefill),

                new Item(Spazer),
                new Item(XRay),
            };

            itemPool.AddRange(Copies(10, () => new Item(HeartContainer, world)));

            foreach (var item in itemPool) item.World = world;

            return itemPool;
        }

        public static List<Item> CreateJunkPool(World world)
        {
            var itemPool = new List<Item> {
                new Item(Arrow),
                new Item(OneHundredRupees)
            };

            itemPool.AddRange(Copies(24, () => new Item(HeartPiece)));
            itemPool.AddRange(Copies(8, () => new Item(TenArrows)));
            itemPool.AddRange(Copies(13, () => new Item(ThreeBombs)));
            itemPool.AddRange(Copies(4, () => new Item(ArrowUpgrade5)));
            itemPool.AddRange(Copies(4, () => new Item(BombUpgrade5)));
            itemPool.AddRange(Copies(2, () => new Item(OneRupee)));
            itemPool.AddRange(Copies(4, () => new Item(FiveRupees)));
            itemPool.AddRange(Copies(world.Config.Keysanity ? 25 : 28, () => new Item(TwentyRupees)));
            itemPool.AddRange(Copies(7, () => new Item(FiftyRupees)));
            itemPool.AddRange(Copies(5, () => new Item(ThreeHundredRupees)));

            itemPool.AddRange(Copies(9, () => new Item(ETank)));
            itemPool.AddRange(Copies(39, () => new Item(Missile)));
            itemPool.AddRange(Copies(15, () => new Item(Super)));
            itemPool.AddRange(Copies(8, () => new Item(PowerBomb)));

            foreach (var item in itemPool) item.World = world;

            return itemPool;
        }

        /* The order of the dungeon pool is significant */
        public static List<Item> CreateDungeonPool(World world)
        {
            var itemPool = new List<Item>();

            itemPool.AddRange(new[] {
                new Item(BigKeyEP),
                new Item(BigKeyDP),
                new Item(BigKeyTH),
                new Item(BigKeyPD),
                new Item(BigKeySP),
                new Item(BigKeySW),
                new Item(BigKeyTT),
                new Item(BigKeyIP),
                new Item(BigKeyMM),
                new Item(BigKeyTR),
                new Item(BigKeyGT),
            });

            itemPool.AddRange(Copies(1, () => new Item(KeyHC)));
            itemPool.AddRange(Copies(2, () => new Item(KeyCT)));
            itemPool.AddRange(Copies(1, () => new Item(KeyDP)));
            itemPool.AddRange(Copies(1, () => new Item(KeyTH)));
            itemPool.AddRange(Copies(6, () => new Item(KeyPD)));
            itemPool.AddRange(Copies(1, () => new Item(KeySP)));
            itemPool.AddRange(Copies(3, () => new Item(KeySW)));
            itemPool.AddRange(Copies(1, () => new Item(KeyTT)));
            itemPool.AddRange(Copies(2, () => new Item(KeyIP)));
            itemPool.AddRange(Copies(3, () => new Item(KeyMM)));
            itemPool.AddRange(Copies(4, () => new Item(KeyTR)));
            itemPool.AddRange(Copies(4, () => new Item(KeyGT)));

            itemPool.AddRange(new[] {
                new Item(MapEP),
                new Item(MapDP),
                new Item(MapTH),
                new Item(MapPD),
                new Item(MapSP),
                new Item(MapSW),
                new Item(MapTT),
                new Item(MapIP),
                new Item(MapMM),
                new Item(MapTR),
            });
            if (!world.Config.Keysanity)
            {
                itemPool.AddRange(new[] {
                    new Item(MapHC),
                    new Item(MapGT),
                    new Item(CompassEP),
                    new Item(CompassDP),
                    new Item(CompassTH),
                    new Item(CompassPD),
                    new Item(CompassSP),
                    new Item(CompassSW),
                    new Item(CompassTT),
                    new Item(CompassIP),
                    new Item(CompassMM),
                    new Item(CompassTR),
                    new Item(CompassGT),
                });
            }

            foreach (var item in itemPool) item.World = world;

            return itemPool;
        }

        public static List<Item> CreateKeycards(World world)
        {
            return new List<Item> {
                new Item(CardCrateriaL1, world),
                new Item(CardCrateriaL2, world),
                new Item(CardCrateriaBoss, world),
                new Item(CardBrinstarL1, world),
                new Item(CardBrinstarL2, world),
                new Item(CardBrinstarBoss, world),
                new Item(CardNorfairL1, world),
                new Item(CardNorfairL2, world),
                new Item(CardNorfairBoss, world),
                new Item(CardMaridiaL1, world),
                new Item(CardMaridiaL2, world),
                new Item(CardMaridiaBoss, world),
                new Item(CardWreckedShipL1, world),
                new Item(CardWreckedShipBoss, world),
                new Item(CardLowerNorfairL1, world),
                new Item(CardLowerNorfairBoss, world),
            };
        }

        static List<Item> Copies(int nr, Func<Item> template)
        {
            return Enumerable.Range(1, nr).Select(i => template()).ToList();
        }

    }

}
