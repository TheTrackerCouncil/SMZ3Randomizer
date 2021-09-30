using System;
using System.Collections.Generic;
using System.Linq;

using static Randomizer.SMZ3.ItemType;

namespace Randomizer.SMZ3
{
    /// <summary>
    /// Represents an item.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class with the
        /// specified item type.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        public Item(ItemType itemType)
        {
            Name = itemType.GetDescription();
            Type = itemType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class with the
        /// specified item type and world.
        /// </summary>
        /// <param name="itemType">The type of item.</param>
        /// <param name="world">The world the item is in.</param>
        public Item(ItemType itemType, World world) : this(itemType)
        {
            World = world;
        }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of item.
        /// </summary>
        public ItemType Type { get; }

        /// <summary>
        /// Indicates whether the item is an item required to make progress.
        /// </summary>
        public bool Progression { get; protected set; }

        /// <summary>
        /// Gets the world the item is located in.
        /// </summary>
        public World World { get; protected set; }

        /// <summary>
        /// Indicates whether the item is a dungeon-specific item.
        /// </summary>
        public bool IsDungeonItem => Type.IsInAnyCategory(
            ItemCategory.SmallKey,
            ItemCategory.BigKey,
            ItemCategory.Compass,
            ItemCategory.Map);

        /// <summary>
        /// Indicates whether the item is a boss key.
        /// </summary>
        public bool IsBigKey => Type.IsInCategory(ItemCategory.BigKey);

        /// <summary>
        /// Indicates whether the item is a small key.
        /// </summary>
        public bool IsKey => Type.IsInCategory(ItemCategory.SmallKey);

        /// <summary>
        /// Indicates whether the item is a dungeon map.
        /// </summary>
        public bool IsMap => Type.IsInCategory(ItemCategory.Map);

        /// <summary>
        /// Indicates whether the item is a dungeon compass.
        /// </summary>
        public bool IsCompass => Type.IsInCategory(ItemCategory.Compass);

        /// <summary>
        /// Indicates whether the item is a keycard.
        /// </summary>
        public bool IsKeycard => Type.IsInCategory(ItemCategory.Keycard);

        /// <summary>
        /// Returns a list of the items required to progress through the game.
        /// </summary>
        /// <param name="world">The world to assign to the items.</param>
        /// <returns>A new collection of items.</returns>
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

        /// <summary>
        /// Returns a list of items that are nice to have but are not required
        /// to finish the game.
        /// </summary>
        /// <param name="world">The world to assign to the items.</param>
        /// <returns>A new collection of items.</returns>
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

        /// <summary>
        /// Returns a list of items used to fill remaining locations.
        /// </summary>
        /// <param name="world">The world to assign to the items.</param>
        /// <returns>A new collection of items.</returns>
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

        /// <summary>
        /// Returns a list of dungeon-specific items.
        /// </summary>
        /// <remarks>The order of the dungeon pool is significant.</remarks>
        /// <param name="world">The world to assign to the items.</param>
        /// <returns>A new collection of items.</returns>
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

        /// <summary>
        /// Returns a list of the items required to open doors in Metroid in a
        /// keysanity seed.
        /// </summary>
        /// <param name="world">The world to assign to the items.</param>
        /// <returns>A new collection of items.</returns>
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

        /// <summary>
        /// Determines whether the item is of the specified type and belongs to
        /// the specified world.
        /// </summary>
        /// <param name="type">The type of item to check.</param>
        /// <param name="world">The world the item belongs to.</param>
        /// <returns>
        /// <see langword="true"/> if the item is of the specified <paramref
        /// name="type"/> and <paramref name="world"/>; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        public bool Is(ItemType type, World world)
            => Type == type && World == world;

        /// <summary>
        /// Determines whether the item is of a different type or belongs to a
        /// different world.
        /// </summary>
        /// <param name="type">The type of item to check.</param>
        /// <param name="world">The world the item belongs to.</param>
        /// <returns>
        /// <see langword="true"/> if the item is not of the specified <paramref
        /// name="type"/> or <paramref name="world"/>; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        public bool IsNot(ItemType type, World world)
            => !Is(type, world);

        private static List<Item> Copies(int nr, Func<Item> template)
        {
            return Enumerable.Range(1, nr).Select(i => template()).ToList();
        }
    }
}
