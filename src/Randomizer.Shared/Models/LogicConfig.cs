using System.ComponentModel;

namespace Randomizer.Shared
{
    public class LogicConfig
    {
        [DisplayName("Prevent Screw Attack Soft Locks"), Description("You're not expected to use Screw Attack without the morph ball"), Category("Logic")]
        public bool PreventScrewAttackSoftLock { get; set; }

        [DisplayName("Prevent Five Power Bomb Seeds"), Description("You're expected to have at least two power bomb upgrades for navigation in Super Metroid. Will require tracking multiple power bombs."), Category("Logic")]
        public bool PreventFivePowerBombSeed { get; set; }

        [DisplayName("Fire Rod for Dark Rooms"), Description("You're expected to be able to use the fire rod to light torches for navigating Hyrule Castle escape, Eastern Palace Armos Knights, and select rooms in Palace of Darkness"), Category("Tricks")]
        public bool FireRodDarkRooms { get; set; }

        [DisplayName("Infinite Bomb Jump"), Description("You're expected to be able to use specially timed morph bombs to access high locations"), Category("Tricks")]
        public bool InfiniteBombJump { get; set; }

        [DisplayName("Parlor Speed Booster Break In"), Description("You're expected to be able to use the speed booster to burst through to the Terminator Room/Brinstar without morph/power bombs or screw attack"), Category("Tricks")]
        public bool ParlorSpeedBooster { get; set; }

        public LogicConfig Clone()
        {
            return (LogicConfig)MemberwiseClone();
        }
    }
}
