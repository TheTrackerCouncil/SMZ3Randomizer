using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared
{
    public class LogicConfig
    {
        [DisplayName("Prevent Screw Attack Sock Locks"), Description("Requires the morph ball before using screw attacks in locations where you can get stuck"), Category("Logic")]
        public bool PreventScrewAttackSoftLock { get; set; }

        [DisplayName("Require Ten Power Bombs"), Description("Always require at least two power bomb upgrades for navigation in Super Metroid. Will require tracking multiple power bombs."), Category("Logic")]
        public bool RequireTwoPowerBombs { get; set; }

        [DisplayName("Fire Rod for Dark Rooms"), Description("Having the fire rod will open up the back of Hyrule Castle, Eastern Palace Armos Knights, and select rooms in Palace of Darkness"), Category("Tricks")]
        public bool FireRodDarkRooms { get; set; }

        [DisplayName("Infinite Bomb Jump"), Description("Allow using morph bombs to access high locations with specially timed morph bombs"), Category("Tricks")]
        public bool InfiniteBombJump { get; set; }

        [DisplayName("Parlor Speed Booster Break In"), Description("Allow using the speed booster to burst through to the Terminator Room/Brinstar without morph bombs or screw attack"), Category("Tricks")]
        public bool ParlorSpeedBooster { get; set; }

        public LogicConfig Clone()
        {
            return (LogicConfig)MemberwiseClone();
        }
    }
}
