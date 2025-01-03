using System;
using System.Linq;
using System.Collections.Generic;
using static Randomizer.SMZ3.LegacyItemType;
using static Randomizer.SMZ3.LegacySMLogic;
using static Randomizer.SMZ3.LegacyRewardType;
using System.Text.RegularExpressions;
using System.ComponentModel;
using TrackerCouncil.Smz3.Shared;

namespace Randomizer.SMZ3 {

    public enum LegacyItemType : byte {
        [Description("Nothing")]
        Nothing,

        /* This is used for the Random starting items */
        [Description("Random")]
        Random,

        [Description("Hyrule Castle Map")]
        MapHC = 0x7F,
        [Description("Eastern Palace Map")]
        MapEP = 0x7D,
        [Description("Desert Palace Map")]
        MapDP = 0x7C,
        [Description("Tower of Hera Map")]
        MapTH = 0x75,
        [Description("Palace of Darkness Map")]
        MapPD = 0x79,
        [Description("Swamp Palace Map")]
        MapSP = 0x7A,
        [Description("Skull Woods Map")]
        MapSW = 0x77,
        [Description("Thieves Town Map")]
        MapTT = 0x74,
        [Description("Ice Palace Map")]
        MapIP = 0x76,
        [Description("Misery Mire Map")]
        MapMM = 0x78,
        [Description("Turtle Rock Map")]
        MapTR = 0x73,
        [Description("Ganons Tower Map")]
        MapGT = 0x72,

        [Description("Eastern Palace Compass")]
        CompassEP = 0x8D,
        [Description("Desert Palace Compass")]
        CompassDP = 0x8C,
        [Description("Tower of Hera Compass")]
        CompassTH = 0x85,
        [Description("Palace of Darkness Compass")]
        CompassPD = 0x89,
        [Description("Swamp Palace Compass")]
        CompassSP = 0x8A,
        [Description("Skull Woods Compass")]
        CompassSW = 0x87,
        [Description("Thieves Town Compass")]
        CompassTT = 0x84,
        [Description("Ice Palace Compass")]
        CompassIP = 0x86,
        [Description("Misery Mire Compass")]
        CompassMM = 0x88,
        [Description("Turtle Rock Compass")]
        CompassTR = 0x83,
        [Description("Ganons Tower Compass")]
        CompassGT = 0x82,

        [Description("Eastern Palace Big Key")]
        BigKeyEP = 0x9D,
        [Description("Desert Palace Big Key")]
        BigKeyDP = 0x9C,
        [Description("Tower of Hera Big Key")]
        BigKeyTH = 0x95,
        [Description("Palace of Darkness Big Key")]
        BigKeyPD = 0x99,
        [Description("Swamp Palace Big Key")]
        BigKeySP = 0x9A,
        [Description("Skull Woods Big Key")]
        BigKeySW = 0x97,
        [Description("Thieves Town Big Key")]
        BigKeyTT = 0x94,
        [Description("Ice Palace Big Key")]
        BigKeyIP = 0x96,
        [Description("Misery Mire Big Key")]
        BigKeyMM = 0x98,
        [Description("Turtle Rock Big Key")]
        BigKeyTR = 0x93,
        [Description("Ganons Tower Big Key")]
        BigKeyGT = 0x92,

        [Description("Sewer Key")]
        KeyHC = 0xA0,
        [Description("Castle Tower Key")]
        KeyCT = 0xA4,
        [Description("Desert Palace Key")]
        KeyDP = 0xA3,
        [Description("Tower of Hera Key")]
        KeyTH = 0xAA,
        [Description("Palace of Darkness Key")]
        KeyPD = 0xA6,
        [Description("Swamp Palace Key")]
        KeySP = 0xA5,
        [Description("Skull Woods Key")]
        KeySW = 0xA8,
        [Description("Thieves Town Key")]
        KeyTT = 0xAB,
        [Description("Ice Palace Key")]
        KeyIP = 0xA9,
        [Description("Misery Mire Key")]
        KeyMM = 0xA7,
        [Description("Turtle Rock Key")]
        KeyTR = 0xAC,
        [Description("Ganons Tower Key")]
        KeyGT = 0xAD,

        [Description("Small Key")]
        Key = 0x24,
        [Description("Compass")]
        Compass = 0x25,
        [Description("Big Key")]
        BigKey = 0x32,
        [Description("Map")]
        Map = 0x33,


