using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.Shared.Enums
{
    public static class RewardExtensions
    {
        public static RewardType ToRewardType(this RewardItem reward) => reward switch
        {
            RewardItem.Agahnim => RewardType.Agahnim,
            RewardItem.GreenPendant => RewardType.PendantGreen,
            RewardItem.NonGreenPendant => RewardType.PendantRed,
            RewardItem.RedPendant => RewardType.PendantRed,
            RewardItem.BluePendant => RewardType.PendantBlue,
            RewardItem.Crystal => RewardType.CrystalBlue,
            RewardItem.RedCrystal => RewardType.CrystalRed,
            _ => RewardType.None
        };

        public static RewardItem ToRewardItem(this RewardType reward) => reward switch
        {
            RewardType.CrystalRed => RewardItem.RedCrystal,
            RewardType.CrystalBlue => RewardItem.Crystal,
            RewardType.PendantGreen => RewardItem.GreenPendant,
            RewardType.PendantRed => RewardItem.RedPendant,
            RewardType.PendantBlue => RewardItem.BluePendant,
            _ => RewardItem.Unknown,
        };
    }
}
