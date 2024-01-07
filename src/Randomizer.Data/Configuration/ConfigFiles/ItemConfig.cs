using System.Collections.Generic;
using System.ComponentModel;
using Randomizer.Shared;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional item information
    /// </summary>
    [Description("Config file for item names and various tracker responses when picking up items")]
    public class ItemConfig : List<ItemData>, IMergeable<ItemData>, IConfigFile<ItemConfig>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ItemConfig() : base()
        {
        }

        /// <summary>
        /// Returns default item information
        /// </summary>
        /// <returns></returns>
        public static ItemConfig Default()
        {
            return new ItemConfig
            {
                new()
                {
                    Item = "Nothing",
                },
                new()
                {
                    Item = "Content",
                    Multiple = true,
                },
                new()
                {
                    Item = "Death",
                    Multiple = true,
                },
                new()
                {
                    Item = "Duck",
                },
                new()
                {
                    Item = "Fire Rod",
                    InternalItemType = ItemType.Firerod,
                },
                new()
                {
                    Item = "Ice Rod",
                    InternalItemType = ItemType.Icerod,
                },
                new()
                {
                    Item = "Hammer",
                    InternalItemType = ItemType.Hammer,
                },
                new()
                {
                    Item = "Hookshot",
                    InternalItemType = ItemType.Hookshot,
                },
                new()
                {
                    Item = "Bow",
                    InternalItemType = ItemType.Bow,
                },
                new()
                {
                    Item = "Blue Boomerang",
                    InternalItemType = ItemType.BlueBoomerang,
                },
                new()
                {
                    Item = "Magic Powder",
                    InternalItemType = ItemType.Powder,
                },
                new()
                {
                    Item = "Bombos",
                    InternalItemType = ItemType.Bombos,
                },
                new()
                {
                    Item = "Ether",
                    InternalItemType = ItemType.Ether,
                },
                new()
                {
                    Item = "Quake",
                    InternalItemType = ItemType.Quake,
                },
                new()
                {
                    Item = "Lamp",
                    InternalItemType = ItemType.Lamp,
                },
                new()
                {
                    Item = "Shovel",
                    InternalItemType = ItemType.Shovel,
                },
                new()
                {
                    Item = "Flute",
                    InternalItemType = ItemType.Flute,
                },
                new()
                {
                    Item = "Cane of Somaria",
                    InternalItemType = ItemType.Somaria,
                },
                new()
                {
                    Item = "Bottle",
                    InternalItemType = ItemType.Bottle,
                    Multiple = true,
                },
                new()
                {
                    Item = "Bee bottle",
                    InternalItemType = ItemType.BottleWithBee,
                    Multiple = true,
                },
                new()
                {
                    Item = "Gold bee bottle",
                    InternalItemType = ItemType.BottleWithGoldBee,
                    Multiple = true,
                },
                new()
                {
                    Item = "Fairy bottle",
                    InternalItemType = ItemType.BottleWithFairy,
                    Multiple = true,
                },
                new()
                {
                    Item = "Blue potion bottle",
                    InternalItemType = ItemType.BottleWithBluePotion,
                    Multiple = true,
                },
                new()
                {
                    Item = "Green potion bottle",
                    InternalItemType = ItemType.BottleWithGreenPotion,
                    Multiple = true,
                },
                new()
                {
                    Item = "Red potion bottle",
                    InternalItemType = ItemType.BottleWithRedPotion,
                    Multiple = true,
                },
                new()
                {
                    Item = "Heart piece",
                    InternalItemType = ItemType.HeartPiece,
                    Multiple = true,
                },
                new()
                {
                    Item = "Cane of Byrna",
                    InternalItemType = ItemType.Byrna,
                },
                new()
                {
                    Item = "Magic Cape",
                    InternalItemType = ItemType.Cape,
                },
                new()
                {
                    Item = "Magic Mirror",
                    InternalItemType = ItemType.Mirror,
                },
                new()
                {
                    Item = "Book of Mudora",
                    InternalItemType = ItemType.Book,
                    Article = "the",
                },
                new()
                {
                    Item = "Zora's Flippers",
                    InternalItemType = ItemType.Flippers,
                },
                new()
                {
                    Item = "Moon Pearl",
                    InternalItemType = ItemType.MoonPearl,
                },
                new()
                {
                    Item = "Bug Catching Net",
                    InternalItemType = ItemType.Bugnet,
                },
                new()
                {
                    Item = "Bombs",
                    InternalItemType = ItemType.ThreeBombs,
                    Multiple = true,
                },
                new()
                {
                    Item = "Mushroom",
                    InternalItemType = ItemType.Mushroom,
                },
                new()
                {
                    Item = "Red Boomerang",
                    InternalItemType = ItemType.RedBoomerang,
                },
                new()
                {
                    Item = "One Rupee",
                    InternalItemType = ItemType.OneRupee,
                    Multiple = true,
                },
                new()
                {
                    Item = "Five Rupees",
                    InternalItemType = ItemType.FiveRupees,
                    Multiple = true,
                },
                new()
                {
                    Item = "Twenty Rupees",
                    InternalItemType = ItemType.TwentyRupees,
                    Multiple = true,
                },
                new()
                {
                    Item = "Heart Container",
                    InternalItemType = ItemType.HeartContainer,
                    Multiple = true,
                },
                new()
                {
                    Item = "Heart Container Refill",
                    InternalItemType = ItemType.HeartContainerRefill,
                },
                new()
                {
                    Item = "One Hundred Rupees",
                    InternalItemType = ItemType.OneHundredRupees,
                    Multiple = true,
                },
                new()
                {
                    Item = "Fifty Rupees",
                    InternalItemType = ItemType.FiftyRupees,
                    Multiple = true,
                },
                new()
                {
                    Item = "Single Arrow",
                    InternalItemType = ItemType.Arrow,
                    Multiple = true,
                },
                new()
                {
                    Item = "Ten Arrows",
                    InternalItemType = ItemType.TenArrows,
                    Multiple = true,
                },
                new()
                {
                    Item = "Three Hundred Rupees",
                    InternalItemType = ItemType.ThreeHundredRupees,
                    Multiple = true,
                },
                new()
                {
                    Item = "Pegasus Boots",
                    InternalItemType = ItemType.Boots,
                },
                new()
                {
                    Item = "Half Magic",
                    InternalItemType = ItemType.HalfMagic,
                    Multiple = true,
                },
                new()
                {
                    Item = "+5 Bomb Capacity",
                    InternalItemType = ItemType.BombUpgrade5,
                    Multiple = true,
                },
                new()
                {
                    Item = "+5 Arrow Capacity",
                    InternalItemType = ItemType.ArrowUpgrade5,
                    Multiple = true,
                },
                new()
                {
                    Item = "Silver Arrows",
                    InternalItemType = ItemType.SilverArrows,
                },
                new()
                {
                    Item = "Sword",
                    InternalItemType = ItemType.ProgressiveSword,
                    Multiple = true,
                },
                new()
                {
                    Item = "Shield",
                    InternalItemType = ItemType.ProgressiveShield,
                    Multiple = true,
                },
                new()
                {
                    Item = "Mail",
                    InternalItemType = ItemType.ProgressiveTunic,
                    Multiple = true,
                },
                new()
                {
                    Item = "Gloves",
                    InternalItemType = ItemType.ProgressiveGlove,
                    Multiple = true,
                },
                new()
                {
                    Item = "Ganons Tower Map",
                    InternalItemType = ItemType.MapGT,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Turtle Rock Map",
                    InternalItemType = ItemType.MapTR,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Thieves Town Map",
                    InternalItemType = ItemType.MapTT,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Tower of Hera Map",
                    InternalItemType = ItemType.MapTH,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Ice Palace Map",
                    InternalItemType = ItemType.MapIP,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Skull Woods Map",
                    InternalItemType = ItemType.MapSW,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Misery Mire Map",
                    InternalItemType = ItemType.MapMM,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Palace of Darkness Map",
                    InternalItemType = ItemType.MapPD,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Swamp Palace Map",
                    InternalItemType = ItemType.MapSP,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Desert Palace Map",
                    InternalItemType = ItemType.MapDP,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Eastern Palace Map",
                    InternalItemType = ItemType.MapEP,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Hyrule Castle Map",
                    InternalItemType = ItemType.MapHC,
                    Image = "map.png",
                },
                new()
                {
                    Item = "Ganons Tower Compass",
                    InternalItemType = ItemType.CompassGT,
                },
                new()
                {
                    Item = "Turtle Rock Compass",
                    InternalItemType = ItemType.CompassTR,
                },
                new()
                {
                    Item = "Thieves Town Compass",
                    InternalItemType = ItemType.CompassTT,
                },
                new()
                {
                    Item = "Tower of Hera Compass",
                    InternalItemType = ItemType.CompassTH,
                },
                new()
                {
                    Item = "Ice Palace Compass",
                    InternalItemType = ItemType.CompassIP,
                },
                new()
                {
                    Item = "Skull Woods Compass",
                    InternalItemType = ItemType.CompassSW,
                },
                new()
                {
                    Item = "Misery Mire Compass",
                    InternalItemType = ItemType.CompassMM,
                },
                new()
                {
                    Item = "Palace of Darkness Compass",
                    InternalItemType = ItemType.CompassPD,
                },
                new()
                {
                    Item = "Swamp Palace Compass",
                    InternalItemType = ItemType.CompassSP,
                },
                new()
                {
                    Item = "Desert Palace Compass",
                    InternalItemType = ItemType.CompassDP,
                },
                new()
                {
                    Item = "Eastern Palace Compass",
                    InternalItemType = ItemType.CompassEP,
                },
                new()
                {
                    Item = "Ganons Tower Big Key",
                    InternalItemType = ItemType.BigKeyGT,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Turtle Rock Big Key",
                    InternalItemType = ItemType.BigKeyTR,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Thieves Town Big Key",
                    InternalItemType = ItemType.BigKeyTT,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Tower of Hera Big Key",
                    InternalItemType = ItemType.BigKeyTH,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Ice Palace Big Key",
                    InternalItemType = ItemType.BigKeyIP,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Skull Woods Big Key",
                    InternalItemType = ItemType.BigKeySW,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Misery Mire Big Key",
                    InternalItemType = ItemType.BigKeyMM,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Palace of Darkness Big Key",
                    InternalItemType = ItemType.BigKeyPD,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Swamp Palace Big Key",
                    InternalItemType = ItemType.BigKeySP,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Desert Palace Big Key",
                    InternalItemType = ItemType.BigKeyDP,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Eastern Palace Big Key",
                    InternalItemType = ItemType.BigKeyEP,
                    Image = "bigkey.png",
                },
                new()
                {
                    Item = "Sewer Key",
                    InternalItemType = ItemType.KeyHC,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Desert Palace Key",
                    InternalItemType = ItemType.KeyDP,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Castle Tower Key",
                    InternalItemType = ItemType.KeyCT,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Swamp Palace Key",
                    InternalItemType = ItemType.KeySP,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Palace of Darkness Key",
                    InternalItemType = ItemType.KeyPD,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Misery Mire Key",
                    InternalItemType = ItemType.KeyMM,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Skull Woods Key",
                    InternalItemType = ItemType.KeySW,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Ice Palace Key",
                    InternalItemType = ItemType.KeyIP,
                    Article = "an",
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Tower of Hera Key",
                    InternalItemType = ItemType.KeyTH,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Thieves Town Key",
                    InternalItemType = ItemType.KeyTT,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Turtle Rock Key",
                    InternalItemType = ItemType.KeyTR,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Ganons Tower Key",
                    InternalItemType = ItemType.KeyGT,
                    Multiple = true,
                    Image = "smallkey.png",
                },
                new()
                {
                    Item = "Small Key",
                    InternalItemType = ItemType.Key,
                    Multiple = false,
                    Image = "key.png",
                },
                new()
                {
                    Item = "Big Key",
                    InternalItemType = ItemType.BigKey,
                    Multiple = false,
                    Image = "lttp_big_key.png",
                },
                new()
                {
                    Item = "Map",
                    InternalItemType = ItemType.Map,
                    Multiple = false,
                    Image = "map_full.png",
                },
                new()
                {
                    Item = "Compass",
                    InternalItemType = ItemType.Compass,
                    Multiple = false,
                    Image = "compass.png",
                },
                new()
                {
                    Item = "Grappling Beam",
                    InternalItemType = ItemType.Grapple,
                },
                new()
                {
                    Item = "X-Ray Scope",
                    InternalItemType = ItemType.XRay,
                },
                new()
                {
                    Item = "Varia Suit",
                    InternalItemType = ItemType.Varia,
                },
                new()
                {
                    Item = "Spring Ball",
                    InternalItemType = ItemType.SpringBall,
                },
                new()
                {
                    Item = "Morphing Ball",
                    InternalItemType = ItemType.Morph,
                },
                new()
                {
                    Item = "Screw Attack",
                    InternalItemType = ItemType.ScrewAttack,
                },
                new()
                {
                    Item = "Gravity Suit",
                    InternalItemType = ItemType.Gravity,
                },
                new()
                {
                    Item = "Hi-Jump Boots",
                    InternalItemType = ItemType.HiJump,
                },
                new()
                {
                    Item = "Space Jump",
                    InternalItemType = ItemType.SpaceJump,
                },
                new()
                {
                    Item = "Morph Bombs",
                    InternalItemType = ItemType.Bombs,
                },
                new()
                {
                    Item = "Speed Booster",
                    InternalItemType = ItemType.SpeedBooster,
                },
                new()
                {
                    Item = "Charge Beam",
                    InternalItemType = ItemType.Charge,
                },
                new()
                {
                    Item = "Ice Beam",
                    InternalItemType = ItemType.Ice,
                },
                new()
                {
                    Item = "Wave Beam",
                    InternalItemType = ItemType.Wave,
                },
                new()
                {
                    Item = "Spazer",
                    InternalItemType = ItemType.Spazer,
                },
                new()
                {
                    Item = "Plasma Beam",
                    InternalItemType = ItemType.Plasma,
                },
                new()
                {
                    Item = "Energy Tank",
                    InternalItemType = ItemType.ETank,
                    Article = "an",
                    Multiple = true,
                },
                new()
                {
                    Item = "Reserve Tank",
                    InternalItemType = ItemType.ReserveTank,
                    Multiple = true,
                },
                new()
                {
                    Item = "Missile",
                    InternalItemType = ItemType.Missile,
                    Multiple = true,
                    CounterMultiplier = 5,
                },
                new()
                {
                    Item = "Super Missile",
                    InternalItemType = ItemType.Super,
                    Multiple = true,
                    CounterMultiplier = 5,
                },
                new()
                {
                    Item = "Power Bomb",
                    InternalItemType = ItemType.PowerBomb,
                    Multiple = true,
                    CounterMultiplier = 5,
                },
                new()
                {
                    Item = "Crateria Level 1 Keycard",
                    InternalItemType = ItemType.CardCrateriaL1,
                    Image = "smkey1.png",
                },
                new()
                {
                    Item = "Crateria Level 2 Keycard",
                    InternalItemType = ItemType.CardCrateriaL2,
                    Image = "smkey2.png",
                },
                new()
                {
                    Item = "Crateria Boss Keycard",
                    InternalItemType = ItemType.CardCrateriaBoss,
                    Image = "smkeyboss.png",
                },
                new()
                {
                    Item = "Brinstar Level 1 Keycard",
                    InternalItemType = ItemType.CardBrinstarL1,
                    Image = "smkey1.png",
                },
                new()
                {
                    Item = "Brinstar Level 2 Keycard",
                    InternalItemType = ItemType.CardBrinstarL2,
                    Image = "smkey2.png",
                },
                new()
                {
                    Item = "Brinstar Boss Keycard",
                    InternalItemType = ItemType.CardBrinstarBoss,
                    Image = "smkeyboss.png",
                },
                new()
                {
                    Item = "Norfair Level 1 Keycard",
                    InternalItemType = ItemType.CardNorfairL1,
                    Image = "smkey1.png",
                },
                new()
                {
                    Item = "Norfair Level 2 Keycard",
                    InternalItemType = ItemType.CardNorfairL2,
                    Image = "smkey2.png",
                },
                new()
                {
                    Item = "Norfair Boss Keycard",
                    InternalItemType = ItemType.CardNorfairBoss,
                    Image = "smkeyboss.png",
                },
                new()
                {
                    Item = "Maridia Level 1 Keycard",
                    InternalItemType = ItemType.CardMaridiaL1,
                    Image = "smkey1.png",
                },
                new()
                {
                    Item = "Maridia Level 2 Keycard",
                    InternalItemType = ItemType.CardMaridiaL2,
                    Image = "smkey2.png",
                },
                new()
                {
                    Item = "Maridia Boss Keycard",
                    InternalItemType = ItemType.CardMaridiaBoss,
                    Image = "smkeyboss.png",
                },
                new()
                {
                    Item = "Wrecked Ship Level 1 Keycard",
                    InternalItemType = ItemType.CardWreckedShipL1,
                    Image = "smkey1.png",
                },
                new()
                {
                    Item = "Wrecked Ship Boss Keycard",
                    InternalItemType = ItemType.CardWreckedShipBoss,
                    Image = "smkeyboss.png",
                },
                new()
                {
                    Item = "Lower Norfair Level 1 Keycard",
                    InternalItemType = ItemType.CardLowerNorfairL1,
                    Image = "smkey1.png",
                },
                new()
                {
                    Item = "Lower Norfair Boss Keycard",
                    InternalItemType = ItemType.CardLowerNorfairBoss,
                    Image = "smkeyboss.png",
                },
            };
        }

        public static object Example()
        {
            return new ItemConfig()
            {
                new()
                {
                    Item = "Half Magic",
                    Name = new("Half Magic", new SchrodingersString.Possibility("50% off magic coupon", 0.1)),
                    ArticledName = new ("the Half Magic", new SchrodingersString.Possibility("the 50% off magic coupon")),
                    Hints = new("It's green", "It's an upgrade"),
                    PedestalHints = new("The ability to do more magical things"),
                    Stages = new Dictionary<int, SchrodingersString>()
                    {
                        {1, new SchrodingersString("Half Magic")},
                        {2, new SchrodingersString("Quarter Magic")},
                    },
                    WhenTracked = new Dictionary<int, SchrodingersString?>()
                    {
                        {1, new SchrodingersString("Custom message when tracking half magic")},
                        {2, new SchrodingersString("Custom message when tracking quarter magic")},
                    },
                },
                new()
                {
                    Item = "Custom Item",
                    Name = new("Custom Item", new SchrodingersString.Possibility("Another custom item", 0.1)),
                    ArticledName = new ("a custom item"),
                    Multiple = true,
                    WhenTracked = new Dictionary<int, SchrodingersString?>()
                    {
                        {1, new SchrodingersString("Message when tracking it the first 3 times")},
                        {4, new SchrodingersString("Message when tracking it 4-7 times, 8-11 will fall back to default messages")},
                        {8, null},
                        {12, new SchrodingersString("Message when tracking it 12+ times")},
                    },
                },
            };
        }
    }
}
