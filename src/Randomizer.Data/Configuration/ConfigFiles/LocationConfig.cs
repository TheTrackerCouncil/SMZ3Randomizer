using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.Data.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.Data.Configuration.ConfigTypes;
using System.Linq;

namespace Randomizer.Data.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional location information
    /// </summary>
    public class LocationConfig : List<LocationInfo>, IMergeable<LocationInfo>, IConfigFile<LocationConfig>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LocationConfig() : base()
        {
        }

        /// <summary>
        /// Returns default location information
        /// </summary>
        /// <returns></returns>
        public static LocationConfig Default()
        {
            return new LocationConfig
            {
                new LocationInfo()
                {
                    LocationNumber = 0,
                    Name = new("Chozo Ruins entrance", "that room opposite the Gauntlet", new("Power Bomb (Crateria surface)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 1,
                    Name = new("Flooded Cavern (under water)", "West Ocean (under water)", "that room before Wrecked Ship that's underwater", new("Missile (outside Wrecked Ship bottom)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 2,
                    Name = new("Sky Missile", "that spot all the way up in the sky after Wrecked Ship", new("Missile (outside Wrecked Ship top)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 3,
                    Name = new("Morph Ball Maze", new("Missile (outside Wrecked Ship middle)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 4,
                    Name = new("The Moat", "Interior Lake", "that room between Crateria and Wrecked Ship with the item on the water", new("Missile (Crateria moat)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 5,
                    Name = new("Gauntlet (Chozo)", "the room in the middle of the Gauntlet", new("Energy Tank, Gauntlet", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 6,
                    Name = new("Mother Brain's reliquary", "Pit Room", "the spot underneath the place Mother Brain used to be", new("Missile (Crateria bottom)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 7,
                    Name = new("Bozo", "Bomb Torizo room", new("Bombs", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 8,
                    Name = new("Terminator Room", "Fungal Slope", "that room in Crateria with the mushrooms", new("Energy Tank, Terminator", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 9,
                    Name = new("Gauntlet Shaft Right", "that spot on the right side after the Gauntlet", new("Missile (Crateria gauntlet right)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 10,
                    Name = new("Gauntlet Shaft Left", "that spot on the left side after the Gauntlet", new("Missile (Crateria gauntlet left)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 11,
                    Name = new("Old Tourian launchpad", new("Super Missile (Crateria)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 12,
                    Name = new("Final Missile Bombway", "The Final Missile", "Dental Plan Missiles", "the room in Crateria on the other side of the entrance to Zelda", new("Missile (Crateria middle)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 13,
                    Name = new("Hell", "Etecoon shaft", "that room where the animals are wall jumping", new("Power Bomb (green Brinstar bottom)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 14,
                    Name = new("Spore Spawn's item", new("Super Missile (pink Brinstar)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 15,
                    Name = new("Mockball Room (Fail item)", "that spot you get to when failing the mock ball in Green Brinstar", new("Missile (green Brinstar below super missile)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 16,
                    Name = new("Mockball Room Attic", "that spot on your way out of the mockball room", new("Super Missile (green Brinstar top)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 17,
                    Name = new("Mockball Chozo", "the room after you use the Speed Booster in Green Brinstar", "the room after you use the mockball glitch in Green Brinstar", new("Reserve Tank, Brinstar", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 18,
                    Name = new("Mockball Back room hidden item", "the room behind the room after you use the Speed Booster in Green Brinstar", "the room behind the room after you use the mockball glitch in Green Brinstar", new("Missile (green Brinstar behind missile)", 0), "Ron Popeil missiles", "Mockball Hall Hidden Room Hidden Item"),
                },
                new LocationInfo()
                {
                    LocationNumber = 19,
                    Name = new("Mockball Back room", "that spot in the wall in the room behind the room after you use the Speed Booster in Green Brinstar", "that spot in the wall in the room behind the room after you use the mockball glitch in Green Brinstar", new("Missile (green Brinstar behind reserve tank)", 0), "Mockball Hall Hidden Room Main Item"),
                },
                new LocationInfo()
                {
                    LocationNumber = 21,
                    Name = new("Pink Shaft (top)", "Big Pink (top)", "that spot in Pink Brinstar on the other side of the grapple ceiling", new("Missile (pink Brinstar top)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 22,
                    Name = new("Pink Shaft (bottom)", "Big Pink (bottom)", "that spot on the bottom of Pink Brinstar", new("Missile (pink Brinstar bottom)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 23,
                    Name = new("Pink Shaft (Chozo)", "the room underneath Pink Brinstar", new("Charge Beam", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 24,
                    Name = new("Mission: Impossible", "Pink Brinstar Power Bomb Room", new("Power Bomb (pink Brinstar)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 25,
                    Name = new("Green Hill Zone", "Jungle slope", new("Missile (green Brinstar pipe)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 26,
                    Name = new("Morph Ball (Corridor No. 1)", "Morphing Ball", "the only place you can get to in Super Metroid without any items"),
                },
                new LocationInfo()
                {
                    LocationNumber = 27,
                    Name = new("Power Bomb wall (Corridor No. 1)", "that spot behind the Morph Ball", new("Power Bomb (blue Brinstar)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 28,
                    Name = new("that spot all the way to the right of Blue Brinstar", new("Missile (blue Brinstar middle)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 29,
                    Name = new("Blue Brinstar Ceiling", "that spot in the ceiling in Blue Brinstar", new("Energy Tank, Brinstar Ceiling", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 30,
                    Name = new("that spot next to the false floor in Green Brinstar", "Highway to Hell", new("Energy Tank, Etecoons", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 31,
                    Name = new("that room after the false floor in Green Brinstar", new("Super Missile (green Brinstar bottom)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 33,
                    Name = new("Waterway", new("Energy Tank, Waterway", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 34,
                    Name = new("that Chozo room at the bottom of Blue Brinstar", new("Missile (blue Brinstar bottom)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 35,
                    Name = new("Hoptank Room", "Wave Beam Glitch room", "the room where everyone does the Wave Beam Glitch", new("Energy Tank, Brinstar Gate", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 36,
                    Name = new("Billy Mays Room Main Item", new("Missile (blue Brinstar top)", 0), "Blue Brinstar Top Main Item", "the visible spot in the Billy Mays Room"),
                },
                new LocationInfo()
                {
                    LocationNumber = 37,
                    Name = new("Billy Mays Room Hidden item", "But wait, there's more!", new("Missile (blue Brinstar behind missile)", 0), "Blue Brinstar Top Hidden Item", "the hidden spot in the Billy Mays Room"),
                },
                new LocationInfo()
                {
                    LocationNumber = 38,
                    Name = new("The Chozo room after the dark room with all the spikes", "X-Ray Scope", new("X-Ray", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 39,
                    Name = new("Beta Power Bomb Room", "Power Bomb (red Brinstar sidehopper room)", "the room underneath the flower in Red Brinstar", "the room underneath the Samus Eater in Red Brinstar"),
                },
                new LocationInfo()
                {
                    LocationNumber = 40,
                    Name = new("Alpha Power Bomb Room", "Power Bomb (red Brinstar spike room)", "the Chozo room in Red Brinstar with the jumping enemies you can freeze", "the Chozo room in Red Brinstar with the Boyons"),
                },
                new LocationInfo()
                {
                    LocationNumber = 41,
                    Name = new("Alpha Power Bomb Room (Behind the wall)", "Missile (red Brinstar spike room)", "the room behind the wall in the Chozo room in Red Brinstar"),
                },
                new LocationInfo()
                {
                    LocationNumber = 42,
                    Name = new("~ S p A z E r ~", "Spazer", "the Chozo room above the entrance to Maridia in Red Brinstar"),
                },
                new LocationInfo()
                {
                    LocationNumber = 43,
                    Name = new("Energy Tank, Kraid", "the room that opens up on your way out after defeating Kraid"),
                },
                new LocationInfo()
                {
                    LocationNumber = 44,
                    Name = new("Warehouse Kihunter Room", "Missile (Kraid)", "the room before the long corridor leading to Kraid with the hidden item in the wall"),
                },
                new LocationInfo()
                {
                    LocationNumber = 48,
                    Name = new("Kraid's Reliquary", "the room after Kraid", new("Varia Suit", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 49,
                    Name = new("Lava Room (Submerged in wall)", "Cathedral", "the room in Upper Norfair with the item in the lava", new("Missile (lava room)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 50,
                    Name = new("Ice Beam Room", "Ice Beam", "the room in Upper Norfair after the Speed Booster gates"),
                },
                new LocationInfo()
                {
                    LocationNumber = 51,
                    Name = new("Crumble Shaft", "the room with all the crumbling platforms", new("Missile (below Ice Beam)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 52,
                    Name = new("Crocomire's Pit", new("Energy Tank, Crocomire", 0), "the room where you fight Crocomire"),
                },
                new LocationInfo()
                {
                    LocationNumber = 53,
                    Name = new("Hi-Jump Boots Room", "Hi-Jump Boots", "the Chozo room after the room in Upper Norfair where you have to wait a long time for an enemy before you can leave again"),
                },
                new LocationInfo()
                {
                    LocationNumber = 54,
                    Name = new("Crocomire Escape", new("Missile (above Crocomire)", 0), "the room with the item beyond a Super Missile gate you can peak at in Upper Norfair"),
                },
                new LocationInfo()
                {
                    LocationNumber = 55,
                    Name = new("Hi-Jump Lobby (Back)", "the spot on your way out of the room in the Upper Norfair where you have to wait a long time for an enemy before you can leave again", new("Missile (Hi-Jump Boots)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 56,
                    Name = new("Hi-Jump Lobby (Entrance)", "the room in the Upper Norfair where you have to wait a long time for an enemy before you can leave again", new("Energy Tank (Hi-Jump Boots)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 57,
                    Name = new("Post Crocomire Power Bomb Room", "the room directly on the other side of Crocomire's room", new("Power Bomb (Crocomire)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 58,
                    Name = new("Cosine Room", "Post Crocomire Missile Room", "the wavy room below Crocomire", new("Missile (below Crocomire)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 59,
                    Name = new("Indiana Jones Room", "Pantry", "Post Crocomire Jump Room", "the big empty room after Crocomire that use the Speed Booster to get", new("Missile (Grappling Beam)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 60,
                    Name = new("Grapple Beam Room", "Grappling Beam", "the Chozo room after the big empty room past Crocomire"),
                },
                new LocationInfo()
                {
                    LocationNumber = 61,
                    Name = new("Reserve Tank, Norfair", "Bubble Mountain Hidden Hall Main Item", "the hidden room on the left side of Bubble Mountain"),
                },
                new LocationInfo()
                {
                    LocationNumber = 62,
                    Name = new("Missile (Norfair Reserve Tank)", "Bubble Mountain Hidden Hall Hidden Item", "the hidden spot in the hidden room on the left side of Bubble Mountain"),
                },
                new LocationInfo()
                {
                    LocationNumber = 63,
                    Name = new("Bubble Mountain Missile Room", "the room on the left side of Bubble Mountain", new("Missile (bubble Norfair green door)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 64,
                    Name = new("Bubble Mountain", "the spot near the spikes on the bottom of Bubble Mountain", new("Missile (bubble Norfair)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 65,
                    Name = new("Speed Booster Hall (Ceiling)", "the spot in the ceiling on the right side of Bubble Mountain", new("Missile (Speed Booster)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 66,
                    Name = new("Speed Booster Room", "Speed Booster", "the room to the right of Bubble Mountain", "the Chozo room in Bubble Mountain"),
                },
                new LocationInfo()
                {
                    LocationNumber = 67,
                    Name = new("Double Chamber", "Grapple Crossing", new("Missile (Wave Beam)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 68,
                    Name = new("Wave Beam Room", "Wave Beam"),
                },
                new LocationInfo()
                {
                    LocationNumber = 70,
                    Name = new("Gold Torizo (Drop down)", "the spot before you drop down to fight Gold Torizo", new("Missile (Gold Torizo)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 71,
                    Name = new("Golden Torizo (Ceiling)", "the spot where Gold Torizo hides before the fight", new("Super Missile (Gold Torizo)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 73,
                    Name = new("Mickey Mouse Clubhouse", "Mickey Mouse room", "the room that looks like Mickey Mouse", new("Missile (Mickey Mouse room)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 74,
                    Name = new("Spring Ball Maze Room", "the room right before the Spring Ball Maze", new("Missile (lower Norfair above fire flea room)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 75,
                    Name = new("Escape Power Bomb Room", "the room in lower Norfair that you hope doesn't have anything because you need to go through a morph ball maze", new("Power Bomb (lower Norfair above fire flea room)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 76,
                    Name = new("Power Bomb of Shame", "the room to the left before the spiky elevator to Ridley", new("Power Bomb (Power Bombs of shame)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 77,
                    Name = new("Three Musketeer's Room", "FrankerZ Missiles", new("Missile (lower Norfair near Wave Beam)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 78,
                    Name = new("Ridley's Reliquary", "the room after Ridley", new("Energy Tank, Ridley", 0)),
                    Hints = new("I hear Ridley is about to open one of Kraid's Christmas presents again."),
                },
                new LocationInfo()
                {
                    LocationNumber = 79,
                    Name = new("Screw Attack", "the Chozo below the portal to Misery Mire"),
                },
                new LocationInfo()
                {
                    LocationNumber = 80,
                    Name = new("Fireflea Room", "the dark room in lower Norfair with the fire flies", new("Energy Tank, Firefleas", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 128,
                    Name = new("Main Shaft (Side room)", "the room behind the bombeable wall on the left side of Wrecked Ship", new("Missile (Wrecked Ship middle)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 129,
                    Name = new("Post Chozo Concert (Speed Booster Item)", "Bowling Alley (Speed Booster Item)", "the spot that requires the Speed Booster in Wrecked Ship", new("Reserve Tank, Wrecked Ship", 0), new("Wrecked Ship, Reserve Tank", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 130,
                    Name = new("Post Chozo Concert (Breakable Chozo)", "the breakable Chozo in Wrecked Ship", new("Missile (Gravity Suit)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 131,
                    Name = new("Attic (Assembly Line)", "the room with the conveyors and robots", new("Missile (Wrecked Ship top)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 132,
                    Name = new("Wrecked Pool", "Ruined Pool", "Wrecked Ship Bullshit Room", "that room in Wrecked Ship with all the water where you always reset", new("Energy Tank, Wrecked Ship", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 133,
                    Name = new("Left Super Missile Chamber", "the room that opens up on the left side after defeating Phantoon", new("Super Missile (Wrecked Ship left)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 134,
                    Name = new("Right Super Missile Chamber", "the room that opens up on the right side after defeating Phantoon", new("Right Super, Wrecked Ship", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 135,
                    Name = new("Post Chozo Concert (Gravity Suit Chamber)", "the room after the Chozo concert", new("Gravity Suit", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 136,
                    Name = new("Main Street (Ceiling Shinespark)", "Main Street Missiles", "the spot in the ceiling in Maridia that requires Shinespark", new("Missile (green Maridia shinespark)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 137,
                    Name = new("Main Street (Crab Supers)", "the tiny room where you watch crabs walking into the wall", new("Super Missile (green Maridia)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 138,
                    Name = new("Mama Turtle Room", "the room with the turtles", new("Energy Tank, Mama turtle", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 139,
                    Name = new("Mama Turtle Room (Wall item)", "the spot in the wall in the turtle room", new("Missile (green Maridia tatori)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 140,
                    Name = new("Watering Hole Left", "the left side of the pit with two items in the northeast of Maridia", new("Super Missile (yellow Maridia)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 141,
                    Name = new("Watering Hole Right", "the right side of the pit with two items in the northeast of Maridia", new("Missile (yellow Maridia super missile)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 142,
                    Name = new("Pseudo Plasma Spark Room", "the room leading up to the pit with two items in the northeast of Maridia", new("Missile (yellow Maridia false wall)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 143,
                    Name = new("Plasma Beam", "Plasma Chamber", "Plasma Beam room", "the room in Maridia near the forgotten highway to Wrecked Ship"),
                },
                new LocationInfo()
                {
                    LocationNumber = 144,
                    Name = new("Left Sand Pit Left", "the left side of the left sand pit", new("Missile (left Maridia sand pit room)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 145,
                    Name = new("Left Sand Pit Right", "the right side of the left sand pit", new("Reserve Tank, Maridia", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 146,
                    Name = new("Right Sand Pit Left", "the left side of the right sand pit", new("Missile (right Maridia sand pit room)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 147,
                    Name = new("Right Sand Pit Right", "the right side of the right sand pit", new("Power Bomb (right Maridia sand pit room)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 148,
                    Name = new("Aqueduct (Left item)", new("Missile (pink Maridia)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 149,
                    Name = new("Aqueduct (Right item)", new("Super Missile (pink Maridia)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 150,
                    Name = new("Shaktool", "Shak's Stash", "Shaktool's item", "the room that everyone's favorite diggy boy helps you get to", "the room after everyone's favorite diggy boy", new("Spring Ball", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 151,
                    Name = new("The Precious Room", "Pre-Draygon Room", "the wall in the room before Draygon", new("Missile (Draygon)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 152,
                    Name = new("Sandy Path", "the room after Botwoon", new("Energy Tank, Botwoon", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 154,
                    Name = new("Draygon's Reliquary", "the room after Draygon", new("Space Jump", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 256,
                    Name = new("Ether Tablet"),
                },
                new LocationInfo()
                {
                    LocationNumber = 257,
                    Name = new("Spectacle Rock"),
                },
                new LocationInfo()
                {
                    LocationNumber = 258,
                    Name = new("Spectacle Rock Cave"),
                },
                new LocationInfo()
                {
                    LocationNumber = 259,
                    Name = new("Old Man"),
                },
                new LocationInfo()
                {
                    LocationNumber = 260,
                    Name = new("Floating Island"),
                },
                new LocationInfo()
                {
                    LocationNumber = 261,
                    Name = new("Spiral Cave"),
                },
                new LocationInfo()
                {
                    LocationNumber = 262,
                    Name = new("Paradox Cave Upper (Left)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 263,
                    Name = new("Paradox Cave Upper (Right)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 264,
                    Name = new("Paradox Cave Lower (Far Left)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 265,
                    Name = new("Paradox Cave Lower (Left)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 266,
                    Name = new("Paradox Cave Lower (Middle)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 267,
                    Name = new("Paradox Cave Lower (Right)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 268,
                    Name = new("Paradox Cave Lower (Far Right)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 269,
                    Name = new("Mimic Cave"),
                },
                new LocationInfo()
                {
                    LocationNumber = 270,
                    Name = new("Master Sword Pedestal", "Pedestal", "Ped"),
                    Hints = new("I hope you don't need it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 271,
                    Name = new("Mushroom"),
                },
                new LocationInfo()
                {
                    LocationNumber = 272,
                    Name = new("Lost Woods Hideout"),
                },
                new LocationInfo()
                {
                    LocationNumber = 273,
                    Name = new("Lumberjack Tree", "Lumberjack Ledge"),
                },
                new LocationInfo()
                {
                    LocationNumber = 274,
                    Name = new("Bonk Rocks", "Pegasus Rocks"),
                },
                new LocationInfo()
                {
                    LocationNumber = 275,
                    Name = new("Graveyard Ledge"),
                },
                new LocationInfo()
                {
                    LocationNumber = 276,
                    Name = new("King's Tomb"),
                },
                new LocationInfo()
                {
                    LocationNumber = 277,
                    Name = new("Kakariko Well Back cave", "Kakariko Well Top"),
                },
                new LocationInfo()
                {
                    LocationNumber = 278,
                    Name = new("Kakariko Well Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 279,
                    Name = new("Kakariko Well Middle"),
                },
                new LocationInfo()
                {
                    LocationNumber = 280,
                    Name = new("Kakariko Well Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 281,
                    Name = new("Kakariko Well Bottom"),
                },
                new LocationInfo()
                {
                    LocationNumber = 282,
                    Name = new("Blind's Hideout Back Room", "Blind's Hideout Top"),
                },
                new LocationInfo()
                {
                    LocationNumber = 283,
                    Name = new("Blind's Hideout Far Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 284,
                    Name = new("Blind's Hideout Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 285,
                    Name = new("Blind's Hideout Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 286,
                    Name = new("Blind's Hideout Far Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 287,
                    Name = new("Bottle Merchant"),
                    Hints = new("You know it's a scam, right?"),
                },
                new LocationInfo()
                {
                    LocationNumber = 289,
                    Name = new("Sick Kid", "Bug Catching Kid's House"),
                },
                new LocationInfo()
                {
                    LocationNumber = 290,
                    Name = new("Kakariko Tavern", "Inn back room"),
                },
                new LocationInfo()
                {
                    LocationNumber = 291,
                    Name = new("Magic Bat"),
                },
                new LocationInfo()
                {
                    LocationNumber = 292,
                    Name = new("Zora", "King Zora"),
                    Hints = new("You know it's a scam, right?"),
                },
                new LocationInfo()
                {
                    LocationNumber = 293,
                    Name = new("Zora's Ledge"),
                },
                new LocationInfo()
                {
                    LocationNumber = 295,
                    Name = new("Waterfall Fairy Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 296,
                    Name = new("Potion Shop", "Mushroom Item"),
                },
                new LocationInfo()
                {
                    LocationNumber = 297,
                    Name = new("Sahasrahla's Hut Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 298,
                    Name = new("Sahasrahla's Hut Middle"),
                },
                new LocationInfo()
                {
                    LocationNumber = 299,
                    Name = new("Sahasrahla's Hut Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 300,
                    Name = new("Sahasrahla"),
                },
                new LocationInfo()
                {
                    LocationNumber = 301,
                    Name = new("Maze Race", "Racing Game"),
                },
                new LocationInfo()
                {
                    LocationNumber = 307,
                    Name = new("Mini Moldorm Cave Far Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 308,
                    Name = new("Mini Moldorm Cave Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 309,
                    Name = new("Mini Moldorm Cave NPC"),
                },
                new LocationInfo()
                {
                    LocationNumber = 310,
                    Name = new("Mini Moldorm Cave Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 314,
                    Name = new("Bombos Tablet"),
                },
                new LocationInfo()
                {
                    LocationNumber = 315,
                    Name = new("Floodgate Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 316,
                    Name = new("Sunken Treasure"),
                },
                new LocationInfo()
                {
                    LocationNumber = 317,
                    Name = new("Lake Hylia Island"),
                },
                new LocationInfo()
                {
                    LocationNumber = 318,
                    Name = new("Under the bridge", "the guy under the bridge", "Hobo"),
                },
                new LocationInfo()
                {
                    LocationNumber = 319,
                    Name = new("Ice Cave", "Ice Rod Cave"),
                },
                new LocationInfo()
                {
                    LocationNumber = 320,
                    Name = new("Spike Cave"),
                },
                new LocationInfo()
                {
                    LocationNumber = 321,
                    Name = new("Hookshot Cave (Top Right)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 322,
                    Name = new("Hookshot Cave (Top Left)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 323,
                    Name = new("Hookshot Cave (Bottom Left)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 324,
                    Name = new("Hookshot Cave (Bottom Right)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 325,
                    Name = new("Superbunny Cave (Top)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 326,
                    Name = new("Superbunny Cave (Bottom)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 327,
                    Name = new("Bumper Cave Ledge", "Bumper Cave", "Bumper Ledge"),
                },
                new LocationInfo()
                {
                    LocationNumber = 328,
                    Name = new("Chest Game"),
                },
                new LocationInfo()
                {
                    LocationNumber = 329,
                    Name = new("C-Shaped House"),
                },
                new LocationInfo()
                {
                    LocationNumber = 330,
                    Name = new("Brewery"),
                },
                new LocationInfo()
                {
                    LocationNumber = 331,
                    Name = new("Peg World", "Hammer Pegs"),
                },
                new LocationInfo()
                {
                    LocationNumber = 332,
                    Name = new("Blacksmith"),
                },
                new LocationInfo()
                {
                    LocationNumber = 333,
                    Name = new("Purple Chest turn-in", "Purple Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 334,
                    Name = new("Catfish", "Scatfish", "Lake of Ill Omen"),
                    Hints = new("You know it's a scam, right?"),
                },
                new LocationInfo()
                {
                    LocationNumber = 335,
                    Name = new("Pyramid of Power", "Pyramid"),
                },
                new LocationInfo()
                {
                    LocationNumber = 336,
                    Name = new("Cursed Fairy Left", "Pyramid Fairy Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 337,
                    Name = new("Cursed Fairy Right", "Pyramid Fairy Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 338,
                    Name = new("Digging Game"),
                },
                new LocationInfo()
                {
                    LocationNumber = 339,
                    Name = new("Haunted Grove", "Stumpy"),
                },
                new LocationInfo()
                {
                    LocationNumber = 340,
                    Name = new("Hype Cave Top"),
                },
                new LocationInfo()
                {
                    LocationNumber = 341,
                    Name = new("Hype Cave Middle Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 342,
                    Name = new("Hype Cave Middle Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 343,
                    Name = new("Hype Cave Bottom"),
                },
                new LocationInfo()
                {
                    LocationNumber = 344,
                    Name = new("Hype Cave NPC"),
                },
                new LocationInfo()
                {
                    LocationNumber = 345,
                    Name = new("Mire Shed (Left)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 346,
                    Name = new("Mire Shed (Right)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 347,
                    Name = new("Sanctuary"),
                },
                new LocationInfo()
                {
                    LocationNumber = 348,
                    Name = new("Secret Room (Left)", "Back of Escape (Left)", new("Sewers Secret Room (Left)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 349,
                    Name = new("Secret Room (Middle)", "Back of Escape (Middle)", new("Sewers Secret Room (Middle)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 350,
                    Name = new("Secret Room (Right)", "Back of Escape (Right)", new("Sewers Secret Room (Right)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 351,
                    Name = new("Dark Cross"),
                },
                new LocationInfo()
                {
                    LocationNumber = 352,
                    Name = new("Hyrule Castle Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 353,
                    Name = new("Boomerang Chest", new("Hyrule Castle Boomerang Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 354,
                    Name = new("Zelda's Cell", new("Hyrule Castle Zelda's Cell", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 355,
                    Name = new("Link's Uncle"),
                },
                new LocationInfo()
                {
                    LocationNumber = 356,
                    Name = new("Secret Passage", new("Hyrule Castle Secret Passage", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 357,
                    Name = new("Castle Tower Foyer"),
                },
                new LocationInfo()
                {
                    LocationNumber = 358,
                    Name = new("Castle Tower (Dark Maze)"),
                },
                new LocationInfo()
                {
                    LocationNumber = 359,
                    Name = new("Cannonball Chest", new("Eastern Palace Cannonball Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 360,
                    Name = new("Eastern Palace Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 361,
                    Name = new("Eastern Palace Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 362,
                    Name = new("Eastern Palace Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 363,
                    Name = new("Eastern Palace Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 364,
                    Name = new("Armos Knights"),
                    Hints = new("One of the dungeon bosses has it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 365,
                    Name = new("Desert Palace Big Chest", "Dessert Palace Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 366,
                    Name = new("Torch", new("Desert Palace Torch", 0), new("Dessert Palace Torch", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 367,
                    Name = new("Desert Palace Map Chest", "Dessert Palace Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 368,
                    Name = new("Desert Palace Big Key Chest", "Dessert Palace Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 369,
                    Name = new("Desert Palace Compass Chest", "Dessert Palace Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 370,
                    Name = new("Lanmolas"),
                    Hints = new("One of the dungeon bosses has it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 371,
                    Name = new("Basement Cage", new("Tower of Hera Basement Cage", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 372,
                    Name = new("Tower of Hera Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 373,
                    Name = new("Tower of Hera Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 374,
                    Name = new("Tower of Hera Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 375,
                    Name = new("Tower of Hera Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 376,
                    Name = new("Moldorm"),
                    Hints = new("One of the dungeon bosses has it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 377,
                    Name = new("Shooter Room", new("Palace of Darkness Shooter Room", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 378,
                    Name = new("Palace of Darkness Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 379,
                    Name = new("Stalfos Basement", new("Palace of Darkness Stalfos Basement", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 380,
                    Name = new("The Arena (Bridge)", new("Palace of Darkness The Arena (Bridge)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 381,
                    Name = new("The Arena (Ledge)", new("Palace of Darkness The Arena (Ledge)", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 382,
                    Name = new("Palace of Darkness Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 383,
                    Name = new("Palace of Darkness Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 384,
                    Name = new("Harmless Hellway", new("Palace of Darkness Harmless Hellway", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 385,
                    Name = new("Dark Basement Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 386,
                    Name = new("Dark Basement Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 387,
                    Name = new("Dark Maze Top"),
                },
                new LocationInfo()
                {
                    LocationNumber = 388,
                    Name = new("Dark Maze Bottom"),
                },
                new LocationInfo()
                {
                    LocationNumber = 389,
                    Name = new("Palace of Darkness Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 390,
                    Name = new("Helmasaur King"),
                    Hints = new("One of the dungeon bosses has it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 391,
                    Name = new("Swamp Palace Entrance"),
                },
                new LocationInfo()
                {
                    LocationNumber = 392,
                    Name = new("Swamp Palace Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 393,
                    Name = new("Swamp Palace Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 394,
                    Name = new("Swamp Palace Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 395,
                    Name = new("Swamp Palace West Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 396,
                    Name = new("Swamp Palace Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 397,
                    Name = new("Flooded Room Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 398,
                    Name = new("Flooded Room Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 399,
                    Name = new("Waterfall Room", new("Swamp Palace Waterfall Room", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 400,
                    Name = new("Arrghus"),
                    Hints = new("One of the dungeon bosses has it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 401,
                    Name = new("Pot Prison", new("Skull Woods Pot Prison", 0), new("Skill Woods Pot Prison", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 402,
                    Name = new("Skull Woods Compass Chest", "Skill Woods Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 403,
                    Name = new("Skull Woods Big Chest", "Skill Woods Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 404,
                    Name = new("Skull Woods Map Chest", "Skill Woods Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 405,
                    Name = new("Pinball Room", "the room with all the bumpers and gib-does", new("Skull Woods Pinball Room", 0), new("Skill Woods Pinball Room", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 406,
                    Name = new("Skull Woods Big Key Chest", "Skill Woods Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 407,
                    Name = new("Bridge Room", new("Skull Woods Bridge Room", 0), new("Skill Woods Bridge Room", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 408,
                    Name = new("Mothula"),
                    Hints = new("One of the dungeon bosses has it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 409,
                    Name = new("Thieves' Town Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 410,
                    Name = new("Ambush Chest", new("Thieves' Town Ambush Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 411,
                    Name = new("Thieves' Town Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 412,
                    Name = new("Thieves' Town Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 413,
                    Name = new("Attic", new("Thieves' Town Attic", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 414,
                    Name = new("Blind's Cell", new("Thieves' Town Blind's Cell", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 415,
                    Name = new("Thieves' Town Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 416,
                    Name = new("Blind"),
                    Hints = new("One of the dungeon bosses has it.", "It will explain everything."),
                },
                new LocationInfo()
                {
                    LocationNumber = 417,
                    Name = new("Ice Palace Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 418,
                    Name = new("Spike Room", new("Ice Palace Spike Room", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 419,
                    Name = new("Ice Palace Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 420,
                    Name = new("Ice Palace Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 421,
                    Name = new("Iced T Room", new("Ice Palace Iced T Room", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 422,
                    Name = new("Freezor Chest", new("Ice Palace Freezor Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 423,
                    Name = new("Ice Palace Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 424,
                    Name = new("Kholdstare"),
                    Hints = new("One of the dungeon bosses has it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 425,
                    Name = new("Misery Mire Main Lobby"),
                },
                new LocationInfo()
                {
                    LocationNumber = 426,
                    Name = new("Misery Mire Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 427,
                    Name = new("Bridge Chest", new("Misery Mire Bridge Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 428,
                    Name = new("Spike Chest", new("Misery Mire Spike Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 429,
                    Name = new("Misery Mire Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 430,
                    Name = new("Misery Mire Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 431,
                    Name = new("Misery Mire Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 432,
                    Name = new("Vitreous"),
                    Hints = new("One of the dungeon bosses has it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 433,
                    Name = new("Turtle Rock Compass Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 434,
                    Name = new("Roller Room Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 435,
                    Name = new("Roller Room Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 436,
                    Name = new("Chain Chomps", new("Turtle Rock Chain Chomps", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 437,
                    Name = new("Turtle Rock Big Key Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 438,
                    Name = new("Turtle Rock Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 439,
                    Name = new("Crystaroller Room", new("Turtle Rock Crystaroller Room", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 440,
                    Name = new("Laser Bridge Top Right", "Eye Bridge Top Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 441,
                    Name = new("Laser Bridge Top Left", "Eye Bridge Top Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 442,
                    Name = new("Laser Bridge Bottom Right", "Eye Bridge Bottom Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 443,
                    Name = new("Laser Bridge Bottom Left", "Eye Bridge Bottom Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 444,
                    Name = new("Trinexx"),
                    Hints = new("One of the dungeon bosses has it."),
                },
                new LocationInfo()
                {
                    LocationNumber = 445,
                    Name = new("Bob's Torch"),
                },
                new LocationInfo()
                {
                    LocationNumber = 446,
                    Name = new("DMs Room Top Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 447,
                    Name = new("DMs Room Top Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 448,
                    Name = new("DMs Room Bottom Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 449,
                    Name = new("DMs Room Bottom Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 450,
                    Name = new("Ganon's Tower Map Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 451,
                    Name = new("Firesnake Room"),
                },
                new LocationInfo()
                {
                    LocationNumber = 452,
                    Name = new("Randomizer Room Top Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 453,
                    Name = new("Randomizer Room Top Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 454,
                    Name = new("Randomizer Room Bottom Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 455,
                    Name = new("Randomizer Room Bottom Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 456,
                    Name = new("Right Side First Room Left", "Hope Room Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 457,
                    Name = new("Right Side First Room Right", "Hope Room Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 458,
                    Name = new("Tile Room", new("Ganon's Tower Tile Room", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 459,
                    Name = new("Compass Room Top Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 460,
                    Name = new("Compass Room Top Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 461,
                    Name = new("Compass Room Bottom Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 462,
                    Name = new("Compass Room Bottom Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 463,
                    Name = new("Bob's Chest", new("Ganon's Tower Bob's Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 464,
                    Name = new("Ganon's Tower Big Chest"),
                },
                new LocationInfo()
                {
                    LocationNumber = 465,
                    Name = new("Bottom Big Key Chest", new("Big Key Room Bottom Big Key Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 466,
                    Name = new("Big Key Room Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 467,
                    Name = new("Big Key Room Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 468,
                    Name = new("Mini Helmasaur Room Left"),
                },
                new LocationInfo()
                {
                    LocationNumber = 469,
                    Name = new("Mini Helmasaur Room Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 470,
                    Name = new("Pre-Moldorm Chest", new("Ganon's Tower Pre-Moldorm Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 471,
                    Name = new("Moldorm Chest", new("Ganon's Tower Moldorm Chest", 0)),
                },
                new LocationInfo()
                {
                    LocationNumber = 496,
                    Name = new("Library"),
                },
                new LocationInfo()
                {
                    LocationNumber = 497,
                    Name = new("Forest Clearing", "Digging Spot", "Flute Spot"),
                },
                new LocationInfo()
                {
                    LocationNumber = 498,
                    Name = new("Cave #45", "Cave 45", "South of Grove"),
                },
                new LocationInfo()
                {
                    LocationNumber = 499,
                    Name = new("Link's House"),
                },
                new LocationInfo()
                {
                    LocationNumber = 500,
                    Name = new(new("Aginah's Cave", 0), "Aggina's Cave"),
                },
                new LocationInfo()
                {
                    LocationNumber = 506,
                    Name = new("Chicken House", "Chicken Lady's House"),
                },
                new LocationInfo()
                {
                    LocationNumber = 507,
                    Name = new("Mini Moldorm Cave Far Right"),
                },
                new LocationInfo()
                {
                    LocationNumber = 508,
                    Name = new("Desert Ledge"),
                },
                new LocationInfo()
                {
                    LocationNumber = 509,
                    Name = new("Checkerboard Cave"),
                },
                new LocationInfo()
                {
                    LocationNumber = 510,
                    Name = new("Waterfall Fairy Left"),
                },
            };
        }
    }


}
