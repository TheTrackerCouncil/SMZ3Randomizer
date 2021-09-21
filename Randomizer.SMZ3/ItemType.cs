using System.ComponentModel;

namespace Randomizer.SMZ3
{
    public enum ItemType : byte
    {
        [Description("Nothing")]
        Nothing,

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

}
