using System.ComponentModel;
using Randomizer.Shared;

namespace Randomizer.Data.Logic
{
    public class LogicConfig
    {
        [DisplayName("Prevent Screw Attack Soft Locks")]
        [Description("You're not expected to use Screw Attack without the morph ball.")]
        [Category("Logic")]
        public bool PreventScrewAttackSoftLock { get; set; }

        [DisplayName("Prevent Five Power Bomb Seeds")]
        [Description("You're expected to have at least two power bomb upgrades for navigation in Super Metroid. Will require tracking multiple power bombs.")]
        [Category("Logic")]
        public bool PreventFivePowerBombSeed { get; set; }

        [DisplayName("Left Sand Pit Needs Spring Ball")]
        [Description("You're expected to have the spring ball to navigate the left sand pit in Maridia.")]
        [Category("Logic")]
        public bool LeftSandPitRequiresSpringBall { get; set; }

        [DisplayName("Launchpad Needs Ice Beam")]
        [Description("You're expected to have the ice beam to freeze the boyons to run over to get speed for a shine spark in the old Tourian launchpad in central Crateria.")]
        [Category("Logic")]
        public bool LaunchPadRequiresIceBeam { get; set; }

        [DisplayName("Waterway Needs Gravity Suit")]
        [Description("You're expected to have the gravity suit to be able to access the Waterway in Pink Brinstar.")]
        [Category("Logic")]
        public bool WaterwayNeedsGravitySuit { get; set; }

        [DisplayName("Easy East Crateria Sky Item")]
        [Description("You're expected to have the space jump or speed booster to get the item in the sky in East Crateria after Wrecked Ship.")]
        [Category("Logic")]
        public bool EasyEastCrateriaSkyItem { get; set; }

        [DisplayName("Fire Rod for Dark Rooms")]
        [Description("You're expected to be able to use the fire rod to light torches for navigating Hyrule Castle escape, Eastern Palace Armos Knights, and select rooms in Palace of Darkness.")]
        [Category("Tricks")]
        public bool FireRodDarkRooms { get; set; }

        [DisplayName("Infinite Bomb Jump")]
        [Description("You're expected to be able to use specially timed morph bombs to access high locations.")]
        [Category("Tricks")]
        public bool InfiniteBombJump { get; set; }

        [DisplayName("Parlor Speed Booster Break In")]
        [Description("You're expected to be able to use the speed booster to burst through to the Terminator Room/Brinstar without morph/power bombs or screw attack.")]
        [Category("Tricks")]
        public bool ParlorSpeedBooster { get; set; }

        [DisplayName("Mockball")]
        [Description("You're expected to be able to use to mockball to avoid having the speed booster at the entrance to Green Brinstar and Upper Norfair West.")]
        [Category("Tricks")]
        public bool MockBall { get; set; }

        [DisplayName("Sword Only Dark Rooms")]
        [Description("Excluding Hyrule Castle Tower, you're expected to be able to pass through dark rooms without any light and using only your sword to navigate.")]
        [Category("Tricks")]
        public bool SwordOnlyDarkRooms { get; set; }

        [DisplayName("Light World South Fake Flippers")]
        [Description("You're expected to be able to use fake flippers in the Light World South near Lake Hylia to access under the bridge and, if you have the moon pearl, the waterfall fairy chests.")]
        [Category("Tricks")]
        public bool LightWorldSouthFakeFlippers { get; set; }

        [DisplayName("Wall Jump Difficulty")]
        [Description("The maximum difficulty of wall jumps you're comfortable with.")]
        [Category("Tricks")]
        [DefaultValue(WallJumpDifficulty.Medium)]
        public WallJumpDifficulty WallJumpDifficulty { get; set; }
            = WallJumpDifficulty.Medium; // Not very cas’, but this seems to be closest to previous assumptions

        public LogicConfig Clone()
        {
            return (LogicConfig)MemberwiseClone();
        }
    }
}
