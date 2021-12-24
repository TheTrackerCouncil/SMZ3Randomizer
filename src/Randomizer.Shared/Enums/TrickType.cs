using System.ComponentModel.DataAnnotations;

namespace Randomizer.Shared
{
    public enum TrickType
    {
        [TrickCategory(TrickCategory.Metroid, TrickCategory.DefaultEnabled)]
        [Display(Name = "Require Only One Power Bomb", Description = "Disabling this will require two power bombs before exploring with power bombs is considered in logic")]
        SinglePowerBombForMorphBombs
    }
}