        [Description("Progressive Mail")]
        ProgressiveTunic = 0x60,
        [Description("Progressive Shield")]
        ProgressiveShield = 0x5F,
        [Description("Progressive Sword")]
        ProgressiveSword = 0x5E,
        [Description("Bow")]
        Bow = 0x0B,
        [Description("Silver Arrows")]
        SilverArrows = 0x58,
        [Description("Blue Boomerang")]
        BlueBoomerang = 0x0C,
        [Description("Red Boomerang")]
        RedBoomerang = 0x2A,
        [Description("Hookshot")]
        Hookshot = 0x0A,
        [Description("Mushroom")]
        Mushroom = 0x29,
        [Description("Magic Powder")]
        Powder = 0x0D,
        [Description("Fire Rod")]
        Firerod = 0x07,
        [Description("Ice Rod")]
        Icerod = 0x08,
        [Description("Bombos")]
        Bombos = 0x0f,
        [Description("Ether")]
        Ether = 0x10,
        [Description("Quake")]
        Quake = 0x11,
        [Description("Lamp")]
        Lamp = 0x12,
        [Description("Hammer")]
        Hammer = 0x09,
        [Description("Shovel")]
        Shovel = 0x13,
        [Description("Flute")]
        Flute = 0x14,
        [Description("Bug Catching Net")]
        Bugnet = 0x21,
        [Description("Book of Mudora")]
        Book = 0x1D,
        [Description("Bottle")]
        Bottle = 0x16,
        [Description("Cane of Somaria")]
        Somaria = 0x15,
        [Description("Cane of Byrna")]
        Byrna = 0x18,
        [Description("Magic Cape")]
        Cape = 0x19,
        [Description("Magic Mirror")]
        Mirror = 0x1A,
        [Description("Pegasus Boots")]
        Boots = 0x4B,
        [Description("Progressive Glove")]
        ProgressiveGlove = 0x61,
        [Description("Zora's Flippers")]
        Flippers = 0x1E,
        [Description("Moon Pearl")]
        MoonPearl = 0x1F,
        [Description("Half Magic")]
        HalfMagic = 0x4E,
        [Description("Piece of Heart")]
        HeartPiece = 0x17,
        [Description("Heart Container")]
        HeartContainer = 0x3E,
        [Description("Sanctuary Heart Container")]
        HeartContainerRefill = 0x3F,
        [Description("Three Bombs")]
        ThreeBombs = 0x28,
        [Description("Single Arrow")]
        Arrow = 0x43,
        [Description("Ten Arrows")]
        TenArrows = 0x44,
        [Description("One Rupee")]
        OneRupee = 0x34,
        [Description("Five Rupees")]
        FiveRupees = 0x35,
        [Description("Twenty Rupees")]
        TwentyRupees = 0x36,
        [Description("Twenty Rupees")]
        TwentyRupees2 = 0x47,
        [Description("Fifty Rupees")]
        FiftyRupees = 0x41,
        [Description("One Hundred Rupees")]
        OneHundredRupees = 0x40,
        [Description("Three Hundred Rupees")]
        ThreeHundredRupees = 0x46,
        [Description("+5 Bomb Capacity")]
        BombUpgrade5 = 0x51,
        [Description("+10 Bomb Capacity")]
        BombUpgrade10 = 0x52,
        [Description("+5 Arrow Capacity")]
        ArrowUpgrade5 = 0x53,
        [Description("+10 Arrow Capacity")]
        ArrowUpgrade10 = 0x54,

        [Description("Crateria Level 1 Keycard")]
        CardCrateriaL1 = 0xD0,
        [Description("Crateria Level 2 Keycard")]
        CardCrateriaL2 = 0xD1,
        [Description("Crateria Boss Keycard")]
        CardCrateriaBoss = 0xD2,
        [Description("Brinstar Level 1 Keycard")]
        CardBrinstarL1 = 0xD3,
        [Description("Brinstar Level 2 Keycard")]
        CardBrinstarL2 = 0xD4,
        [Description("Brinstar Boss Keycard")]
        CardBrinstarBoss = 0xD5,
        [Description("Norfair Level 1 Keycard")]
        CardNorfairL1 = 0xD6,
        [Description("Norfair Level 2 Keycard")]
        CardNorfairL2 = 0xD7,
        [Description("Norfair Boss Keycard")]
        CardNorfairBoss = 0xD8,
        [Description("Maridia Level 1 Keycard")]
        CardMaridiaL1 = 0xD9,
        [Description("Maridia Level 2 Keycard")]
        CardMaridiaL2 = 0xDA,
        [Description("Maridia Boss Keycard")]
        CardMaridiaBoss = 0xDB,
        [Description("Wrecked Ship Level 1 Keycard")]
        CardWreckedShipL1 = 0xDC,
        [Description("Wrecked Ship Boss Keycard")]
        CardWreckedShipBoss = 0xDD,
        [Description("Lower Norfair Level 1 Keycard")]
        CardLowerNorfairL1 = 0xDE,
        [Description("Lower Norfair Boss Keycard")]
        CardLowerNorfairBoss = 0xDF,

        [Description("Brinstar Map")]
        SmMapBrinstar = 0xCA,
        [Description("Wrecked Ship Map")]
        SmMapWreckedShip = 0xCB,
        [Description("Maridia Map")]
        SmMapMaridia = 0xCC,
        [Description("Lower Norfair Map")]
        SmMapLowerNorfair = 0xCD,

        [Description("Missile")]
        Missile = 0xC2,
        [Description("Super Missile")]
        Super = 0xC3,
        [Description("Power Bomb")]
        PowerBomb = 0xC4,
        [Description("Grappling Beam")]
        Grapple = 0xB0,
        [Description("X-Ray Scope")]
        XRay = 0xB1,
        [Description("Energy Tank")]
        ETank = 0xC0,
        [Description("Reserve Tank")]
        ReserveTank = 0xC1,
        [Description("Charge Beam")]
        Charge = 0xBB,
        [Description("Ice Beam")]
        Ice = 0xBC,
        [Description("Wave Beam")]
        Wave = 0xBD,
        [Description("Spazer")]
        Spazer = 0xBE,
        [Description("Plasma Beam")]
        Plasma = 0xBF,
        [Description("Varia Suit")]
        Varia = 0xB2,
        [Description("Gravity Suit")]
        Gravity = 0xB6,
        [Description("Morphing Ball")]
        Morph = 0xB4,
        [Description("Morph Bombs")]
        Bombs = 0xB9,
        [Description("Spring Ball")]
        SpringBall = 0xB3,
        [Description("Screw Attack")]
        ScrewAttack = 0xB5,
        [Description("Hi-Jump Boots")]
        HiJump = 0xB7,
        [Description("Space Jump")]
        SpaceJump = 0xB8,
        [Description("Speed Booster")]
        SpeedBooster = 0xBA,

        [Description("Bottle with Red Potion")]
        BottleWithRedPotion = 0x2B,
        [Description("Bottle with Green Potion")]
        BottleWithGreenPotion = 0x2C,
        [Description("Bottle with Blue Potion")]
        BottleWithBluePotion = 0x2D,
        [Description("Bottle with Fairy")]
        BottleWithFairy = 0x3D,
        [Description("Bottle with Bee")]
        BottleWithBee = 0x3C,
        [Description("Bottle with Gold Bee")]
        BottleWithGoldBee = 0x48,
        [Description("Red Potion Refill")]
        RedContent = 0x2E,
        [Description("Green Potion Refill")]
        GreenContent = 0x2F,
        [Description("Blue Potion Refill")]
        BlueContent = 0x30,
        [Description("Bee Refill")]
        BeeContent = 0x0E,
    }

