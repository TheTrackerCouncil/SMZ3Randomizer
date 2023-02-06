using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Data.Options
{
    public class CasPatches
    {
        [DisplayName("Disable flashing")]
        [Description("Disables many flashing effects from things like Ether, lightning, and Metroid bosses. Note some flashing may still exist unfortunately.\nMetroid patch developed by Kara.")]
        public bool DisableFlashing { get; set; } = true;

        [DisplayName("Disable screen shake (Metroid)")]
        [Description("Disables the screen shake effect during escape and when lava/acid is rising.\nPatch developed by flo.")]
        public bool DisableScreenShake { get; set; } = true;

        [DisplayName("Prevent Scams")]
        [Description("The bottle merchant and King Zora will tell you what they have before asking if you want to purchase it, even if you don't have enough rupees.")]
        public bool PreventScams { get; set; } = true;

        [DisplayName("Randomized Bottles")]
        [Description("Adds randomly filled bottles instead of empty bottles and random fairy bottle trades.")]
        public bool RandomizedBottles { get; set; } = true;

        [DisplayName("Quarter Magic")]
        [Description("Adds an additional progressive half magic to the item pool.")]
        public bool QuarterMagic { get; set; } = true;

        [DisplayName("Aim with Any Button")]
        [Description("Allows you to map the aiming button to any button.\nPatch developed by Kejardon.")]
        public bool AimAnyButton { get; set; } = true;

        [DisplayName("Infinite Space Jump")]
        [Description("Updates to make the space jump timing easier.\nPatch developed by MetConst.")]
        public bool InfiniteSpaceJump { get; set; } = true;

        [DisplayName("Spin Jump Restart")]
        [Description("Allows you to initiate a spinning jump in the middle of falling normally.\nPatch developed by Kejardon.")]
        public bool Respin { get; set; } = true;

        [DisplayName("Conserve Metroid Momentum")]
        [Description("Preserve your horizontal movement when landing from a fall or jump.\nPatch developed by Oi27.")]
        public bool Speedkeep { get; set; } = true;

        [DisplayName("Starting Nerfed Charge Beam")]
        [Description("You will always start with a weak charge beam to prevent soft locks against bosses when running out of missiles.\nPatch developed by Smiley and Flo.")]
        public bool NerfedCharge { get; set; } = true;

        [DisplayName("No Bomb Torizo Softlocks")]
        [Description("Closes the door to Bomb Torizo faster after picking up the item to prevent softlocks by getting stuck in the door.")]
        public bool NoBozoSoftlock { get; set; } = true;

        [DisplayName("Fast Metroid Doors")]
        [Description("Speeds up the transition between Metroid rooms via doors.\nPatch developed by Rakki.")]
        public bool FastDoors { get; set; } = true;

        [DisplayName("Fast Metroid Elevators")]
        [Description("Speeds up the transition between Metroid areas via elevators.\nPatch developed by Lioran.")]
        public bool FastElevators { get; set; } = true;

        [DisplayName("Refill at Save Stations")]
        [Description("Save stations will refill your ammo.\nPatch developed by Adam.")]
        public bool RefillAtSaveStation { get; set; } = true;

        [DisplayName("Easier Wall Jumps")]
        [Description("Makes the timing of wall jumps more leniant.\nPatch developed by Benox50.")]
        public bool EasierWallJumps { get; set; } = true;

        [DisplayName("Snap Morph to Holes")]
        [Description("Makes it easier to get into morph ball holes and allows you to morph over some 1 tile wide pits.\nPatch developed by Benox50.")]
        public bool SnapMorph { get; set; } = true;

        public CasPatches Clone()
        {
            return (CasPatches)MemberwiseClone();
        }
    }
}
