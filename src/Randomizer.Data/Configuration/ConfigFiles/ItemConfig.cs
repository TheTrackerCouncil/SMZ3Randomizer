using System.Collections.Generic;
using Randomizer.Shared;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional item information
    /// </summary>
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
                new ItemData()
                {
                    Item = "Nothing",
                    Name = new("Nothing"),
                },
                new ItemData()
                {
                    Item = "Content",
                    Name = new(new("Content", 0), "Con-tent"),
                    Plural = new(new("Content", 0), "Con-tent"),
                    Multiple = true,
                },
                new ItemData()
                {
                    Item = "Death",
                    Name = new("Death", "Game Over", "Tactical reset"),
                    Plural = new("Deaths", "Game Overs", "Tactical resets"),
                    Multiple = true,
                },
                new ItemData()
                {
                    Item = "Duck",
                    Name = new("Duck", "Bird"),
                },
                new ItemData()
                {
                    Item = "Fire Rod",
                    Name = new("Fire Rod"),
                    InternalItemType = ItemType.Firerod,
                    Hints = new("You could use it to set things on fire.", "You can hurt people with it.", "It's something magical.", "You could use it to see things you usually can't.", "It's red."),
                    PedestalHints = new("A magical way to shoot flames far away")
                },
                new ItemData()
                {
                    Item = "Ice Rod",
                    Name = new("Ice Rod"),
                    InternalItemType = ItemType.Icerod,
                    Hints = new("You can hurt people with it.", "It's something magical.", "It's cold.", "It's blue."),
                    PedestalHints = new("A magical way to shoot ice far away"),
                },
                new ItemData()
                {
                    Item = "Hammer",
                    Name = new("Hammer"),
                    InternalItemType = ItemType.Hammer,
                    Hints = new("You can hurt people with it.", "It's one of the items required for Peg World.", "You could find it in real life."),
                    PedestalHints = new("Some tool to hit pegs and nails with"),
                },
                new ItemData()
                {
                    Item = "Hookshot",
                    Name = new("Hookshot"),
                    InternalItemType = ItemType.Hookshot,
                    Hints = new("You could use it to get around."),
                    PedestalHints = new("A tool that can pull you over pits"),
                },
                new ItemData()
                {
                    Item = "Bow",
                    Name = new(new("Bow", 0), "Boh", "Boh and Arrow"),
                    InternalItemType = ItemType.Bow,
                    Hints = new("You can hurt people with it."),
                    PedestalHints = new("A stick shooting device"),
                },
                new ItemData()
                {
                    Item = "Blue Boomerang",
                    Name = new("Blue Boomerang"),
                    InternalItemType = ItemType.BlueBoomerang,
                    Hints = new("You can hurt people with it.", "It's blue."),
                    PedestalHints = new("Something blue to stun enemies with"),
                },
                new ItemData()
                {
                    Item = "Magic Powder",
                    Name = new("Magic Powder"),
                    InternalItemType = ItemType.Powder,
                    Hints = new("It's something magical."),
                    PedestalHints = new("Something to sprinkle on anti-faeries"),
                },
                new ItemData()
                {
                    Item = "Bombos",
                    Name = new("Bombos"),
                    InternalItemType = ItemType.Bombos,
                    Hints = new("It's something magical.", "It's one of the medallions."),
                    PedestalHints = new("Some magic coin to burn everything"),
                },
                new ItemData()
                {
                    Item = "Ether",
                    Name = new("Ether"),
                    InternalItemType = ItemType.Ether,
                    Hints = new("It's something magical.", "It's one of the medallions."),
                    PedestalHints = new("Some magic coin to freeze everything"),
                },
                new ItemData()
                {
                    Item = "Quake",
                    Name = new("Quake"),
                    InternalItemType = ItemType.Quake,
                    Hints = new("It's something magical.", "It's one of the medallions."),
                    PedestalHints = new("Some magic coin to shake the ground"),
                },
                new ItemData()
                {
                    Item = "Lamp",
                    Name = new("Lamp", "Lantern"),
                    InternalItemType = ItemType.Lamp,
                    Hints = new("You could use it to set things on fire.", "You could use it to see things you usually can't.", "It's red."),
                    PedestalHints = new("A basic tool to help light the way"),
                },
                new ItemData()
                {
                    Item = "Shovel",
                    Name = new("Shovel"),
                    InternalItemType = ItemType.Shovel,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("A digging tool that isn't Shaktool"),
                },
                new ItemData()
                {
                    Item = "Flute",
                    Name = new("Flute", "Ocarina"),
                    InternalItemType = ItemType.Flute,
                    Hints = new("You could use it to get around.", "You could find it in real life.", "It's blue."),
                    PedestalHints = new("A musical instrument to call a bird"),
                },
                new ItemData()
                {
                    Item = "Cane of Somaria",
                    Name = new("Cane of Somaria", new("Red Cane", 0)),
                    InternalItemType = ItemType.Somaria,
                    Hints = new("It's something magical.", "It's red."),
                    PedestalHints = new("A magical device for making blocks"),
                },
                new ItemData()
                {
                    Item = "Bottle",
                    Name = new("Bottle"),
                    InternalItemType = ItemType.Bottle,
                    Plural = new("Bottles"),
                    Multiple = true,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("Something to hold various liquids and things with"),
                },
                new ItemData()
                {
                    Item = "Bee bottle",
                    Name = new("Bee bottle", "Bottle with a bee"),
                    InternalItemType = ItemType.BottleWithBee,
                    Plural = new("Bee bottles", "Bottles with a bee"),
                    Multiple = true,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("Something to hold various liquids and things with"),
                },
                new ItemData()
                {
                    Item = "Gold bee bottle",
                    Name = new("Gold bee bottle", "Good bee bottle", "Bottle with a gold bee"),
                    InternalItemType = ItemType.BottleWithGoldBee,
                    Plural = new("Gold bee bottles", "Good bee bottles", "Bottles with a gold bee"),
                    Multiple = true,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("Something to hold various liquids and things with"),
                },
                new ItemData()
                {
                    Item = "Fairy bottle",
                    Name = new("Fairy bottle", "Bottle with a fairy"),
                    InternalItemType = ItemType.BottleWithFairy,
                    Plural = new("Fairy bottles", "Bottles with a fairy"),
                    Multiple = true,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("Something to hold various liquids and things with"),
                },
                new ItemData()
                {
                    Item = "Blue potion bottle",
                    Name = new("Blue potion bottle", "Bottle with a blue potion"),
                    InternalItemType = ItemType.BottleWithBluePotion,
                    Plural = new("Blue potion bottles", "Bottles with a blue potion"),
                    Multiple = true,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("Something to hold various liquids and things with"),
                },
                new ItemData()
                {
                    Item = "Green potion bottle",
                    Name = new("Green potion bottle", "Bottle with a green potion"),
                    InternalItemType = ItemType.BottleWithGreenPotion,
                    Plural = new("Green potion bottles", "Bottles with a green potion"),
                    Multiple = true,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("Something to hold various liquids and things with"),
                },
                new ItemData()
                {
                    Item = "Red potion bottle",
                    Name = new("Red potion bottle", "Bottle with a red potion"),
                    InternalItemType = ItemType.BottleWithRedPotion,
                    Plural = new("Red potion bottles", "Bottles with a red potion"),
                    Multiple = true,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("Something to hold various liquids and things with"),
                },
                new ItemData()
                {
                    Item = "Heart piece",
                    Name = new("Heart piece", "Piece of Heart"),
                    InternalItemType = ItemType.HeartPiece,
                    Plural = new("Heart pieces", "Pieces of Heart"),
                    Multiple = true,
                    Hints = new("You probably already have a lot of it.", "It improves your survivability.", "It's red."),
                    PedestalHints = new("A small piece of love"),
                },
                new ItemData()
                {
                    Item = "Cane of Byrna",
                    Name = new("Cane of Byrna", new("Blue Cane", 0)),
                    InternalItemType = ItemType.Byrna,
                    Hints = new("It's something magical.", "It's blue."),
                    PedestalHints = new("A magical device for becoming invincible"),
                },
                new ItemData()
                {
                    Item = "Magic Cape",
                    Name = new("Magic Cape", "Cape"),
                    InternalItemType = ItemType.Cape,
                    Hints = new("It's something magical.", "It's something you could wear.", "It's red."),
                    PedestalHints = new("Something to wear to become invincible and invisible"),
                },
                new ItemData()
                {
                    Item = "Magic Mirror",
                    Name = new("Magic Mirror", "Mirror"),
                    InternalItemType = ItemType.Mirror,
                    Hints = new("It's something magical."),
                    PedestalHints = new("A magical object that lets you change worlds"),
                },
                new ItemData()
                {
                    Item = "Book of Mudora",
                    Name = new("Book of Mudora"),
                    InternalItemType = ItemType.Book,
                    Article = "the",
                    Hints = new("It's something magical."),
                    PedestalHints = new("A dictionary from the library"),
                },
                new ItemData()
                {
                    Item = "Zora's Flippers",
                    Name = new(new("Zora's Flippers", 0), "Flippers"),
                    InternalItemType = ItemType.Flippers,
                    Hints = new("You could use it to get around.", "It's blue."),
                    PedestalHints = new("Something to put on your feet to swim"),
                },
                new ItemData()
                {
                    Item = "Moon Pearl",
                    Name = new("Moon Pearl"),
                    InternalItemType = ItemType.MoonPearl,
                    Hints = new("It's round.", "It's red."),
                    PedestalHints = new("An orb to let you traverse the dark world"),
                },
                new ItemData()
                {
                    Item = "Bug Catching Net",
                    Name = new("Bug Catching Net", "Bug net"),
                    InternalItemType = ItemType.Bugnet,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("A tool to capture bees and faeries with"),
                },
                new ItemData()
                {
                    Item = "Bombs",
                    Name = new("Bombs", "Zelda Bombs"),
                    InternalItemType = ItemType.ThreeBombs,
                    Plural = new("Bombs", "Zelda Bombs"),
                    Multiple = true,
                    Hints = new("You probably already have a lot of it.", "It's round."),
                    PedestalHints = new("A small collection of primitive explosives"),
                },
                new ItemData()
                {
                    Item = "Mushroom",
                    Name = new("Mushroom"),
                    InternalItemType = ItemType.Mushroom,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("Some sort of fungus"),
                },
                new ItemData()
                {
                    Item = "Red Boomerang",
                    Name = new("Red Boomerang"),
                    InternalItemType = ItemType.RedBoomerang,
                    Hints = new("You can hurt people with it.", "It's red."),
                    PedestalHints = new("Something red to stun enemies with"),
                },
                new ItemData()
                {
                    Item = "One Rupee",
                    Name = new("One Rupee", "Green Rupee"),
                    InternalItemType = ItemType.OneRupee,
                    Multiple = true,
                    Hints = new("It's just money.", "You probably already have a lot of it.", "It's green."),
                    PedestalHints = new("A small amount of pocket change"),
                },
                new ItemData()
                {
                    Item = "Five Rupees",
                    Name = new("Five Rupees", "Blue Rupee"),
                    InternalItemType = ItemType.FiveRupees,
                    Multiple = true,
                    Hints = new("It's just money.", "You probably already have a lot of it.", "It's blue."),
                    PedestalHints = new("A small amount of pocket change"),
                },
                new ItemData()
                {
                    Item = "Twenty Rupees",
                    Name = new("Twenty Rupees", "Red Rupee"),
                    InternalItemType = ItemType.TwentyRupees,
                    Multiple = true,
                    Hints = new("It's just money.", "You probably already have a lot of it.", "It's red."),
                    PedestalHints = new("A small amount of pocket change"),
                },
                new ItemData()
                {
                    Item = "Heart Container",
                    Name = new("Heart Container"),
                    InternalItemType = ItemType.HeartContainer,
                    Plural = new("Heart Containers"),
                    Multiple = true,
                    Hints = new("You probably already have a lot of it.", "It improves your survivability.", "It's red."),
                    PedestalHints = new("A whole piece of love"),
                },
                new ItemData()
                {
                    Item = "Heart Container Refill",
                    Name = new("Heart Container Refill", "Sanctuary Heart Container", "Sanc Heart"),
                    InternalItemType = ItemType.HeartContainerRefill,
                    Hints = new("It improves your survivability.", "It's red."),
                    PedestalHints = new("A whole piece of love"),
                },
                new ItemData()
                {
                    Item = "One Hundred Rupees",
                    Name = new("One Hundred Rupees"),
                    InternalItemType = ItemType.OneHundredRupees,
                    Multiple = true,
                    Hints = new("It's just money.", "You probably already have a lot of it.", "It's green."),
                    PedestalHints = new("A large stash of rupees"),
                },
                new ItemData()
                {
                    Item = "Fifty Rupees",
                    Name = new("Fifty Rupees"),
                    InternalItemType = ItemType.FiftyRupees,
                    Multiple = true,
                    Hints = new("It's just money.", "You probably already have a lot of it.", "It's green."),
                    PedestalHints = new("A large stash of rupees"),
                },
                new ItemData()
                {
                    Item = "Single Arrow",
                    Name = new("Single Arrow", "Single Stick", "One Stick"),
                    InternalItemType = ItemType.Arrow,
                    Multiple = true,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("A single stick to shoot at enemies"),
                },
                new ItemData()
                {
                    Item = "Ten Arrows",
                    Name = new("Ten Arrows"),
                    InternalItemType = ItemType.TenArrows,
                    Multiple = true,
                    Hints = new("You could find it in real life."),
                    PedestalHints = new("A bundle of sticks to shoot at enemies"),
                },
                new ItemData()
                {
                    Item = "Three Hundred Rupees",
                    Name = new("Three Hundred Rupees"),
                    InternalItemType = ItemType.ThreeHundredRupees,
                    Multiple = true,
                    Hints = new("It's just money.", "You probably already have a lot of it.", "It's green."),
                    PedestalHints = new("Over half the amount to pay off Zora"),
                },
                new ItemData()
                {
                    Item = "Pegasus Boots",
                    Name = new("Pegasus Boots", "Boots"),
                    InternalItemType = ItemType.Boots,
                    Hints = new("You could use it to get around.", "It's red."),
                    PedestalHints = new("Something to let you run into enemies with your sword"),
                },
                new ItemData()
                {
                    Item = "Half Magic",
                    Name = new("Half Magic"),
                    InternalItemType = ItemType.HalfMagic,
                    Multiple = true,
                    Stages = new Dictionary<int, SchrodingersString>()
                    {
                        {1, new SchrodingersString("Half Magic") },
                        {2, new SchrodingersString("Quarter Magic") }
                    },
                    WhenTracked = new Dictionary<int, SchrodingersString?>()
                    {
                        {1, new SchrodingersString("Tracked half magic") },
                        {2, new SchrodingersString("Tracked quarter magic") }
                    },
                    Hints = new("It's green."),
                    PedestalHints = new("The ability to do more magical things"),
                },
                new ItemData()
                {
                    Item = "+5 Bomb Capacity",
                    Name = new("+5 Bomb Capacity", "Bomb Capacity", "Bomb Upgrade"),
                    InternalItemType = ItemType.BombUpgrade5,
                    Multiple = true,
                    Hints = new("It allows you to carry more of something"),
                    PedestalHints = new("A larger container for primitive explosives"),
                },
                new ItemData()
                {
                    Item = "+5 Arrow Capacity",
                    Name = new("+5 Arrow Capacity", "Arrow Capacity", "Arrow Upgrade"),
                    InternalItemType = ItemType.ArrowUpgrade5,
                    Multiple = true,
                    Hints = new("It allows you to carry more of something"),
                    PedestalHints = new("A larger container for sticks"),
                },
                new ItemData()
                {
                    Item = "Silver Arrows",
                    Name = new("Silver Arrows", "Silvers"),
                    InternalItemType = ItemType.SilverArrows,
                    Hints = new("You can hurt people with it."),
                    PedestalHints = new("Fancy tipped arrows"),
                },
                new ItemData()
                {
                    Item = "Sword",
                    Name = new("Sword", new("Progressive Sword", 0)),
                    InternalItemType = ItemType.ProgressiveSword,
                    Multiple = true,
                    Hints = new("You can hurt people with it."),
                    PedestalHints = new("Something to slash enemies with"),
                },
                new ItemData()
                {
                    Item = "Shield",
                    Name = new("Shield", new("Progressive Shield", 0)),
                    InternalItemType = ItemType.ProgressiveShield,
                    Multiple = true,
                    Hints = new("It improves your survivability."),
                    PedestalHints = new("A way to block arrows and fireballs"),
                },
                new ItemData()
                {
                    Item = "Mail",
                    Name = new("Mail", new("Tunic", 0.1), new("Progressive Mail", 0)),
                    InternalItemType = ItemType.ProgressiveTunic,
                    Multiple = true,
                    Hints = new("It improves your survivability."),
                    PedestalHints = new("A change of clothes to help you survive"),
                },
                new ItemData()
                {
                    Item = "Gloves",
                    Name = new("Gloves", new("Progressive Glove", 0)),
                    InternalItemType = ItemType.ProgressiveGlove,
                    Multiple = true,
                    Hints = new("It's something magical.", "It's something you could wear."),
                    PedestalHints = new("Something to help you lift heavy things"),
                },
                new ItemData()
                {
                    Item = "Ganons Tower Map",
                    Name = new("Ganons Tower Map"),
                    InternalItemType = ItemType.MapGT,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Turtle Rock Map",
                    Name = new("Turtle Rock Map"),
                    InternalItemType = ItemType.MapTR,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Thieves Town Map",
                    Name = new("Thieves Town Map"),
                    InternalItemType = ItemType.MapTT,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Tower of Hera Map",
                    Name = new("Tower of Hera Map"),
                    InternalItemType = ItemType.MapTH,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Ice Palace Map",
                    Name = new("Ice Palace Map"),
                    InternalItemType = ItemType.MapIP,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Skull Woods Map",
                    Name = new("Skull Woods Map"),
                    InternalItemType = ItemType.MapSW,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Misery Mire Map",
                    Name = new("Misery Mire Map"),
                    InternalItemType = ItemType.MapMM,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Palace of Darkness Map",
                    Name = new("Palace of Darkness Map"),
                    InternalItemType = ItemType.MapPD,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Swamp Palace Map",
                    Name = new("Swamp Palace Map"),
                    InternalItemType = ItemType.MapSP,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Desert Palace Map",
                    Name = new("Desert Palace Map"),
                    InternalItemType = ItemType.MapDP,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Eastern Palace Map",
                    Name = new("Eastern Palace Map"),
                    InternalItemType = ItemType.MapEP,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Hyrule Castle Map",
                    Name = new("Hyrule Castle Map"),
                    InternalItemType = ItemType.MapHC,
                    Hints = new("It helps you find a place."),
                    Image = "map.png",
                    PedestalHints = new("A piece of paper with some floor plans"),
                },
                new ItemData()
                {
                    Item = "Ganons Tower Compass",
                    Name = new("Ganons Tower Compass"),
                    InternalItemType = ItemType.CompassGT,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Turtle Rock Compass",
                    Name = new("Turtle Rock Compass"),
                    InternalItemType = ItemType.CompassTR,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Thieves Town Compass",
                    Name = new("Thieves Town Compass"),
                    InternalItemType = ItemType.CompassTT,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Tower of Hera Compass",
                    Name = new("Tower of Hera Compass"),
                    InternalItemType = ItemType.CompassTH,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Ice Palace Compass",
                    Name = new("Ice Palace Compass"),
                    InternalItemType = ItemType.CompassIP,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Skull Woods Compass",
                    Name = new("Skull Woods Compass"),
                    InternalItemType = ItemType.CompassSW,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Misery Mire Compass",
                    Name = new("Misery Mire Compass"),
                    InternalItemType = ItemType.CompassMM,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Palace of Darkness Compass",
                    Name = new("Palace of Darkness Compass"),
                    InternalItemType = ItemType.CompassPD,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Swamp Palace Compass",
                    Name = new("Swamp Palace Compass"),
                    InternalItemType = ItemType.CompassSP,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Desert Palace Compass",
                    Name = new("Desert Palace Compass"),
                    InternalItemType = ItemType.CompassDP,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Eastern Palace Compass",
                    Name = new("Eastern Palace Compass"),
                    InternalItemType = ItemType.CompassEP,
                    Hints = new("It points you to the boss."),
                    PedestalHints = new("A tool that points toward the boss"),
                },
                new ItemData()
                {
                    Item = "Ganons Tower Big Key",
                    Name = new("Ganons Tower Big Key"),
                    InternalItemType = ItemType.BigKeyGT,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("The big key of evil's bane"),
                },
                new ItemData()
                {
                    Item = "Turtle Rock Big Key",
                    Name = new("Turtle Rock Big Key"),
                    InternalItemType = ItemType.BigKeyTR,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("The big key of turtles"),
                },
                new ItemData()
                {
                    Item = "Thieves Town Big Key",
                    Name = new("Thieves Town Big Key"),
                    InternalItemType = ItemType.BigKeyTT,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("The big key of rogues"),
                },
                new ItemData()
                {
                    Item = "Tower of Hera Big Key",
                    Name = new("Tower of Hera Big Key"),
                    InternalItemType = ItemType.BigKeyTH,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("The big key to moldorm's heart"),
                },
                new ItemData()
                {
                    Item = "Ice Palace Big Key",
                    Name = new("Ice Palace Big Key"),
                    InternalItemType = ItemType.BigKeyIP,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("A frozen big key"),
                },
                new ItemData()
                {
                    Item = "Skull Woods Big Key",
                    Name = new("Skull Woods Big Key"),
                    InternalItemType = ItemType.BigKeySW,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("The big key of the dark forest"),
                },
                new ItemData()
                {
                    Item = "Misery Mire Big Key",
                    Name = new("Misery Mire Big Key"),
                    InternalItemType = ItemType.BigKeyMM,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("A big key that will make you miserable"),
                },
                new ItemData()
                {
                    Item = "Palace of Darkness Big Key",
                    Name = new("Palace of Darkness Big Key"),
                    InternalItemType = ItemType.BigKeyPD,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("The big key that steals light"),
                },
                new ItemData()
                {
                    Item = "Swamp Palace Big Key",
                    Name = new("Swamp Palace Big Key"),
                    InternalItemType = ItemType.BigKeySP,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("The big key to the floodgates"),
                },
                new ItemData()
                {
                    Item = "Desert Palace Big Key",
                    Name = new("Desert Palace Big Key"),
                    InternalItemType = ItemType.BigKeyDP,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("A sand-filled big key"),
                },
                new ItemData()
                {
                    Item = "Eastern Palace Big Key",
                    Name = new("Eastern Palace Big Key"),
                    InternalItemType = ItemType.BigKeyEP,
                    Hints = new("It opens doors."),
                    Image = "bigkey.png",
                    PedestalHints = new("The big key of the east"),
                },
                new ItemData()
                {
                    Item = "Sewer Key",
                    Name = new("Sewer Key", "Hyrule Castle Key"),
                    InternalItemType = ItemType.KeyHC,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("The key to the castle"),
                },
                new ItemData()
                {
                    Item = "Desert Palace Key",
                    Name = new("Desert Palace Key"),
                    InternalItemType = ItemType.KeyDP,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("A sand-filled key"),
                },
                new ItemData()
                {
                    Item = "Castle Tower Key",
                    Name = new("Castle Tower Key"),
                    InternalItemType = ItemType.KeyCT,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("A key to get the wizard in the tower"),
                },
                new ItemData()
                {
                    Item = "Swamp Palace Key",
                    Name = new("Swamp Palace Key"),
                    InternalItemType = ItemType.KeySP,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("A small key to get into the swamp"),
                },
                new ItemData()
                {
                    Item = "Palace of Darkness Key",
                    Name = new("Palace of Darkness Key"),
                    InternalItemType = ItemType.KeyPD,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("A small key that steals light"),
                },
                new ItemData()
                {
                    Item = "Misery Mire Key",
                    Name = new("Misery Mire Key"),
                    InternalItemType = ItemType.KeyMM,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("A small key that makes you miserable"),
                },
                new ItemData()
                {
                    Item = "Skull Woods Key",
                    Name = new("Skull Woods Key"),
                    InternalItemType = ItemType.KeySW,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("A small key of the dark forest"),
                },
                new ItemData()
                {
                    Item = "Ice Palace Key",
                    Name = new("Ice Palace Key"),
                    InternalItemType = ItemType.KeyIP,
                    Article = "an",
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("A frozen small key"),
                },
                new ItemData()
                {
                    Item = "Tower of Hera Key",
                    Name = new("Tower of Hera Key"),
                    InternalItemType = ItemType.KeyTH,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("The small key to moldorm's basement"),
                },
                new ItemData()
                {
                    Item = "Thieves Town Key",
                    Name = new("Thieves Town Key"),
                    InternalItemType = ItemType.KeyTT,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("The small key of rogues"),
                },
                new ItemData()
                {
                    Item = "Turtle Rock Key",
                    Name = new("Turtle Rock Key"),
                    InternalItemType = ItemType.KeyTR,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("A small key of turtles"),
                },
                new ItemData()
                {
                    Item = "Ganons Tower Key",
                    Name = new("Ganons Tower Key"),
                    InternalItemType = ItemType.KeyGT,
                    Multiple = true,
                    Hints = new("It opens doors."),
                    Image = "smallkey.png",
                    PedestalHints = new("A small key of evil's bane"),
                },
                new ItemData()
                {
                    Item = "Grappling Beam",
                    Name = new("Grappling Beam", "Grapple Beam", "Grapple"),
                    InternalItemType = ItemType.Grapple,
                    Hints = new("You could use it to get around."),
                    PedestalHints = new("The key to Shaktool's front door"),
                },
                new ItemData()
                {
                    Item = "X-Ray Scope",
                    Name = new("X-Ray Scope", "X-Ray"),
                    InternalItemType = ItemType.XRay,
                    Hints = new("You could use it to see things you usually can't."),
                    PedestalHints = new("A way to peek into walls"),
                },
                new ItemData()
                {
                    Item = "Varia Suit",
                    Name = new("Varia Suit"),
                    InternalItemType = ItemType.Varia,
                    Hints = new("It's something you could wear.", "It improves your survivability."),
                    PedestalHints = new("Alien armor that keeps you cool"),
                },
                new ItemData()
                {
                    Item = "Spring Ball",
                    Name = new("Spring Ball"),
                    InternalItemType = ItemType.SpringBall,
                    Hints = new("You could use it to get around."),
                    PedestalHints = new("A way for you to bounce around when you're in ball form"),
                },
                new ItemData()
                {
                    Item = "Morphing Ball",
                    Name = new("Morphing Ball", "Morph Ball", "Morph"),
                    InternalItemType = ItemType.Morph,
                    PedestalHints = new("Some sort of object that lets you roll around"),
                },
                new ItemData()
                {
                    Item = "Screw Attack",
                    Name = new("Screw Attack"),
                    InternalItemType = ItemType.ScrewAttack,
                    Hints = new("You can hurt people with it."),
                    PedestalHints = new("Some sort of destructive shield while spinning"),
                },
                new ItemData()
                {
                    Item = "Gravity Suit",
                    Name = new("Gravity Suit"),
                    InternalItemType = ItemType.Gravity,
                    Hints = new("It's something you could wear.", "It improves your survivability."),
                    PedestalHints = new("Alien armor that goes through water easily"),
                },
                new ItemData()
                {
                    Item = "Hi-Jump Boots",
                    Name = new("Hi-Jump Boots", "Hi-Jump"),
                    InternalItemType = ItemType.HiJump,
                    Hints = new("You could use it to get around.", "It's something you could wear."),
                    PedestalHints = new("Shoes that help you reach slightly higher places"),
                },
                new ItemData()
                {
                    Item = "Space Jump",
                    Name = new("Space Jump"),
                    InternalItemType = ItemType.SpaceJump,
                    Hints = new("You could use it to get around."),
                    PedestalHints = new("A way to fly high in the sky"),
                },
                new ItemData()
                {
                    Item = "Morph Bombs",
                    Name = new("Morph Bombs", "Morph Bomb", "Morph Ball Bombs"),
                    InternalItemType = ItemType.Bombs,
                    Hints = new("It's useful but not required."),
                    PedestalHints = new("Basic reusable futuristic explosives"),
                },
                new ItemData()
                {
                    Item = "Speed Booster",
                    Name = new("Speed Booster"),
                    InternalItemType = ItemType.SpeedBooster,
                    Hints = new("You could use it to get around."),
                    PedestalHints = new("An object that will let you run fast through blocks"),
                },
                new ItemData()
                {
                    Item = "Charge Beam",
                    Name = new("Charge Beam"),
                    InternalItemType = ItemType.Charge,
                    Hints = new("You can hurt people with it."),
                    PedestalHints = new("An upgrade to your beam to make them more powerful"),
                },
                new ItemData()
                {
                    Item = "Ice Beam",
                    Name = new("Ice Beam"),
                    InternalItemType = ItemType.Ice,
                    Hints = new("You can hurt people with it.", "It's cold.", "It's blue."),
                    PedestalHints = new("Some sort of freeze ray gun"),
                },
                new ItemData()
                {
                    Item = "Wave Beam",
                    Name = new("Wave Beam"),
                    InternalItemType = ItemType.Wave,
                    Hints = new("You can hurt people with it."),
                    PedestalHints = new("A gun that shoots through walls"),
                },
                new ItemData()
                {
                    Item = "Spazer",
                    Name = new(new("Spazer", 0), "Spayzer"),
                    InternalItemType = ItemType.Spazer,
                    Hints = new("You can hurt people with it."),
                    PedestalHints = new("Some sort of spread gun"),
                },
                new ItemData()
                {
                    Item = "Plasma Beam",
                    Name = new("Plasma Beam", "Plasma"),
                    InternalItemType = ItemType.Plasma,
                    Hints = new("You can hurt people with it."),
                    PedestalHints = new("A gun that pierces enemies"),
                },
                new ItemData()
                {
                    Item = "Energy Tank",
                    Name = new("Energy Tank", "E-Tank"),
                    InternalItemType = ItemType.ETank,
                    Article = "an",
                    Plural = new("Energy Tanks", "E-Tanks"),
                    Multiple = true,
                    Hints = new("You probably already have a lot of it.", "It improves your survivability."),
                    PedestalHints = new("Some sort of future tank of health"),
                },
                new ItemData()
                {
                    Item = "Reserve Tank",
                    Name = new("Reserve Tank"),
                    InternalItemType = ItemType.ReserveTank,
                    Plural = new("Reserve Tanks"),
                    Multiple = true,
                    Hints = new("You probably already have a lot of it.", "It improves your survivability."),
                    PedestalHints = new("A futuristic object to restore health before dying"),
                },
                new ItemData()
                {
                    Item = "Missile",
                    Name = new("Missile", new("Missiles", 0), new("Missile pack", 0.2)),
                    InternalItemType = ItemType.Missile,
                    Plural = new("Missiles", new("Missile packs", 0)),
                    Multiple = true,
                    CounterMultiplier = 5,
                    Hints = new("You probably already have a lot of it.", "It allows you to carry more of something"),
                    PedestalHints = new("A small collection of flying bombs"),
                },
                new ItemData()
                {
                    Item = "Super Missile",
                    Name = new("Super Missile", new("Supers", 0), new("Super Missile pack", 0)),
                    InternalItemType = ItemType.Super,
                    Plural = new("Super Missiles", "Supers", new("Super Missile packs", 0)),
                    Multiple = true,
                    CounterMultiplier = 5,
                    Hints = new("You probably already have a lot of it.", "It allows you to carry more of something"),
                    PedestalHints = new("Some sort of large flying bomb"),
                },
                new ItemData()
                {
                    Item = "Power Bomb",
                    Name = new("Power Bomb", new("Power Bombs", 0)),
                    InternalItemType = ItemType.PowerBomb,
                    Plural = new("Power Bombs", "Powerful Bombs"),
                    Multiple = true,
                    CounterMultiplier = 5,
                    Hints = new("You probably already have a lot of it.", "It allows you to carry more of something"),
                    PedestalHints = new("A massive explosive device"),
                },
                new ItemData()
                {
                    Item = "Crateria Level 1 Keycard",
                    Name = new("Crateria Level 1 Keycard"),
                    InternalItemType = ItemType.CardCrateriaL1,
                    Hints = new("It opens doors."),
                    Image = "smkey1.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Crateria"),
                },
                new ItemData()
                {
                    Item = "Crateria Level 2 Keycard",
                    Name = new("Crateria Level 2 Keycard"),
                    InternalItemType = ItemType.CardCrateriaL2,
                    Hints = new("It opens doors."),
                    Image = "smkey2.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Crateria"),
                },
                new ItemData()
                {
                    Item = "Crateria Boss Keycard",
                    Name = new("Crateria Boss Keycard"),
                    InternalItemType = ItemType.CardCrateriaBoss,
                    Hints = new("It opens doors."),
                    Image = "smkeyboss.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Crateria"),
                },
                new ItemData()
                {
                    Item = "Brinstar Level 1 Keycard",
                    Name = new("Brinstar Level 1 Keycard"),
                    InternalItemType = ItemType.CardBrinstarL1,
                    Hints = new("It opens doors."),
                    Image = "smkey1.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Brinstar"),
                },
                new ItemData()
                {
                    Item = "Brinstar Level 2 Keycard",
                    Name = new("Brinstar Level 2 Keycard"),
                    InternalItemType = ItemType.CardBrinstarL2,
                    Hints = new("It opens doors."),
                    Image = "smkey2.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Brinstar"),
                },
                new ItemData()
                {
                    Item = "Brinstar Boss Keycard",
                    Name = new("Brinstar Boss Keycard"),
                    InternalItemType = ItemType.CardBrinstarBoss,
                    Hints = new("It opens doors."),
                    Image = "smkeyboss.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Brinstar"),
                },
                new ItemData()
                {
                    Item = "Norfair Level 1 Keycard",
                    Name = new("Norfair Level 1 Keycard"),
                    InternalItemType = ItemType.CardNorfairL1,
                    Hints = new("It opens doors."),
                    Image = "smkey1.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Upper Norfair"),
                },
                new ItemData()
                {
                    Item = "Norfair Level 2 Keycard",
                    Name = new("Norfair Level 2 Keycard"),
                    InternalItemType = ItemType.CardNorfairL2,
                    Hints = new("It opens doors."),
                    Image = "smkey2.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Upper Norfair"),
                },
                new ItemData()
                {
                    Item = "Norfair Boss Keycard",
                    Name = new("Norfair Boss Keycard"),
                    InternalItemType = ItemType.CardNorfairBoss,
                    Hints = new("It opens doors."),
                    Image = "smkeyboss.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Upper Norfair"),
                },
                new ItemData()
                {
                    Item = "Maridia Level 1 Keycard",
                    Name = new("Maridia Level 1 Keycard"),
                    InternalItemType = ItemType.CardMaridiaL1,
                    Hints = new("It opens doors."),
                    Image = "smkey1.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Maridia"),
                },
                new ItemData()
                {
                    Item = "Maridia Level 2 Keycard",
                    Name = new("Maridia Level 2 Keycard"),
                    InternalItemType = ItemType.CardMaridiaL2,
                    Hints = new("It opens doors."),
                    Image = "smkey2.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Maridia"),
                },
                new ItemData()
                {
                    Item = "Maridia Boss Keycard",
                    Name = new("Maridia Boss Keycard"),
                    InternalItemType = ItemType.CardMaridiaBoss,
                    Hints = new("It opens doors."),
                    Image = "smkeyboss.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Maridia"),
                },
                new ItemData()
                {
                    Item = "Wrecked Ship Level 1 Keycard",
                    Name = new("Wrecked Ship Level 1 Keycard"),
                    InternalItemType = ItemType.CardWreckedShipL1,
                    Hints = new("It opens doors."),
                    Image = "smkey1.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Wrecked Ship"),
                },
                new ItemData()
                {
                    Item = "Wrecked Ship Boss Keycard",
                    Name = new("Wrecked Ship Boss Keycard"),
                    InternalItemType = ItemType.CardWreckedShipBoss,
                    Hints = new("It opens doors."),
                    Image = "smkeyboss.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Wrecked Ship"),
                },
                new ItemData()
                {
                    Item = "Lower Norfair Level 1 Keycard",
                    Name = new("Lower Norfair Level 1 Keycard"),
                    InternalItemType = ItemType.CardLowerNorfairL1,
                    Hints = new("It opens doors."),
                    Image = "smkey1.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Lower Norfair"),
                },
                new ItemData()
                {
                    Item = "Lower Norfair Boss Keycard",
                    Name = new("Lower Norfair Boss Keycard"),
                    InternalItemType = ItemType.CardLowerNorfairBoss,
                    Hints = new("It opens doors."),
                    Image = "smkeyboss.png",
                    PedestalHints = new("A device that unlocks futuristic doors in Lower Norfair"),
                },
            };
        }
    }
}