    class LegacyItem {

        public string Name { get; set; }
        public LegacyItemType Type { get; set; }
        public bool Progression { get; set; }
        public LegacyWorld LegacyWorld { get; set; }

        static readonly Regex dungeon = new("^(BigKey|Key|Map|Compass)");
        static readonly Regex bigKey = new("^BigKey");
        static readonly Regex key = new("^Key");
        static readonly Regex map = new("^Map");
        static readonly Regex compass = new("^Compass");
        static readonly Regex keycard = new("^Card");
        static readonly Regex smMap = new("^SmMap");

        public bool IsDungeonItem => dungeon.IsMatch(Type.ToString());
        public bool IsBigKey => bigKey.IsMatch(Type.ToString());
        public bool IsKey => key.IsMatch(Type.ToString());
        public bool IsMap => map.IsMatch(Type.ToString());
        public bool IsCompass => compass.IsMatch(Type.ToString());
        public bool IsKeycard => keycard.IsMatch(Type.ToString());
        public bool IsSmMap => smMap.IsMatch(Type.ToString());

        public bool Is(LegacyItemType type, LegacyWorld legacyWorld) => Type == type && LegacyWorld == legacyWorld;
        public bool IsNot(LegacyItemType type, LegacyWorld legacyWorld) => !Is(type, legacyWorld);

        public LegacyItem(LegacyItemType legacyItemType) {
            Name = legacyItemType.GetDescription();
            Type = legacyItemType;
        }

        public LegacyItem(LegacyItemType legacyItemType, LegacyWorld legacyWorld) : this(legacyItemType) {
            LegacyWorld = legacyWorld;
        }

        public static LegacyItem Nothing(LegacyWorld legacyWorld) {
            return new LegacyItem(LegacyItemType.Nothing, legacyWorld);
        }

        public static List<LegacyItem> CreateProgressionPool(LegacyWorld legacyWorld) {
            var itemPool = new List<LegacyItem> {
                new LegacyItem(ProgressiveShield),
                new LegacyItem(ProgressiveShield),
                new LegacyItem(ProgressiveShield),
                new LegacyItem(ProgressiveSword),
                new LegacyItem(ProgressiveSword),
                new LegacyItem(Bow),
                new LegacyItem(Hookshot),
                new LegacyItem(Mushroom),
                new LegacyItem(Powder),
                new LegacyItem(Firerod),
                new LegacyItem(Icerod),
                new LegacyItem(Bombos),
                new LegacyItem(Ether),
                new LegacyItem(Quake),
                new LegacyItem(Lamp),
                new LegacyItem(Hammer),
                new LegacyItem(Shovel),
                new LegacyItem(Flute),
                new LegacyItem(Bugnet),
                new LegacyItem(Book),
                new LegacyItem(Bottle),
                new LegacyItem(Somaria),
                new LegacyItem(Byrna),
                new LegacyItem(Cape),
                new LegacyItem(Mirror),
                new LegacyItem(Boots),
                new LegacyItem(ProgressiveGlove),
                new LegacyItem(ProgressiveGlove),
                new LegacyItem(Flippers),
                new LegacyItem(MoonPearl),
                new LegacyItem(HalfMagic),

                new LegacyItem(Grapple),
                new LegacyItem(Charge),
                new LegacyItem(Ice),
                new LegacyItem(Wave),
                new LegacyItem(Plasma),
                new LegacyItem(Varia),
                new LegacyItem(Gravity),
                new LegacyItem(Morph),
                new LegacyItem(Bombs),
                new LegacyItem(SpringBall),
                new LegacyItem(ScrewAttack),
                new LegacyItem(HiJump),
                new LegacyItem(SpaceJump),
                new LegacyItem(SpeedBooster),

                new LegacyItem(Missile),
                new LegacyItem(Super),
                new LegacyItem(PowerBomb),
                new LegacyItem(PowerBomb),
                new LegacyItem(ETank),
                new LegacyItem(ETank),
                new LegacyItem(ETank),
                new LegacyItem(ETank),
                new LegacyItem(ETank),

                new LegacyItem(ReserveTank),
                new LegacyItem(ReserveTank),
                new LegacyItem(ReserveTank),
                new LegacyItem(ReserveTank),
            };

            foreach (var item in itemPool) {
                item.Progression = true;
                item.LegacyWorld = legacyWorld;
            }

            return itemPool;
        }

        public static List<LegacyItem> CreateNicePool(LegacyWorld legacyWorld) {
            var itemPool = new List<LegacyItem> {
                new LegacyItem(ProgressiveTunic),
                new LegacyItem(ProgressiveTunic),
                new LegacyItem(ProgressiveSword),
                new LegacyItem(ProgressiveSword),
                new LegacyItem(SilverArrows),
                new LegacyItem(BlueBoomerang),
                new LegacyItem(RedBoomerang),
                new LegacyItem(Bottle),
                new LegacyItem(Bottle),
                new LegacyItem(Bottle),
                new LegacyItem(HeartContainerRefill),

                new LegacyItem(Spazer),
                new LegacyItem(XRay),
            };

            itemPool.AddRange(Copies(10, () => new LegacyItem(HeartContainer, legacyWorld)));

            foreach (var item in itemPool) item.LegacyWorld = legacyWorld;

            return itemPool;
        }

