using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using Randomizer.Data.Configuration.ConfigTypes;
using Randomizer.Shared;
using Randomizer.Shared.Models;

namespace Randomizer.Data.WorldData
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
        public Item(ItemType itemType, World world, string? name = null) : this(itemType)
        {
            World = world;
            if (!string.IsNullOrEmpty(name))
            {
                Name = name;
            }
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
        /// Additional information about the item
        /// </summary>
        public ItemData Metadata { get; set; }

        /// <summary>
        /// Current state of the item
        /// </summary>
        public TrackerItemState State { get; set; }

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
        /// Gets the number of actual items as displayed or mentioned by
        /// tracker, or <c>0</c> if the item does not have copies.
        /// </summary>
        public int Counter => Metadata.Multiple && !Metadata.HasStages ? State.TrackingState * (Metadata.CounterMultiplier ?? 1) : 0;

        /// <summary>
        /// Tracks the item.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the item was tracked; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Tracking may fail if the item is already tracked, or if the item is
        /// already at the highest stage.
        /// </remarks>
        public bool Track()
        {
            if (State.TrackingState == 0 // Item hasn't been tracked yet (any case)
                || !Metadata.HasStages && Metadata.Multiple // State.Multiple items always track
                || Metadata.HasStages && State.TrackingState < Metadata.MaxStage) // Hasn't reached max. stage yet
            {
                State.TrackingState++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Untracks the item or decreases the item by one step.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the item was removed; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        public bool Untrack()
        {
            if (State.TrackingState == 0)
                return false;

            State.TrackingState--;
            return true;
        }

        /// <summary>
        /// Marks the item at the specified stage.
        /// </summary>
        /// <param name="stage">The stage to set the item to.</param>
        /// <returns>
        /// <see langword="true"/> if the item was tracked; otherwise, <see
        /// langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Tracking may fail if the item is already at a higher stage.
        /// </remarks>
        public bool Track(int stage)
        {
            if (!Metadata.HasStages)
                throw new ArgumentException($"The item '{Name}' does not have Multiple stages.");

            if (stage > Metadata.MaxStage)
                throw new ArgumentOutOfRangeException($"Cannot advance item '{Name}' to stage {stage} as the highest state is {Metadata.MaxStage}.");

            if (State.TrackingState < stage)
            {
                State.TrackingState = stage;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the item-specific phrases to respond with when tracking
        /// the item.
        /// </summary>
        /// <param name="response">
        /// When this method returns <c>true</c>, contains the possible phrases
        /// to respond with when tracking the item.
        /// </param>
        /// <returns>
        /// <c>true</c> if a response was configured for the item at the current
        /// tracking state; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetTrackingResponse([NotNullWhen(true)] out SchrodingersString? response)
        {
            return Metadata.TryGetTrackingResponse(State.TrackingState, out response);
        }


        /// <summary>
        /// Returns a list of the items required to progress through the game.
        /// </summary>
        /// <param name="world">The world to assign to the items.</param>
        /// <returns>A new collection of items.</returns>
        public static List<Item> CreateProgressionPool(World world)
        {
            var itemPool = new List<Item> {
                new Item(ItemType.ProgressiveShield),
                new Item(ItemType.ProgressiveShield),
                new Item(ItemType.ProgressiveShield),
                new Item(ItemType.ProgressiveSword),
                new Item(ItemType.ProgressiveSword),
                new Item(ItemType.Bow),
                new Item(ItemType.Hookshot),
                new Item(ItemType.Mushroom),
                new Item(ItemType.Powder),
                new Item(ItemType.Firerod),
                new Item(ItemType.Icerod),
                new Item(ItemType.Bombos),
                new Item(ItemType.Ether),
                new Item(ItemType.Quake),
                new Item(ItemType.Lamp),
                new Item(ItemType.Hammer),
                new Item(ItemType.Shovel),
                new Item(ItemType.Flute),
                new Item(ItemType.Book),
                new Item(ItemType.Bottle),
                new Item(ItemType.Somaria),
                new Item(ItemType.Byrna),
                new Item(ItemType.Cape),
                new Item(ItemType.Mirror),
                new Item(ItemType.Boots),
                new Item(ItemType.ProgressiveGlove),
                new Item(ItemType.ProgressiveGlove),
                new Item(ItemType.Flippers),
                new Item(ItemType.MoonPearl),
                new Item(ItemType.HalfMagic),

                new Item(ItemType.Grapple),
                new Item(ItemType.Charge),
                new Item(ItemType.Ice),
                new Item(ItemType.Wave),
                new Item(ItemType.Plasma),
                new Item(ItemType.Varia),
                new Item(ItemType.Gravity),
                new Item(ItemType.Morph),
                new Item(ItemType.Bombs),
                new Item(ItemType.SpringBall),
                new Item(ItemType.ScrewAttack),
                new Item(ItemType.HiJump),
                new Item(ItemType.SpaceJump),
                new Item(ItemType.SpeedBooster),

                new Item(ItemType.Missile),
                new Item(ItemType.Super),
                new Item(ItemType.PowerBomb),
                new Item(ItemType.PowerBomb),
                new Item(ItemType.ETank),
                new Item(ItemType.ETank),
                new Item(ItemType.ETank),
                new Item(ItemType.ETank),
                new Item(ItemType.ETank),

                new Item(ItemType.ReserveTank),
                new Item(ItemType.ReserveTank),
                new Item(ItemType.ReserveTank),
                new Item(ItemType.ReserveTank),

                new Item(ItemType.ThreeHundredRupees),
                new Item(ItemType.ThreeHundredRupees),
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
                new Item(ItemType.ProgressiveTunic),
                new Item(ItemType.ProgressiveTunic),
                new Item(ItemType.ProgressiveSword),
                new Item(ItemType.ProgressiveSword),
                new Item(ItemType.SilverArrows),
                new Item(ItemType.BlueBoomerang),
                new Item(ItemType.RedBoomerang),
                new Item(ItemType.Bottle),
                new Item(ItemType.Bottle),
                new Item(ItemType.Bottle),
                new Item(ItemType.Bugnet),
                new Item(ItemType.HeartContainerRefill),

                new Item(ItemType.Spazer),
                new Item(ItemType.XRay),
            };

            itemPool.AddRange(Copies(10, () => new Item(ItemType.HeartContainer, world)));

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
                new Item(ItemType.Arrow),
                new Item(ItemType.OneHundredRupees)
            };

            itemPool.AddRange(Copies(24, () => new Item(ItemType.HeartPiece)));
            itemPool.AddRange(Copies(8, () => new Item(ItemType.TenArrows)));
            itemPool.AddRange(Copies(13, () => new Item(ItemType.ThreeBombs)));
            itemPool.AddRange(Copies(4, () => new Item(ItemType.ArrowUpgrade5)));
            itemPool.AddRange(Copies(4, () => new Item(ItemType.BombUpgrade5)));
            itemPool.AddRange(Copies(2, () => new Item(ItemType.OneRupee)));
            itemPool.AddRange(Copies(4, () => new Item(ItemType.FiveRupees)));
            itemPool.AddRange(Copies(world.Config.MetroidKeysanity ? 25 : 28, () => new Item(ItemType.TwentyRupees)));
            itemPool.AddRange(Copies(7, () => new Item(ItemType.FiftyRupees)));
            itemPool.AddRange(Copies(3, () => new Item(ItemType.ThreeHundredRupees)));

            itemPool.AddRange(Copies(9, () => new Item(ItemType.ETank)));
            itemPool.AddRange(Copies(39, () => new Item(ItemType.Missile)));
            itemPool.AddRange(Copies(15, () => new Item(ItemType.Super)));
            itemPool.AddRange(Copies(8, () => new Item(ItemType.PowerBomb)));

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
                new Item(ItemType.BigKeyEP),
                new Item(ItemType.BigKeyDP),
                new Item(ItemType.BigKeyTH),
                new Item(ItemType.BigKeyPD),
                new Item(ItemType.BigKeySP),
                new Item(ItemType.BigKeySW),
                new Item(ItemType.BigKeyTT),
                new Item(ItemType.BigKeyIP),
                new Item(ItemType.BigKeyMM),
                new Item(ItemType.BigKeyTR),
                new Item(ItemType.BigKeyGT),
            });

            itemPool.AddRange(Copies(1, () => new Item(ItemType.KeyHC)));
            itemPool.AddRange(Copies(2, () => new Item(ItemType.KeyCT)));
            itemPool.AddRange(Copies(1, () => new Item(ItemType.KeyDP)));
            itemPool.AddRange(Copies(1, () => new Item(ItemType.KeyTH)));
            itemPool.AddRange(Copies(6, () => new Item(ItemType.KeyPD)));
            itemPool.AddRange(Copies(1, () => new Item(ItemType.KeySP)));
            itemPool.AddRange(Copies(3, () => new Item(ItemType.KeySW)));
            itemPool.AddRange(Copies(1, () => new Item(ItemType.KeyTT)));
            itemPool.AddRange(Copies(2, () => new Item(ItemType.KeyIP)));
            itemPool.AddRange(Copies(3, () => new Item(ItemType.KeyMM)));
            itemPool.AddRange(Copies(4, () => new Item(ItemType.KeyTR)));
            itemPool.AddRange(Copies(4, () => new Item(ItemType.KeyGT)));

            itemPool.AddRange(new[] {
                new Item(ItemType.MapEP),
                new Item(ItemType.MapDP),
                new Item(ItemType.MapTH),
                new Item(ItemType.MapPD),
                new Item(ItemType.MapSP),
                new Item(ItemType.MapSW),
                new Item(ItemType.MapTT),
                new Item(ItemType.MapIP),
                new Item(ItemType.MapMM),
                new Item(ItemType.MapTR),
            });
            if (world != null && !world.Config.MetroidKeysanity)
            {
                itemPool.AddRange(new[] {
                    new Item(ItemType.MapHC),
                    new Item(ItemType.MapGT),
                    new Item(ItemType.CompassEP),
                    new Item(ItemType.CompassDP),
                    new Item(ItemType.CompassTH),
                    new Item(ItemType.CompassPD),
                    new Item(ItemType.CompassSP),
                    new Item(ItemType.CompassSW),
                    new Item(ItemType.CompassTT),
                    new Item(ItemType.CompassIP),
                    new Item(ItemType.CompassMM),
                    new Item(ItemType.CompassTR),
                    new Item(ItemType.CompassGT),
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
                new Item(ItemType.CardCrateriaL1, world),
                new Item(ItemType.CardCrateriaL2, world),
                new Item(ItemType.CardCrateriaBoss, world),
                new Item(ItemType.CardBrinstarL1, world),
                new Item(ItemType.CardBrinstarL2, world),
                new Item(ItemType.CardBrinstarBoss, world),
                new Item(ItemType.CardNorfairL1, world),
                new Item(ItemType.CardNorfairL2, world),
                new Item(ItemType.CardNorfairBoss, world),
                new Item(ItemType.CardMaridiaL1, world),
                new Item(ItemType.CardMaridiaL2, world),
                new Item(ItemType.CardMaridiaBoss, world),
                new Item(ItemType.CardWreckedShipL1, world),
                new Item(ItemType.CardWreckedShipBoss, world),
                new Item(ItemType.CardLowerNorfairL1, world),
                new Item(ItemType.CardLowerNorfairBoss, world),
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

        /// <summary>
        /// Determines if an item matches the type or name
        /// </summary>
        /// <param name="type">The type to compare against</param>
        /// <param name="name">The name to compare against if the item type is set to Nothing</param>
        /// <see langword="true"/> if the item matches the given type or name
        /// name="type"/> or <paramref name="world"/>; otherwise, <see
        /// langword="false"/>.
        public bool Is(ItemType type, string name)
            => (Type != ItemType.Nothing && Type == type) || (Type == ItemType.Nothing && Name == name);

        /// <summary>
        /// Returns a string that represents the item.
        /// </summary>
        /// <returns>A string representing this item.</returns>
        public override string ToString() => $"{Name}";

        private static List<Item> Copies(int nr, Func<Item> template)
        {
            return Enumerable.Range(1, nr).Select(i => template()).ToList();
        }
    }
}
