using Randomizer.Shared;
using static Randomizer.SMZ3.SMLogic;

namespace Randomizer.SMZ3
{
    static class ProgressionTrickExtensions {

        public static bool CanPerformTrick(this Progression items, TrickType trick) {

            
            if (!items.EnabledTricks.Contains(trick))
            {
                return false;
            }
            
            return trick switch
            {
                TrickType.SinglePowerBombForMorphBombs => items.PowerBomb,
                _ => false,
            };
        }

    }

}