        public static List<LegacyItem> CreateJunkPool(LegacyWorld legacyWorld) {
            var itemPool = new List<LegacyItem> {
                new LegacyItem(Arrow),
                new LegacyItem(OneHundredRupees)
            };

            itemPool.AddRange(Copies(24, () => new LegacyItem(HeartPiece)));
            itemPool.AddRange(Copies(8,  () => new LegacyItem(TenArrows)));
            itemPool.AddRange(Copies(13, () => new LegacyItem(ThreeBombs)));
            itemPool.AddRange(Copies(4,  () => new LegacyItem(ArrowUpgrade5)));
            itemPool.AddRange(Copies(4,  () => new LegacyItem(BombUpgrade5)));
            itemPool.AddRange(Copies(2,  () => new LegacyItem(OneRupee)));
            itemPool.AddRange(Copies(4,  () => new LegacyItem(FiveRupees)));
            itemPool.AddRange(Copies(legacyWorld.LegacyConfig.Keysanity ? 21 : 28, () => new LegacyItem(TwentyRupees)));
            itemPool.AddRange(Copies(7,  () => new LegacyItem(FiftyRupees)));
            itemPool.AddRange(Copies(5,  () => new LegacyItem(ThreeHundredRupees)));

            itemPool.AddRange(Copies(9,  () => new LegacyItem(ETank)));
            itemPool.AddRange(Copies(39, () => new LegacyItem(Missile)));
            itemPool.AddRange(Copies(15, () => new LegacyItem(Super)));
            itemPool.AddRange(Copies(8,  () => new LegacyItem(PowerBomb)));

            foreach (var item in itemPool) item.LegacyWorld = legacyWorld;

            return itemPool;
        }

        /* The order of the dungeon pool is significant */
        public static List<LegacyItem> CreateDungeonPool(LegacyWorld legacyWorld) {
            var itemPool = new List<LegacyItem>();

            itemPool.AddRange(new[] {
                new LegacyItem(BigKeyEP),
                new LegacyItem(BigKeyDP),
                new LegacyItem(BigKeyTH),
                new LegacyItem(BigKeyPD),
                new LegacyItem(BigKeySP),
                new LegacyItem(BigKeySW),
                new LegacyItem(BigKeyTT),
                new LegacyItem(BigKeyIP),
                new LegacyItem(BigKeyMM),
                new LegacyItem(BigKeyTR),
                new LegacyItem(BigKeyGT),
            });

            itemPool.AddRange(Copies(1, () => new LegacyItem(KeyHC)));
            itemPool.AddRange(Copies(2, () => new LegacyItem(KeyCT)));
            itemPool.AddRange(Copies(1, () => new LegacyItem(KeyDP)));
            itemPool.AddRange(Copies(1, () => new LegacyItem(KeyTH)));
            itemPool.AddRange(Copies(6, () => new LegacyItem(KeyPD)));
            itemPool.AddRange(Copies(1, () => new LegacyItem(KeySP)));
            itemPool.AddRange(Copies(3, () => new LegacyItem(KeySW)));
            itemPool.AddRange(Copies(1, () => new LegacyItem(KeyTT)));
            itemPool.AddRange(Copies(2, () => new LegacyItem(KeyIP)));
            itemPool.AddRange(Copies(3, () => new LegacyItem(KeyMM)));
            itemPool.AddRange(Copies(4, () => new LegacyItem(KeyTR)));
            itemPool.AddRange(Copies(4, () => new LegacyItem(KeyGT)));

            itemPool.AddRange(new[] {
                new LegacyItem(MapEP),
                new LegacyItem(MapDP),
                new LegacyItem(MapTH),
                new LegacyItem(MapPD),
                new LegacyItem(MapSP),
                new LegacyItem(MapSW),
                new LegacyItem(MapTT),
                new LegacyItem(MapIP),
                new LegacyItem(MapMM),
                new LegacyItem(MapTR),
            });
            if (!legacyWorld.LegacyConfig.Keysanity) {
                itemPool.AddRange(new[] {
                    new LegacyItem(MapHC),
                    new LegacyItem(MapGT),
                    new LegacyItem(CompassEP),
                    new LegacyItem(CompassDP),
                    new LegacyItem(CompassTH),
                    new LegacyItem(CompassPD),
                    new LegacyItem(CompassSP),
                    new LegacyItem(CompassSW),
                    new LegacyItem(CompassTT),
                    new LegacyItem(CompassIP),
                    new LegacyItem(CompassMM),
                    new LegacyItem(CompassTR),
                    new LegacyItem(CompassGT),
                });
            }

            foreach (var item in itemPool) item.LegacyWorld = legacyWorld;

            return itemPool;
        }

        public static List<LegacyItem> CreateKeycards(LegacyWorld legacyWorld) {
            return new List<LegacyItem> {
                new LegacyItem(CardCrateriaL1, legacyWorld),
                new LegacyItem(CardCrateriaL2, legacyWorld),
                new LegacyItem(CardCrateriaBoss, legacyWorld),
                new LegacyItem(CardBrinstarL1, legacyWorld),
                new LegacyItem(CardBrinstarL2, legacyWorld),
                new LegacyItem(CardBrinstarBoss, legacyWorld),
                new LegacyItem(CardNorfairL1, legacyWorld),
                new LegacyItem(CardNorfairL2, legacyWorld),
                new LegacyItem(CardNorfairBoss, legacyWorld),
                new LegacyItem(CardMaridiaL1, legacyWorld),
                new LegacyItem(CardMaridiaL2, legacyWorld),
                new LegacyItem(CardMaridiaBoss, legacyWorld),
                new LegacyItem(CardWreckedShipL1, legacyWorld),
                new LegacyItem(CardWreckedShipBoss, legacyWorld),
                new LegacyItem(CardLowerNorfairL1, legacyWorld),
                new LegacyItem(CardLowerNorfairBoss, legacyWorld),
            };
        }

