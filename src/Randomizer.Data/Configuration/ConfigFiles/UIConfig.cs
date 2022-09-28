using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.Configuration.ConfigTypes;

namespace Randomizer.Data.Configuration.ConfigFiles
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
                #region Minimal UI
                new UILayout("Minimal", new List<UIGridLocation>()
                {
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 1,
                        Identifiers = new List<string>() { "Bow", "Silver Arrows" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 2,
                        Identifiers = new List<string>() { "Blue Boomerang", "Red Boomerang" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 3,
                        Identifiers = new List<string>() { "Hookshot" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 4,
                        Identifiers = new List<string>() { "Magic Powder", "Mushroom" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 5,
                        Identifiers = new List<string>() { "Charge Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 6,
                        Identifiers = new List<string>() { "Ice Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 7,
                        Identifiers = new List<string>() { "Wave Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 8,
                        Identifiers = new List<string>() { "Spazer" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 9,
                        Identifiers = new List<string>() { "Plasma Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 10,
                        Identifiers = new List<string>() { "Screw Attack" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 1,
                        Identifiers = new List<string>() { "Fire Rod" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 2,
                        Identifiers = new List<string>() { "Ice Rod" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 3,
                        Identifiers = new List<string>() { "Bombos" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 4,
                        Identifiers = new List<string>() { "Ether" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 5,
                        Identifiers = new List<string>() { "Quake" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 6,
                        Identifiers = new List<string>() { "Shovel" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 7,
                        Identifiers = new List<string>() { "Morph Ball" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 8,
                        Identifiers = new List<string>() { "Morph Bombs" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 9,
                        Identifiers = new List<string>() { "Spring Ball" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 10,
                        Identifiers = new List<string>() { "Space Jump" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 1,
                        Identifiers = new List<string>() { "Lamp" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 2,
                        Identifiers = new List<string>() { "Hammer" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 3,
                        Identifiers = new List<string>() { "Flute", "Duck" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 4,
                        Identifiers = new List<string>() { "Bug Catching Net" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 5,
                        Identifiers = new List<string>() { "Book of Mudora" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 6,
                        Identifiers = new List<string>() { "Moon Pearl" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 7,
                        Identifiers = new List<string>() { "Varia Suit" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 8,
                        Identifiers = new List<string>() { "Gravity Suit" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 9,
                        Identifiers = new List<string>() { "Hi-Jump Boots" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 10,
                        Identifiers = new List<string>() { "Speed Booster" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 1,
                        Identifiers = new List<string>() { "Bottle" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 2,
                        Identifiers = new List<string>() { "Cane of Somaria" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 3,
                        Identifiers = new List<string>() { "Cane of Byrna" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 4,
                        Identifiers = new List<string>() { "Magic Cape" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 5,
                        Identifiers = new List<string>() { "Magic Mirror" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 6,
                        Identifiers = new List<string>() { "Heart Container" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 7,
                        Identifiers = new List<string>() { "Grapple Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 8,
                        Identifiers = new List<string>() { "X-Ray Scope" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 9,
                        Identifiers = new List<string>() { "Energy Tank" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 10,
                        Identifiers = new List<string>() { "Reserve Tank" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 1,
                        Identifiers = new List<string>() { "Gloves" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 2,
                        Identifiers = new List<string>() { "Pegasus Boots" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 3,
                        Identifiers = new List<string>() { "Flippers" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 4,
                        Identifiers = new List<string>() { "Half Magic" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 5,
                        Identifiers = new List<string>() { "Sword" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 6,
                        Identifiers = new List<string>() { "Shield" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 7,
                        Identifiers = new List<string>() { "Mail" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 8,
                        Identifiers = new List<string>() { "Missile" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 9,
                        Identifiers = new List<string>() { "Super Missile" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
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
                        Identifiers = new List<string>() { "Castle Tower" }
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
                        Type = UIGridLocationType.Items,
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
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 10,
                        Identifiers = new List<string>() { "Death" }
                    },
                }),
                #endregion Minimal UI

                #region Keysanity UI
                new UILayout("Keysanity", new List<UIGridLocation>()
                {
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 1,
                        Identifiers = new List<string>() { "Bow", "Silver Arrows" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 2,
                        Identifiers = new List<string>() { "Blue Boomerang", "Red Boomerang" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 3,
                        Identifiers = new List<string>() { "Hookshot" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 4,
                        Identifiers = new List<string>() { "Magic Powder", "Mushroom" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 5,
                        Identifiers = new List<string>() { "Charge Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 6,
                        Identifiers = new List<string>() { "Ice Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 7,
                        Identifiers = new List<string>() { "Wave Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 8,
                        Identifiers = new List<string>() { "Spazer" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 9,
                        Identifiers = new List<string>() { "Plasma Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 10,
                        Identifiers = new List<string>() { "Screw Attack" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 1,
                        Identifiers = new List<string>() { "Fire Rod" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 2,
                        Identifiers = new List<string>() { "Ice Rod" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 3,
                        Identifiers = new List<string>() { "Bombos" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 4,
                        Identifiers = new List<string>() { "Ether" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 5,
                        Identifiers = new List<string>() { "Quake" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 6,
                        Identifiers = new List<string>() { "Shovel" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 7,
                        Identifiers = new List<string>() { "Morph Ball" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 8,
                        Identifiers = new List<string>() { "Morph Bombs" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 9,
                        Identifiers = new List<string>() { "Spring Ball" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 2,
                        Column = 10,
                        Identifiers = new List<string>() { "Space Jump" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 1,
                        Identifiers = new List<string>() { "Lamp" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 2,
                        Identifiers = new List<string>() { "Hammer" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 3,
                        Identifiers = new List<string>() { "Flute", "Duck" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 4,
                        Identifiers = new List<string>() { "Bug Catching Net" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 5,
                        Identifiers = new List<string>() { "Book of Mudora" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 6,
                        Identifiers = new List<string>() { "Moon Pearl" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 7,
                        Identifiers = new List<string>() { "Varia Suit" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 8,
                        Identifiers = new List<string>() { "Gravity Suit" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 9,
                        Identifiers = new List<string>() { "Hi-Jump Boots" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 10,
                        Identifiers = new List<string>() { "Speed Booster" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 1,
                        Identifiers = new List<string>() { "Bottle" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 2,
                        Identifiers = new List<string>() { "Cane of Somaria" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 3,
                        Identifiers = new List<string>() { "Cane of Byrna" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 4,
                        Identifiers = new List<string>() { "Magic Cape" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 5,
                        Identifiers = new List<string>() { "Magic Mirror" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 6,
                        Identifiers = new List<string>() { "Heart Container" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 7,
                        Identifiers = new List<string>() { "Grapple Beam" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 8,
                        Identifiers = new List<string>() { "X-Ray Scope" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 9,
                        Identifiers = new List<string>() { "Energy Tank" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 4,
                        Column = 10,
                        Identifiers = new List<string>() { "Reserve Tank" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 1,
                        Identifiers = new List<string>() { "Gloves" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 2,
                        Identifiers = new List<string>() { "Pegasus Boots" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 3,
                        Identifiers = new List<string>() { "Flippers" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 4,
                        Identifiers = new List<string>() { "Half Magic" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 5,
                        Identifiers = new List<string>() { "Sword" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 6,
                        Identifiers = new List<string>() { "Shield" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 7,
                        Identifiers = new List<string>() { "Mail" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 8,
                        Identifiers = new List<string>() { "Missile" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 9,
                        Identifiers = new List<string>() { "Super Missile" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
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
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 1,
                        Identifiers = new List<string>() { "Eastern Palace Big Key", "Eastern Palace Map" }
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
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 2,
                        Identifiers = new List<string>() { "Desert Palace Key", "Desert Palace Big Key", "Desert Palace Map" }
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
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 3,
                        Identifiers = new List<string>() { "Tower of Hera Key", "Tower of Hera Big Key", "Tower of Hera Map" }
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
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 4,
                        Identifiers = new List<string>() { "Palace of Darkness Key", "Palace of Darkness Big Key", "Palace of Darkness Map" }
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
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 5,
                        Identifiers = new List<string>() { "Swamp Palace Key", "Swamp Palace Big Key", "Swamp Palace Map" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 6,
                        Identifiers = new List<string>() { "Skull Woods" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 6,
                        Identifiers = new List<string>() { "Skull Woods Key", "Skull Woods Big Key", "Skull Woods Map" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 7,
                        Identifiers = new List<string>() { "Thieves' Town" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 7,
                        Identifiers = new List<string>() { "Thieves Town Key", "Thieves Town Big Key", "Thieves Town Map" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 8,
                        Identifiers = new List<string>() { "Ice Palace" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 8,
                        Identifiers = new List<string>() { "Ice Palace Key", "Ice Palace Big Key", "Ice Palace Map" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 9,
                        Identifiers = new List<string>() { "Misery Mire" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 9,
                        Identifiers = new List<string>() { "Misery Mire Key", "Misery Mire Big Key", "Misery Mire Map" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 10,
                        Identifiers = new List<string>() { "Turtle Rock" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 10,
                        Identifiers = new List<string>() { "Turtle Rock Key", "Turtle Rock Big Key", "Turtle Rock Map" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 11,
                        Identifiers = new List<string>() { "Hyrule Castle" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 11,
                        Identifiers = new List<string>() { "Sewer Key", "Hyrule Castle Map" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 12,
                        Identifiers = new List<string>() { "Castle Tower" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 12,
                        Identifiers = new List<string>() { "Castle Tower Key" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Dungeon,
                        Row = 6,
                        Column = 13,
                        Identifiers = new List<string>() { "Ganon's Tower" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 7,
                        Column = 13,
                        Identifiers = new List<string>() { "Ganons Tower Key", "Ganons Tower Big Key", "Ganons Tower Map" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 11,
                        Image = "CR.png",
                        Identifiers = new List<string>() { "Crateria Level 1 Keycard", "Crateria Level 2 Keycard", "Crateria Boss Keycard" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 2,
                        Column = 11,
                        Identifiers = new List<string>() { "Bomb Torizo" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 12,
                        Image = "BR.png",
                        Identifiers = new List<string>() { "Brinstar Level 1 Keycard", "Brinstar Level 2 Keycard", "Brinstar Boss Keycard" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 2,
                        Column = 12,
                        Identifiers = new List<string>() { "Kraid" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 1,
                        Column = 13,
                        Image = "WS.png",
                        Identifiers = new List<string>() { "Wrecked Ship Level 1 Keycard", "Wrecked Ship Boss Keycard" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 2,
                        Column = 13,
                        Identifiers = new List<string>() { "Phantoon" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 11,
                        Image = "MA.png",
                        Identifiers = new List<string>() { "Maridia Level 1 Keycard", "Maridia Level 2 Keycard", "Maridia Boss Keycard" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 4,
                        Column = 11,
                        Identifiers = new List<string>() { "Draygon" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 12,
                        Image = "UN.png",
                        Identifiers = new List<string>() { "Norfair Level 1 Keycard", "Norfair Level 2 Keycard", "Norfair Boss Keycard" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 4,
                        Column = 12,
                        Identifiers = new List<string>() { "Crocomire" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 3,
                        Column = 13,
                        Image = "LN.png",
                        Identifiers = new List<string>() { "Lower Norfair Level 1 Keycard", "Lower Norfair Boss Keycard" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 4,
                        Column = 13,
                        Identifiers = new List<string>() { "Ridley" }
                    },

                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 11,
                        Identifiers = new List<string>() { "Content" }
                    },

                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Items,
                        Row = 5,
                        Column = 12,
                        Identifiers = new List<string>() { "Death" }
                    },

                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.SMBoss,
                        Row = 5,
                        Column = 13,
                        Identifiers = new List<string>() { "Mother Brain" }
                    },

                }),
                #endregion Keysanity UI

                #region Peg World
                new UILayout("Peg World", new List<UIGridLocation>()
                {
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 1,
                        Column = 2,
                        Identifiers = new List<string>() { "1" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 1,
                        Column = 3,
                        Identifiers = new List<string>() { "2" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 1,
                        Column = 4,
                        Identifiers = new List<string>() { "3" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 1,
                        Column = 5,
                        Identifiers = new List<string>() { "4" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 1,
                        Column = 6,
                        Identifiers = new List<string>() { "5" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 2,
                        Column = 2,
                        Identifiers = new List<string>() { "6" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 2,
                        Column = 3,
                        Identifiers = new List<string>() { "7" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 2,
                        Column = 4,
                        Identifiers = new List<string>() { "8" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 2,
                        Column = 5,
                        Identifiers = new List<string>() { "9" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 2,
                        Column = 6,
                        Identifiers = new List<string>() { "10" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 3,
                        Column = 1,
                        Identifiers = new List<string>() { "11" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 3,
                        Column = 2,
                        Identifiers = new List<string>() { "12" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 3,
                        Column = 3,
                        Identifiers = new List<string>() { "13" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 3,
                        Column = 4,
                        Identifiers = new List<string>() { "14" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 3,
                        Column = 5,
                        Identifiers = new List<string>() { "15" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 3,
                        Column = 6,
                        Identifiers = new List<string>() { "16" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 4,
                        Column = 1,
                        Identifiers = new List<string>() { "17" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 4,
                        Column = 2,
                        Identifiers = new List<string>() { "18" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 4,
                        Column = 3,
                        Identifiers = new List<string>() { "19" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 5,
                        Column = 1,
                        Identifiers = new List<string>() { "20" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 5,
                        Column = 2,
                        Identifiers = new List<string>() { "21" }
                    },
                    new UIGridLocation()
                    {
                        Type = UIGridLocationType.Peg,
                        Row = 5,
                        Column = 3,
                        Identifiers = new List<string>() { "22" }
                    }
                })
                #endregion
            };
        }
    }
}
