using System.ComponentModel;

namespace TrackerCouncil.Smz3.Shared.Enums;

public enum ItemType : byte
{
    [Description("Nothing")]
    Nothing,

    [Description("Hyrule Castle Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapHC = 0x7F,

    [Description("Eastern Palace Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapEP = 0x7D,

    [Description("Desert Palace Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapDP = 0x7C,

    [Description("Tower of Hera Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapTH = 0x75,

    [Description("Palace of Darkness Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapPD = 0x79,

    [Description("Swamp Palace Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapSP = 0x7A,

    [Description("Skull Woods Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapSW = 0x77,

    [Description("Thieves Town Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapTT = 0x74,

    [Description("Ice Palace Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapIP = 0x76,

    [Description("Misery Mire Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapMM = 0x78,

    [Description("Turtle Rock Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapTR = 0x73,

    [Description("Ganons Tower Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NeverProgression)]
    MapGT = 0x72,

    [Description("Eastern Palace Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassEP = 0x8D,

    [Description("Desert Palace Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassDP = 0x8C,

    [Description("Tower of Hera Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassTH = 0x85,

    [Description("Palace of Darkness Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassPD = 0x89,

    [Description("Swamp Palace Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassSP = 0x8A,

    [Description("Skull Woods Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassSW = 0x87,

    [Description("Thieves Town Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassTT = 0x84,

    [Description("Ice Palace Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassIP = 0x86,

    [Description("Misery Mire Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassMM = 0x88,

    [Description("Turtle Rock Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassTR = 0x83,

    [Description("Ganons Tower Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NeverProgression)]
    CompassGT = 0x82,

    [Description("Eastern Palace Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeyEP = 0x9D,

    [Description("Desert Palace Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeyDP = 0x9C,

    [Description("Tower of Hera Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeyTH = 0x95,

    [Description("Palace of Darkness Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeyPD = 0x99,

    [Description("Swamp Palace Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeySP = 0x9A,

    [Description("Skull Woods Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeySW = 0x97,

    [Description("Thieves Town Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeyTT = 0x94,

    [Description("Ice Palace Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeyIP = 0x96,

    [Description("Misery Mire Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeyMM = 0x98,

    [Description("Turtle Rock Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeyTR = 0x93,

    [Description("Ganons Tower Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey)]
    BigKeyGT = 0x92,

    [Description("Sewer Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyHC = 0xA0,

    [Description("Castle Tower Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyCT = 0xA4,

    [Description("Desert Palace Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyDP = 0xA3,

    [Description("Tower of Hera Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyTH = 0xAA,

    [Description("Palace of Darkness Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyPD = 0xA6,

    [Description("Swamp Palace Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeySP = 0xA5,

    [Description("Skull Woods Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeySW = 0xA8,

    [Description("Thieves Town Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyTT = 0xAB,

    [Description("Ice Palace Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyIP = 0xA9,

    [Description("Misery Mire Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyMM = 0xA7,

    [Description("Turtle Rock Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyTR = 0xAC,

    [Description("Ganons Tower Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey)]
    KeyGT = 0xAD,

    [Description("Small Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.SmallKey, ItemCategory.NonRandomized)]
    Key = 0x24,

    [Description("Compass")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Compass, ItemCategory.NonRandomized)]
    Compass = 0x25,

    [Description("Big Key")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.BigKey, ItemCategory.NonRandomized)]
    BigKey = 0x32,

    [Description("Map")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Map, ItemCategory.NonRandomized)]
    Map = 0x33,

    [Description("Progressive Mail")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Nice, ItemCategory.NeverProgression)]
    ProgressiveTunic = 0x60,

    [Description("Progressive Shield")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.PossibleProgression)]
    ProgressiveShield = 0x5F,

    [Description("Progressive Sword")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    ProgressiveSword = 0x5E,

    [Description("Bow")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Bow = 0x0B,

    [Description("Silver Arrows")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Nice, ItemCategory.PossibleProgression)]
    SilverArrows = 0x58,

    [Description("Blue Boomerang")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.NeverProgression)]
    BlueBoomerang = 0x0C,

    [Description("Red Boomerang")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.NeverProgression)]
    RedBoomerang = 0x2A,

    [Description("Hookshot")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Hookshot = 0x0A,

    [Description("Mushroom")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.PossibleProgression)]
    Mushroom = 0x29,

    [Description("Magic Powder")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.PossibleProgression)]
    Powder = 0x0D,

    [Description("Fire Rod")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Firerod = 0x07,

    [Description("Ice Rod")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Icerod = 0x08,

    [Description("Bombos")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Medallion, ItemCategory.PossibleProgression)]
    Bombos = 0x0f,

    [Description("Ether")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Medallion, ItemCategory.PossibleProgression)]
    Ether = 0x10,

    [Description("Quake")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Medallion, ItemCategory.PossibleProgression)]
    Quake = 0x11,

    [Description("Lamp")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Lamp = 0x12,

    [Description("Hammer")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Hammer = 0x09,

    [Description("Shovel")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Shovel = 0x13,

    [Description("Flute")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Flute = 0x14,

    [Description("Bug Catching Net")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.NeverProgression)]
    Bugnet = 0x21,

    [Description("Book of Mudora")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Book = 0x1D,

    [Description("Bottle")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Bottle, ItemCategory.PossibleProgression)]
    Bottle = 0x16,

    [Description("Cane of Somaria")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Somaria = 0x15,

    [Description("Cane of Byrna")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Byrna = 0x18,

    [Description("Magic Cape")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Cape = 0x19,

    [Description("Magic Mirror")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Mirror = 0x1A,

    [Description("Pegasus Boots")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Boots = 0x4B,

    [Description("Progressive Glove")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    ProgressiveGlove = 0x61,

    [Description("Zora's Flippers")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    Flippers = 0x1E,

    [Description("Moon Pearl")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    MoonPearl = 0x1F,

    [Description("Half Magic")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.PossibleProgression)]
    HalfMagic = 0x4E,

    [Description("Piece of Heart")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.Plentiful)]
    HeartPiece = 0x17,

    [Description("Heart Container")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.ProgressionOnLimitedAmount)]
    HeartContainer = 0x3E,

    [Description("Sanctuary Heart Container")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.NeverProgression)]
    HeartContainerRefill = 0x3F,

    [Description("Three Bombs")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.Plentiful, ItemCategory.IgnoreOnMultiplayerCompletion, ItemCategory.NeverProgression)]
    ThreeBombs = 0x28,

    [Description("Single Arrow")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.IgnoreOnMultiplayerCompletion, ItemCategory.NeverProgression)]
    Arrow = 0x43,

    [Description("Ten Arrows")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.IgnoreOnMultiplayerCompletion, ItemCategory.NeverProgression)]
    TenArrows = 0x44,

    [Description("One Rupee")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.IgnoreOnMultiplayerCompletion, ItemCategory.NeverProgression)]
    OneRupee = 0x34,

    [Description("Five Rupees")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.IgnoreOnMultiplayerCompletion, ItemCategory.NeverProgression)]
    FiveRupees = 0x35,

    [Description("Twenty Rupees")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.Plentiful, ItemCategory.IgnoreOnMultiplayerCompletion, ItemCategory.NeverProgression)]
    TwentyRupees = 0x36,

    [Description("Twenty Rupees")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.IgnoreOnMultiplayerCompletion, ItemCategory.NeverProgression)]
    TwentyRupees2 = 0x47,

    [Description("Fifty Rupees")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.IgnoreOnMultiplayerCompletion, ItemCategory.NeverProgression)]
    FiftyRupees = 0x41,

    [Description("One Hundred Rupees")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.NeverProgression)]
    OneHundredRupees = 0x40,

    [Description("Three Hundred Rupees")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.PossibleProgression)]
    ThreeHundredRupees = 0x46,

    [Description("+5 Bomb Capacity")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.NeverProgression)]
    BombUpgrade5 = 0x51,

    [Description("+10 Bomb Capacity")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.NeverProgression)]
    BombUpgrade10 = 0x52,

    [Description("+5 Arrow Capacity")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.NeverProgression)]
    ArrowUpgrade5 = 0x53,

    [Description("+10 Arrow Capacity")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.NeverProgression)]
    ArrowUpgrade10 = 0x54,

    [Description("Crateria Level 1 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL1)]
    CardCrateriaL1 = 0xD0,

    [Description("Crateria Level 2 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL2)]
    CardCrateriaL2 = 0xD1,

    [Description("Crateria Boss Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardBoss)]
    CardCrateriaBoss = 0xD2,

    [Description("Brinstar Level 1 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL1)]
    CardBrinstarL1 = 0xD3,

    [Description("Brinstar Level 2 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL2)]
    CardBrinstarL2 = 0xD4,

    [Description("Brinstar Boss Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardBoss)]
    CardBrinstarBoss = 0xD5,

    [Description("Norfair Level 1 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL1)]
    CardNorfairL1 = 0xD6,

    [Description("Norfair Level 2 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL2)]
    CardNorfairL2 = 0xD7,

    [Description("Norfair Boss Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardBoss)]
    CardNorfairBoss = 0xD8,

    [Description("Maridia Level 1 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL1)]
    CardMaridiaL1 = 0xD9,

    [Description("Maridia Level 2 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL2)]
    CardMaridiaL2 = 0xDA,

    [Description("Maridia Boss Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardBoss)]
    CardMaridiaBoss = 0xDB,

    [Description("Wrecked Ship Level 1 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL1)]
    CardWreckedShipL1 = 0xDC,

    [Description("Wrecked Ship Boss Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardBoss)]
    CardWreckedShipBoss = 0xDD,

    [Description("Lower Norfair Level 1 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardL1)]
    CardLowerNorfairL1 = 0xDE,

    [Description("Lower Norfair Boss Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Keycard, ItemCategory.KeycardBoss)]
    CardLowerNorfairBoss = 0xDF,

    [Description("Missile")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.Plentiful, ItemCategory.ProgressionOnLimitedAmount)]
    Missile = 0xC2,

    [Description("Super Missile")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.Plentiful, ItemCategory.ProgressionOnLimitedAmount)]
    Super = 0xC3,

    [Description("Power Bomb")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.ProgressionOnLimitedAmount)]
    PowerBomb = 0xC4,

    [Description("Grappling Beam")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    Grapple = 0xB0,

    [Description("X-Ray Scope")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Scam, ItemCategory.NeverProgression)]
    XRay = 0xB1,

    [Description("Energy Tank")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.ProgressionOnLimitedAmount)]
    ETank = 0xC0,

    [Description("Reserve Tank")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Scam, ItemCategory.Junk, ItemCategory.ProgressionOnLimitedAmount)]
    ReserveTank = 0xC1,

    [Description("Charge Beam")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    Charge = 0xBB,

    [Description("Ice Beam")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    Ice = 0xBC,

    [Description("Wave Beam")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    Wave = 0xBD,

    [Description("Spazer")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.Nice, ItemCategory.NeverProgression)]
    Spazer = 0xBE,

    [Description("Plasma Beam")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    Plasma = 0xBF,

    [Description("Varia Suit")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    Varia = 0xB2,

    [Description("Gravity Suit")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    Gravity = 0xB6,

    [Description("Morphing Ball")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    Morph = 0xB4,

    [Description("Morph Bombs")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    Bombs = 0xB9,

    [Description("Spring Ball")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    SpringBall = 0xB3,

    [Description("Screw Attack")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    ScrewAttack = 0xB5,

    [Description("Hi-Jump Boots")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    HiJump = 0xB7,

    [Description("Space Jump")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    SpaceJump = 0xB8,

    [Description("Speed Booster")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.PossibleProgression)]
    SpeedBooster = 0xBA,

    [Description("Bottle with Red Potion")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Bottle, ItemCategory.PossibleProgression)]
    BottleWithRedPotion = 0x2B,

    [Description("Bottle with Green Potion")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Bottle, ItemCategory.PossibleProgression)]
    BottleWithGreenPotion = 0x2C,

    [Description("Bottle with Blue Potion")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Bottle, ItemCategory.PossibleProgression)]
    BottleWithBluePotion = 0x2D,

    [Description("Bottle with Fairy")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Bottle, ItemCategory.PossibleProgression)]
    BottleWithFairy = 0x3D,

    [Description("Bottle with Bee")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Bottle, ItemCategory.PossibleProgression)]
    BottleWithBee = 0x3C,

    [Description("Bottle with Gold Bee")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.Scam, ItemCategory.Bottle, ItemCategory.PossibleProgression)]
    BottleWithGoldBee = 0x48,

    [Description("Red Potion Refill")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.NonRandomized)]
    RedContent = 0x2E,

    [Description("Green Potion Refill")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.NonRandomized)]
    GreenContent = 0x2F,

    [Description("Blue Potion Refill")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.NonRandomized)]
    BlueContent = 0x30,

    [Description("Bee Refill")]
    [ItemCategory(ItemCategory.Zelda, ItemCategory.NonRandomized)]
    BeeContent = 0x0E,

    [Description("Level 1 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.NonRandomized, ItemCategory.Keycard, ItemCategory.KeycardL1)]
    CardL1 = 0xF0,

    [Description("Level 2 Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.NonRandomized, ItemCategory.Keycard, ItemCategory.KeycardL2)]
    CardL2 = 0xF1,

    [Description("Boss Keycard")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.NonRandomized, ItemCategory.Keycard, ItemCategory.KeycardBoss)]
    CardBoss = 0xF2,

    [Description("Brinstar Map")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.MetroidMap, ItemCategory.NeverProgression)]
    SmMapBrinstar = 0xCA,

    [Description("Wrecked Ship Map")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.MetroidMap, ItemCategory.NeverProgression)]
    SmMapWreckedShip = 0xCB,

    [Description("Maridia Map")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.MetroidMap, ItemCategory.NeverProgression)]
    SmMapMaridia = 0xCC,

    [Description("Lower Norfair Map")]
    [ItemCategory(ItemCategory.Metroid, ItemCategory.MetroidMap, ItemCategory.NeverProgression)]
    SmMapLowerNorfair = 0xCD,

    [Description("Other Game Item")]
    [ItemCategory(ItemCategory.NonRandomized, ItemCategory.NeverProgression)]
    OtherGameItem = 0xFE,

    [Description("Important Other Game Item")]
    [ItemCategory(ItemCategory.NonRandomized, ItemCategory.PossibleProgression)]
    OtherGameProgressionItem = 0xFF
}