        public static List<LegacyItem> CreateSmMaps(LegacyWorld legacyWorld) {
            return new List<LegacyItem> {
                new LegacyItem(SmMapBrinstar, legacyWorld),
                new LegacyItem(SmMapWreckedShip, legacyWorld),
                new LegacyItem(SmMapMaridia, legacyWorld),
                new LegacyItem(SmMapLowerNorfair, legacyWorld),
            };
        }

        static List<LegacyItem> Copies(int nr, Func<LegacyItem> template) {
            return Enumerable.Range(1, nr).Select(i => template()).ToList();
        }

    }

    static class ItemsExtensions {

        public static LegacyItem Get(this IEnumerable<LegacyItem> items, LegacyItemType legacyItemType) {
            var item = items.FirstOrDefault(i => i.Type == legacyItemType);
            if (item == null)
                throw new InvalidOperationException($"Could not find an item of type {legacyItemType}");
            return item;
        }

        public static LegacyItem Get(this IEnumerable<LegacyItem> items, LegacyItemType legacyItemType, LegacyWorld legacyWorld) {
            var item = items.FirstOrDefault(i => i.Is(legacyItemType, legacyWorld));
            if (item == null)
                throw new InvalidOperationException($"Could not find an item of type {legacyItemType} in world {legacyWorld.Id}");
            return item;
        }

    }

    public class LegacyProgression {

        public bool BigKeyEP { get; private set; }
        public bool BigKeyDP { get; private set; }
        public bool BigKeyTH { get; private set; }
        public bool BigKeyPD { get; private set; }
        public bool BigKeySP { get; private set; }
        public bool BigKeySW { get; private set; }
        public bool BigKeyTT { get; private set; }
        public bool BigKeyIP { get; private set; }
        public bool BigKeyMM { get; private set; }
        public bool BigKeyTR { get; private set; }
        public bool BigKeyGT { get; private set; }
        public bool KeyHC { get; private set; }
        public bool KeyDP { get; private set; }
        public bool KeyTH { get; private set; }
        public bool KeySP { get; private set; }
        public bool KeyTT { get; private set; }
        public int KeyCT { get; private set; }
        public int KeyPD { get; private set; }
        public int KeySW { get; private set; }
        public int KeyIP { get; private set; }
        public int KeyMM { get; private set; }
        public int KeyTR { get; private set; }
        public int KeyGT { get; private set; }
        public bool CardCrateriaL1 { get; private set; }
        public bool CardCrateriaL2 { get; private set; }
        public bool CardCrateriaBoss { get; private set; }
        public bool CardBrinstarL1 { get; private set; }
        public bool CardBrinstarL2 { get; private set; }
        public bool CardBrinstarBoss { get; private set; }
        public bool CardNorfairL1 { get; private set; }
        public bool CardNorfairL2 { get; private set; }
        public bool CardNorfairBoss { get; private set; }
        public bool CardMaridiaL1 { get; private set; }
        public bool CardMaridiaL2 { get; private set; }
        public bool CardMaridiaBoss { get; private set; }
        public bool CardWreckedShipL1 { get; private set; }
        public bool CardWreckedShipBoss { get; private set; }
        public bool CardLowerNorfairL1 { get; private set; }
        public bool CardLowerNorfairBoss { get; private set; }
        public bool CanBlockLasers { get { return shield >= 3; } }
        public bool Sword { get; private set; }
        public bool MasterSword { get; private set; }
        public bool Bow { get; private set; }
        public bool Hookshot { get; private set; }
        public bool Mushroom { get; private set; }
        public bool Powder { get; private set; }
        public bool Firerod { get; private set; }
        public bool Icerod { get; private set; }
        public bool Bombos { get; private set; }
        public bool Ether { get; private set; }
        public bool Quake { get; private set; }
        public bool Lamp { get; private set; }
        public bool Hammer { get; private set; }
        public bool Shovel { get; private set; }
        public bool Flute { get; private set; }
        public bool Book { get; private set; }
        public bool Bottle { get; private set; }
        public bool Somaria { get; private set; }
        public bool Byrna { get; private set; }
        public bool Cape { get; private set; }
        public bool Mirror { get; private set; }
        public bool Boots { get; private set; }
        public bool Glove { get; private set; }
        public bool Mitt { get; private set; }
        public bool Flippers { get; private set; }
        public bool MoonPearl { get; private set; }
        public bool HalfMagic { get; private set; }
        public bool Grapple { get; private set; }
        public bool Charge { get; private set; }
        public bool Ice { get; private set; }
        public bool Wave { get; private set; }
        public bool Plasma { get; private set; }
        public bool Varia { get; private set; }
        public bool Gravity { get; private set; }
        public bool Morph { get; private set; }
        public bool Bombs { get; private set; }
        public bool SpringBall { get; private set; }
        public bool ScrewAttack { get; private set; }
        public bool HiJump { get; private set; }
        public bool SpaceJump { get; private set; }
        public bool SpeedBooster { get; private set; }
        public bool Missile { get; private set; }
        public bool Super { get; private set; }
        public bool PowerBomb { get; private set; }
        public bool TwoPowerBombs { get; private set; }
        public int ETank { get; private set; }
        public int ReserveTank { get; private set; }

        int shield;

        public LegacyProgression(List<int> itemTypeValues)
        {
            Add(itemTypeValues.Select(x => new LegacyItem((LegacyItemType)x)));
        }

        internal LegacyProgression(IEnumerable<LegacyItem> items) {
            Add(items);
        }

