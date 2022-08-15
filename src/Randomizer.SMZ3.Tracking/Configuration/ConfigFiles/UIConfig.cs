using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigFiles
{
    /// <summary>
    /// Represents a config file that houses different layouts for the Tracker UI
    /// </summary>
    public class UIConfig : List<UILayout>, IConfigFile<UIConfig>, IMergeable<UILayout>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UIConfig() { }

        /// <summary>
        /// Builds the default Tracker UI layouts
        /// </summary>
        /// <returns>A collection of layouts</returns>
        public static UIConfig Default()
        {
            return new UIConfig
            {
                new UILayout("Minimal", new List<UIGridLocation>()
                {
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.ItemStack,
                        Row = 1,
                        Column = 1,
                        Identifiers = new List<string>() { "Bow", "Silver Arrows" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.ItemStack,
                        Row = 1,
                        Column = 2,
                        Identifiers = new List<string>() { "Blue Boomerang", "Red Boomerang" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 1,
                        Column = 3,
                        Identifiers = new List<string>() { "Hookshot" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.ItemStack,
                        Row = 1,
                        Column = 4,
                        Identifiers = new List<string>() { "Magic Powder", "Mushroom" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 1,
                        Column = 5,
                        Identifiers = new List<string>() { "Charge Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 1,
                        Column = 6,
                        Identifiers = new List<string>() { "Ice Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 1,
                        Column = 7,
                        Identifiers = new List<string>() { "Wave Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 1,
                        Column = 8,
                        Identifiers = new List<string>() { "Spazer" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 1,
                        Column = 9,
                        Identifiers = new List<string>() { "Plasma Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 1,
                        Column = 10,
                        Identifiers = new List<string>() { "Screw Attack" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 1,
                        Identifiers = new List<string>() { "Fire Rod" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 2,
                        Identifiers = new List<string>() { "Ice Rod" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 3,
                        Identifiers = new List<string>() { "Bombos" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 4,
                        Identifiers = new List<string>() { "Ether" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 5,
                        Identifiers = new List<string>() { "Quake" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 6,
                        Identifiers = new List<string>() { "Shovel" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 7,
                        Identifiers = new List<string>() { "Morph Ball" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 8,
                        Identifiers = new List<string>() { "Morph Bombs" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 9,
                        Identifiers = new List<string>() { "Spring Ball" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 2,
                        Column = 10,
                        Identifiers = new List<string>() { "Space Jump" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 3,
                        Column = 1,
                        Identifiers = new List<string>() { "Lamp" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 3,
                        Column = 2,
                        Identifiers = new List<string>() { "Hammer" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.ItemStack,
                        Row = 3,
                        Column = 3,
                        Identifiers = new List<string>() { "Flute", "Duck" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 3,
                        Column = 4,
                        Identifiers = new List<string>() { "Bug Catching Net" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 3,
                        Column = 5,
                        Identifiers = new List<string>() { "Book of Mudora" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 3,
                        Column = 6,
                        Identifiers = new List<string>() { "Moon Pearl" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 3,
                        Column = 7,
                        Identifiers = new List<string>() { "Varia Suit" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 3,
                        Column = 8,
                        Identifiers = new List<string>() { "Gravity Suit" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 3,
                        Column = 9,
                        Identifiers = new List<string>() { "Hi-Jump Boots" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 3,
                        Column = 10,
                        Identifiers = new List<string>() { "Speed Booster" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 1,
                        Identifiers = new List<string>() { "Bottle" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 2,
                        Identifiers = new List<string>() { "Cane of Somaria" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 3,
                        Identifiers = new List<string>() { "Cane of Byrna" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 4,
                        Identifiers = new List<string>() { "Magic Cape" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 5,
                        Identifiers = new List<string>() { "Magic Mirror" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 6,
                        Identifiers = new List<string>() { "Heart Container" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 7,
                        Identifiers = new List<string>() { "Grapple Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 8,
                        Identifiers = new List<string>() { "X-Ray Scope" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 9,
                        Identifiers = new List<string>() { "Energy Tank" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 4,
                        Column = 10,
                        Identifiers = new List<string>() { "Reserve Tank" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 1,
                        Identifiers = new List<string>() { "Gloves" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 2,
                        Identifiers = new List<string>() { "Pegasus Boots" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 3,
                        Identifiers = new List<string>() { "Flippers" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 4,
                        Identifiers = new List<string>() { "Half Magic" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 5,
                        Identifiers = new List<string>() { "Sword" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 6,
                        Identifiers = new List<string>() { "Shield" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 7,
                        Identifiers = new List<string>() { "Mail" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 8,
                        Identifiers = new List<string>() { "Missile" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 9,
                        Identifiers = new List<string>() { "Super Missile" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 5,
                        Column = 10,
                        Identifiers = new List<string>() { "Power Bomb" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 1,
                        Identifiers = new List<string>() { "Eastern Palace" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 2,
                        Identifiers = new List<string>() { "Desert Palace" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 3,
                        Identifiers = new List<string>() { "Tower of Hera" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 4,
                        Identifiers = new List<string>() { "Palace of Darkness" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 5,
                        Identifiers = new List<string>() { "Swamp Palace" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 6,
                        Identifiers = new List<string>() { "Hyrule Castle" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 7,
                        Identifiers = new List<string>() { "Agahnim's Tower" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 6,
                        Column = 8,
                        Identifiers = new List<string>() { "Kraid" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 6,
                        Column = 9,
                        Identifiers = new List<string>() { "Phantoon" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 6,
                        Column = 10,
                        Identifiers = new List<string>() { "Content" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 7,
                        Column = 1,
                        Identifiers = new List<string>() { "Skull Woods" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 7,
                        Column = 2,
                        Identifiers = new List<string>() { "Thieves' Town" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 7,
                        Column = 3,
                        Identifiers = new List<string>() { "Ice Palace" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 7,
                        Column = 4,
                        Identifiers = new List<string>() { "Misery Mire" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 7,
                        Column = 5,
                        Identifiers = new List<string>() { "Turtle Rock" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 7,
                        Column = 6,
                        Identifiers = new List<string>() { "Ganon's Tower" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 7,
                        Column = 7,
                        Identifiers = new List<string>() { "Mother Brain" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 7,
                        Column = 8,
                        Identifiers = new List<string>() { "Draygon" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 7,
                        Column = 9,
                        Identifiers = new List<string>() { "Ridley" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Item,
                        Row = 7,
                        Column = 10,
                        Identifiers = new List<string>() { "Death" }
                    },
                })
            };
        }
    }
}
