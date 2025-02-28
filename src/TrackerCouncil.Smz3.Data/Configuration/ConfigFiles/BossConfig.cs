using System.Collections.Generic;
using System.ComponentModel;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Shared.Enums;
using static TrackerCouncil.Smz3.Data.Configuration.ConfigTypes.SchrodingersString;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

/// <summary>
/// Config file for additional boss information
/// </summary>
[Description("Config file for the various Metroid Bosses and other things that should be beatable")]
public class BossConfig : List<BossInfo>, IMergeable<BossInfo>, IConfigFile<BossConfig>
{
    /// <summary>
    /// Constructor
    /// </summary>
    public BossConfig() : base()
    {
    }

    /// <summary>
    /// Returns default boss information
    /// </summary>
    /// <returns></returns>
    public static BossConfig Default()
    {
        return
        [
            new BossInfo("Spore Spawn") { MemoryAddress = 1, MemoryFlag = 0x2, },
            new BossInfo("Botwoon") { MemoryAddress = 4, MemoryFlag = 0x2, },
            new BossInfo("Kraid") { Type = BossType.Kraid, MemoryAddress = 1, MemoryFlag = 0x1, },
            new BossInfo("Crocomire") { MemoryAddress = 2, MemoryFlag = 0x2, },
            new BossInfo("Phantoon") { Type = BossType.Phantoon, MemoryAddress = 3, MemoryFlag = 0x1, },
            new BossInfo("Shaktool"),
            new BossInfo("Draygon") { Type = BossType.Draygon, MemoryAddress = 4, MemoryFlag = 0x1, },
            new BossInfo("Ridley") { Type = BossType.Ridley, MemoryAddress = 2, MemoryFlag = 0x1, },
            new BossInfo("Mother Brain") { Type = BossType.MotherBrain },
            new BossInfo("Bomb Torizo") { MemoryAddress = 0, MemoryFlag = 0x4, },
            new BossInfo("Golden Torizo") { MemoryAddress = 2, MemoryFlag = 0x4, },
            new BossInfo("Castle Guard") { Type = BossType.CastleGuard },
            new BossInfo("Armos Knights") { Type = BossType.ArmosKnights },
            new BossInfo("Moldorm") { Type = BossType.Moldorm },
            new BossInfo("Lanmolas") { Type = BossType.Lanmolas },
            new BossInfo("Agahnim") { Type = BossType.Agahnim },
            new BossInfo("Helmasaur King") { Type = BossType.HelmasaurKing },
            new BossInfo("Arrghus") { Type = BossType.Arrghus },
            new BossInfo("Blind") { Type = BossType.Blind },
            new BossInfo("Mothula") { Type = BossType.Mothula },
            new BossInfo("Kholdstare") { Type = BossType.Kholdstare },
            new BossInfo("Vitreous") { Type = BossType.Vitreous },
            new BossInfo("Trinexx") { Type = BossType.Trinexx },
            new BossInfo("Ganon") { Type = BossType.Ganon },
        ];
    }

    public static object Example()
    {
        return new BossConfig
        {
            new BossInfo("Bomb Torizo")
            {
                Name = new("Bomb Torizo", "Bomb Chozo", new Possibility("Bozo", 0.1)),
                WhenTracked = new SchrodingersString("Message when clearing the boss", new Possibility("Another message when clearing the boss", 0.1)),
                WhenDefeated = new SchrodingersString("Message when defeating the boss", new Possibility("Another message when defeating the boss", 0.1)),
            },
        };
    }
}