        internal void Add(IEnumerable<LegacyItem> items) {
            foreach (var item in items) {
                bool done = item.Type switch {
                    LegacyItemType.BigKeyEP => BigKeyEP = true,
                    LegacyItemType.BigKeyDP => BigKeyDP = true,
                    LegacyItemType.BigKeyTH => BigKeyTH = true,
                    LegacyItemType.BigKeyPD => BigKeyPD = true,
                    LegacyItemType.BigKeySP => BigKeySP = true,
                    LegacyItemType.BigKeySW => BigKeySW = true,
                    LegacyItemType.BigKeyTT => BigKeyTT = true,
                    LegacyItemType.BigKeyIP => BigKeyIP = true,
                    LegacyItemType.BigKeyMM => BigKeyMM = true,
                    LegacyItemType.BigKeyTR => BigKeyTR = true,
                    LegacyItemType.BigKeyGT => BigKeyGT = true,
                    LegacyItemType.KeyHC => KeyHC = true,
                    LegacyItemType.KeyDP => KeyDP = true,
                    LegacyItemType.KeyTH => KeyTH = true,
                    LegacyItemType.KeySP => KeySP = true,
                    LegacyItemType.KeyTT => KeyTT = true,
                    LegacyItemType.CardCrateriaL1 => CardCrateriaL1 = true,
                    LegacyItemType.CardCrateriaL2 => CardCrateriaL2 = true,
                    LegacyItemType.CardCrateriaBoss => CardCrateriaBoss = true,
                    LegacyItemType.CardBrinstarL1 => CardBrinstarL1 = true,
                    LegacyItemType.CardBrinstarL2 => CardBrinstarL2 = true,
                    LegacyItemType.CardBrinstarBoss => CardBrinstarBoss = true,
                    LegacyItemType.CardNorfairL1 => CardNorfairL1 = true,
                    LegacyItemType.CardNorfairL2 => CardNorfairL2 = true,
                    LegacyItemType.CardNorfairBoss => CardNorfairBoss = true,
                    LegacyItemType.CardMaridiaL1 => CardMaridiaL1 = true,
                    LegacyItemType.CardMaridiaL2 => CardMaridiaL2 = true,
                    LegacyItemType.CardMaridiaBoss => CardMaridiaBoss = true,
                    LegacyItemType.CardWreckedShipL1 => CardWreckedShipL1 = true,
                    LegacyItemType.CardWreckedShipBoss => CardWreckedShipBoss = true,
                    LegacyItemType.CardLowerNorfairL1 => CardLowerNorfairL1 = true,
                    LegacyItemType.CardLowerNorfairBoss => CardLowerNorfairBoss = true,
                    LegacyItemType.Bow => Bow = true,
                    LegacyItemType.Hookshot => Hookshot = true,
                    LegacyItemType.Mushroom => Mushroom = true,
                    LegacyItemType.Powder => Powder = true,
                    LegacyItemType.Firerod => Firerod = true,
                    LegacyItemType.Icerod => Icerod = true,
                    LegacyItemType.Bombos => Bombos = true,
                    LegacyItemType.Ether => Ether = true,
                    LegacyItemType.Quake => Quake = true,
                    LegacyItemType.Lamp => Lamp = true,
                    LegacyItemType.Hammer => Hammer = true,
                    LegacyItemType.Shovel => Shovel = true,
                    LegacyItemType.Flute => Flute = true,
                    LegacyItemType.Book => Book = true,
                    LegacyItemType.Bottle => Bottle = true,
                    LegacyItemType.Somaria => Somaria = true,
                    LegacyItemType.Byrna => Byrna = true,
                    LegacyItemType.Cape => Cape = true,
                    LegacyItemType.Mirror => Mirror = true,
                    LegacyItemType.Boots => Boots = true,
                    LegacyItemType.Flippers => Flippers = true,
                    LegacyItemType.MoonPearl => MoonPearl = true,
                    LegacyItemType.HalfMagic => HalfMagic = true,
                    LegacyItemType.Grapple => Grapple = true,
                    LegacyItemType.Charge => Charge = true,
                    LegacyItemType.Ice => Ice = true,
                    LegacyItemType.Wave => Wave = true,
                    LegacyItemType.Plasma => Plasma = true,
                    LegacyItemType.Varia => Varia = true,
                    LegacyItemType.Gravity => Gravity = true,
                    LegacyItemType.Morph => Morph = true,
                    LegacyItemType.Bombs => Bombs = true,
                    LegacyItemType.SpringBall => SpringBall = true,
                    LegacyItemType.ScrewAttack => ScrewAttack = true,
                    LegacyItemType.HiJump => HiJump = true,
                    LegacyItemType.SpaceJump => SpaceJump = true,
                    LegacyItemType.SpeedBooster => SpeedBooster = true,
                    LegacyItemType.Missile => Missile = true,
                    LegacyItemType.Super => Super = true,
                    _ => false
                };

                if (done)
                    continue;

                switch (item.Type) {
                    case LegacyItemType.KeyCT: KeyCT += 1; break;
                    case LegacyItemType.KeyPD: KeyPD += 1; break;
                    case LegacyItemType.KeySW: KeySW += 1; break;
                    case LegacyItemType.KeyIP: KeyIP += 1; break;
                    case LegacyItemType.KeyMM: KeyMM += 1; break;
                    case LegacyItemType.KeyTR: KeyTR += 1; break;
                    case LegacyItemType.KeyGT: KeyGT += 1; break;
                    case LegacyItemType.ETank: ETank += 1; break;
                    case LegacyItemType.ReserveTank: ReserveTank += 1; break;
                    case ProgressiveShield: shield += 1; break;
                    case ProgressiveSword:
                        MasterSword = Sword;
                        Sword = true;
                        break;
                    case ProgressiveGlove:
                        Mitt = Glove;
                        Glove = true;
                        break;
                    case LegacyItemType.PowerBomb:
                        TwoPowerBombs = PowerBomb;
                        PowerBomb = true;
                        break;
                }
            }
        }
    }

    static class ProgressionExtensions {

        public static bool CanLiftLight(this LegacyProgression items) => items.Glove;
        public static bool CanLiftHeavy(this LegacyProgression items) => items.Mitt;

        public static bool CanLightTorches(this LegacyProgression items) {
            return items.Firerod || items.Lamp;
        }

        public static bool CanMeltFreezors(this LegacyProgression items) {
            return items.Firerod || items.Bombos && items.Sword;
        }

        public static bool CanExtendMagic(this LegacyProgression items, int bars = 2) {
            return (items.HalfMagic ? 2 : 1) * (items.Bottle ? 2 : 1) >= bars;
        }

        public static bool CanKillManyEnemies(this LegacyProgression items) {
            return items.Sword || items.Hammer || items.Bow || items.Firerod ||
                items.Somaria || items.Byrna && items.CanExtendMagic();
        }

        public static bool CanAccessDeathMountainPortal(this LegacyProgression items) {
            return (items.CanDestroyBombWalls() || items.SpeedBooster) && items.Super && items.Morph;
        }

        public static bool CanAccessDarkWorldPortal(this LegacyProgression items, LegacyConfig legacyConfig) {
            return legacyConfig.LegacySmLogic switch {
                Normal =>
                    items.CardMaridiaL1 && items.CardMaridiaL2 && items.CanUsePowerBombs() && items.Super && items.Gravity && items.SpeedBooster,
                _ =>
                    items.CardMaridiaL1 && items.CardMaridiaL2 && items.CanUsePowerBombs() && items.Super &&
                    (items.Charge || items.Super && items.Missile) &&
                    (items.Gravity || items.HiJump && items.Ice && items.Grapple) &&
                    (items.Ice || items.Gravity && items.SpeedBooster)
            };
        }

        public static bool CanAccessMiseryMirePortal(this LegacyProgression items, LegacyConfig legacyConfig) {
            return legacyConfig.LegacySmLogic switch {
                Normal =>
                    (items.CardNorfairL2 || items.SpeedBooster && items.Wave) && items.Varia && items.Super &&
                        items.Gravity && items.SpaceJump && items.CanUsePowerBombs(),
                _ =>
                    (items.CardNorfairL2 || items.SpeedBooster) && items.Varia && items.Super && (
                        items.CanFly() || items.HiJump || items.SpeedBooster || items.CanSpringBallJump() || items.Ice
                    ) && (items.Gravity || items.HiJump) && items.CanUsePowerBombs()
             };
        }

        public static bool CanIbj(this LegacyProgression items) {
            return items.Morph && items.Bombs;
        }

        public static bool CanFly(this LegacyProgression items) {
            return items.SpaceJump || items.CanIbj();
        }

        public static bool CanUsePowerBombs(this LegacyProgression items) {
            return items.Morph && items.PowerBomb;
        }

        public static bool CanPassBombPassages(this LegacyProgression items) {
            return items.Morph && (items.Bombs || items.PowerBomb);
        }

        public static bool CanDestroyBombWalls(this LegacyProgression items) {
            return items.CanPassBombPassages() || items.ScrewAttack;
        }

        public static bool CanSpringBallJump(this LegacyProgression items) {
            return items.Morph && items.SpringBall;
        }

        public static bool CanHellRun(this LegacyProgression items) {
            return items.Varia || items.HasEnergyReserves(5);
        }

        public static bool HasEnergyReserves(this LegacyProgression items, int amount) {
            return (items.ETank + items.ReserveTank) >= amount;
        }

        public static bool CanOpenRedDoors(this LegacyProgression items) {
            return items.Missile || items.Super;
        }

        public static bool CanAccessNorfairUpperPortal(this LegacyProgression items) {
            return items.Flute || items.CanLiftLight() && items.Lamp;
        }

        public static bool CanAccessNorfairLowerPortal(this LegacyProgression items) {
            return items.Flute && items.CanLiftHeavy();
        }

        public static bool CanAccessMaridiaPortal(this LegacyProgression items, LegacyWorld legacyWorld) {
            return legacyWorld.LegacyConfig.LegacySmLogic switch {
                Normal =>
                    items.MoonPearl && items.Flippers &&
                    items.Gravity && items.Morph &&
                    (legacyWorld.CanAcquire(items, Agahnim) || items.Hammer && items.CanLiftLight() || items.CanLiftHeavy()),
                _ =>
                    items.MoonPearl && items.Flippers &&
                    (items.CanSpringBallJump() || items.HiJump || items.Gravity) && items.Morph &&
                    (legacyWorld.CanAcquire(items, Agahnim) || items.Hammer && items.CanLiftLight() || items.CanLiftHeavy())
            };
        }

    }

    public record ItemAddress {
        public int Address { get; init; }
        public int Value { get; init; } = 0x01;
        public bool Bitflag { get; init; } = false;
        public bool Additive { get; init; } = false;
    }

    static class ItemTypeExtensions {
        public static ItemAddress[] ItemAddress(this LegacyItemType legacyItemType) {
            return legacyItemType switch {
                Bow => new[] { new ItemAddress { Address = 0x403000 }, new ItemAddress { Address = 0x40304E, Value = 0b1000000, Bitflag = true } },
                SilverArrows => new[] { new ItemAddress { Address = 0x40304E, Value = 0b01000000, Bitflag = true } },
                BlueBoomerang => new[] { new ItemAddress { Address = 0x403001 }, new ItemAddress { Address = 0x40304C, Value = 0b1000000, Bitflag = true } },
                RedBoomerang => new[] { new ItemAddress { Address = 0x403001, Value = 0x02 }, new ItemAddress { Address = 0x40304C, Value = 0b0100000, Bitflag = true } },
                Hookshot => new[] { new ItemAddress { Address = 0x403002 } },
                ThreeBombs => new[] { new ItemAddress { Address = 0x403003, Value = 0x03, Additive = true }, new ItemAddress { Address = 0x40304D, Value = 0b00000010, Bitflag = true } },
                Mushroom => new[] { new ItemAddress { Address = 0x403004, Value = 0x01 }, new ItemAddress { Address = 0x40304C, Value = 0b00101000, Bitflag = true } },
                Powder => new[] { new ItemAddress { Address = 0x403004, Value = 0x02 }, new ItemAddress { Address = 0x40304C, Value = 0b00010000, Bitflag = true } },
                Firerod => new[] { new ItemAddress { Address = 0x403005 } },
                Icerod => new[] { new ItemAddress { Address = 0x403006 } },
                Bombos => new[] { new ItemAddress { Address = 0x403007 } },
                Ether => new[] { new ItemAddress { Address = 0x403008 } },
                Quake => new[] { new ItemAddress { Address = 0x403009 } },
                Lamp => new[] { new ItemAddress { Address = 0x40300A } },
                Hammer => new[] { new ItemAddress { Address = 0x40300B } },
                Shovel => new[] { new ItemAddress { Address = 0x40300C }, new ItemAddress { Address = 0x40304C, Value = 0b00000100, Bitflag = true } },
                Flute => new[] { new ItemAddress { Address = 0x40300C, Value = 0x03 }, new ItemAddress { Address = 0x40304C, Value = 0b00000001, Bitflag = true } },
                Bugnet => new[] { new ItemAddress { Address = 0x40300D } },
                Book => new[] { new ItemAddress { Address = 0x40300E } },
                Somaria => new[] { new ItemAddress { Address = 0x403010 } },
                Byrna => new[] { new ItemAddress { Address = 0x403011 } },
                Cape => new[] { new ItemAddress { Address = 0x403012 } },
                Mirror => new[] { new ItemAddress { Address = 0x403013, Value = 0x02 } },
                ProgressiveGlove => new[] { new ItemAddress { Address = 0x403014, Value = 0x01, Additive = true } },
                Boots => new[] { new ItemAddress { Address = 0x403015 }, new ItemAddress { Address = 0x403039, Value = 0b00000100, Bitflag = true } },
                Flippers => new[] { new ItemAddress { Address = 0x403016 }, new ItemAddress { Address = 0x403039, Value = 0b00000010, Bitflag = true } },
                MoonPearl => new[] { new ItemAddress { Address = 0x403017 } },
                ProgressiveSword => new[] { new ItemAddress { Address = 0x400043, Value = 0x01, Additive = true } },
                ProgressiveShield => new[] { new ItemAddress { Address = 0x40301A, Value = 0x01, Additive = true } },
                ProgressiveTunic => new[] { new ItemAddress { Address = 0x40301B, Value = 0x01, Additive = true } },
                Bottle => new[] { new ItemAddress { Address = 0x40301C, Value = 0x02 }, new ItemAddress { Address = 0x40300F, Value = 0x01 } },
                OneRupee => new[] { new ItemAddress { Address = 0x403020, Value = 0x01, Additive = true } },
                FiveRupees => new[] { new ItemAddress { Address = 0x403020, Value = 0x05, Additive = true } },
                TwentyRupees => new[] { new ItemAddress { Address = 0x403020, Value = 0x14, Additive = true } },
                TwentyRupees2 => new[] { new ItemAddress { Address = 0x403020, Value = 0x14, Additive = true } },
                FiftyRupees => new[] { new ItemAddress { Address = 0x403020, Value = 0x32, Additive = true } },
                OneHundredRupees => new[] { new ItemAddress { Address = 0x403020, Value = 0x64, Additive = true } },
                ThreeHundredRupees => new[] { new ItemAddress { Address = 0x403020, Value = 0x12C, Additive = true } },

                Missile => new[] { new ItemAddress { Address = 0xF26106, Value = 0x05, Additive = true } },
                Super => new[] { new ItemAddress { Address = 0xF26108, Value = 0x05, Additive = true } },
                PowerBomb => new[] { new ItemAddress { Address = 0xF2610A, Value = 0x05, Additive = true } },

                Varia => new[] { new ItemAddress { Address = 0xF26100, Value = 0x1, Bitflag = true } },
                SpringBall => new[] { new ItemAddress { Address = 0xF26100, Value = 0x2, Bitflag = true } },
                Morph => new[] { new ItemAddress { Address = 0xF26100, Value = 0x4, Bitflag = true } },
                ScrewAttack => new[] { new ItemAddress { Address = 0xF26100, Value = 0x8, Bitflag = true } },
                Gravity => new[] { new ItemAddress { Address = 0xF26100, Value = 0x20, Bitflag = true } },
                HiJump => new[] { new ItemAddress { Address = 0xF26100, Value = 0x100, Bitflag = true } },
                SpaceJump => new[] { new ItemAddress { Address = 0xF26100, Value = 0x200, Bitflag = true } },
                Bombs => new[] { new ItemAddress { Address = 0xF26100, Value = 0x1000, Bitflag = true } },
                SpeedBooster => new[] { new ItemAddress { Address = 0xF26100, Value = 0x2000, Bitflag = true } },
                Grapple => new[] { new ItemAddress { Address = 0xF26100, Value = 0x4000, Bitflag = true } },
                XRay => new[] { new ItemAddress { Address = 0xF26100, Value = 0x8000, Bitflag = true } },

                Wave => new[] { new ItemAddress { Address = 0xF26102, Value = 0x1, Bitflag = true } },
                Ice => new[] { new ItemAddress { Address = 0xF26102, Value = 0x2, Bitflag = true } },
                Spazer => new[] { new ItemAddress { Address = 0xF26102, Value = 0x4, Bitflag = true } },
                Plasma => new[] { new ItemAddress { Address = 0xF26102, Value = 0x8, Bitflag = true } },
                Charge => new[] { new ItemAddress { Address = 0xF26102, Value = 0x1000, Bitflag = true } },

                ETank => new[] { new ItemAddress { Address = 0xF26104, Value = 0x64, Additive = true } },
                _ => null
            };
        }
    }

}
