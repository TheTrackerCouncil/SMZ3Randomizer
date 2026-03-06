using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupGroupBox(DynamicFormLayout.Vertical, "Game Settings")]
[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Game Settings Top", parentGroup: "Game Settings")]
[DynamicFormGroupBasic(DynamicFormLayout.TwoColumns, "Game Settings Bottom", parentGroup: "Game Settings")]
[DynamicFormGroupGroupBox(DynamicFormLayout.TwoColumns, "Race Settings", visibleWhenTrue: nameof(IsSingleplayer))]
public class GenerationWindowGameSettingsViewModel : ViewModelBase
{
    public const string StaticRewardsText = "No";
    public const string RandomizedRewardsText = "Yes";

    [DynamicFormFieldComboBox(label: "Keysanity:", groupName: "Game Settings Top")]
    public KeysanityMode KeysanityMode
    {
        get;
        set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(IsMetroidKeysanity));
            OnPropertyChanged(nameof(IsZeldaKeysanity));
        }
    }

    [DynamicFormFieldCheckBox("Require Crateria Boss Keycard for Tourian", groupName: "Game Settings Top", visibleWhenTrue: nameof(IsMetroidKeysanity))]
    public bool RequireBossKeycardForTourian { get; set; }

    [DynamicFormFieldCheckBox("Place GT Big Key in Ganon's Tower", groupName: "Game Settings Top", visibleWhenTrue: nameof(IsZeldaKeysanity))]
    public bool PlaceGTBigKeyInGT { get; set; }

    [DynamicFormFieldComboBox(comboBoxOptionsProperty: nameof(CrystalBossCountOptions), label: "Randomize needed crystal/boss counts:", groupName: "Game Settings Top")]
    public string CrystalBossCountStyle
    {
        get;
        set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(StaticCrystalBossCounts));
            OnPropertyChanged(nameof(RandomizedCrystalBossCounts));
        }
    } = "";

    [DynamicFormFieldCheckBox("Ganon/GT/Tourian requirements hidden", groupName: "Game Settings Top", visibleWhenTrue: nameof(AlwaysHidden))]
    public bool HideCrystalBossCount { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Crystals needed for GT:", groupName: "Game Settings Top", visibleWhenTrue: nameof(StaticCrystalBossCounts))]
    public int CrystalsNeededForGT { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Minimum crystals needed for GT:", groupName: "Game Settings Top", visibleWhenTrue: nameof(RandomizedCrystalBossCounts))]
    public int CrystalsNeededForGTMin
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value > CrystalsNeededForGTMax)
            {
                CrystalsNeededForGTMax = value;
            }
        }
    }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Maximum crystals needed for GT:", groupName: "Game Settings Top", visibleWhenTrue: nameof(RandomizedCrystalBossCounts))]
    public int CrystalsNeededForGTMax
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value < CrystalsNeededForGTMin)
            {
                CrystalsNeededForGTMin = value;
            }
        }
    }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Crystals needed for Ganon:", groupName: "Game Settings Top", visibleWhenTrue: nameof(StaticCrystalBossCounts))]
    public int CrystalsNeededForGanon { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Minimum crystals needed for Ganon:", groupName: "Game Settings Top", visibleWhenTrue: nameof(RandomizedCrystalBossCounts))]
    public int CrystalsNeededForGanonMin
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value > CrystalsNeededForGanonMax)
            {
                CrystalsNeededForGanonMax = value;
            }
        }
    }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Maximum crystals needed for Ganon:", groupName: "Game Settings Top", visibleWhenTrue: nameof(RandomizedCrystalBossCounts))]
    public int CrystalsNeededForGanonMax
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value < CrystalsNeededForGanonMin)
            {
                CrystalsNeededForGanonMin = value;
            }
        }
    }

    [DynamicFormFieldSlider(0, 4, 0, 1, label: "Bosses needed for Tourian:", groupName: "Game Settings Top", visibleWhenTrue: nameof(StaticCrystalBossCounts))]
    public int BossesNeededForTourian { get; set; }

    [DynamicFormFieldSlider(0, 4, 0, 1, label: "Minimum bosses needed for Tourian:", groupName: "Game Settings Top", visibleWhenTrue: nameof(RandomizedCrystalBossCounts))]
    public int BossesNeededForTourianMin
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value > BossesNeededForTourianMax)
            {
                BossesNeededForTourianMax = value;
            }
        }
    }

    [DynamicFormFieldSlider(0, 4, 0, 1, label: "Maximum bosses needed for Tourian:", groupName: "Game Settings Top", visibleWhenTrue: nameof(RandomizedCrystalBossCounts))]
    public int BossesNeededForTourianMax
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value < BossesNeededForTourianMin)
            {
                BossesNeededForTourianMin = value;
            }
        }
    }

    [DynamicFormFieldCheckBox("Open Ganon's pyramid by default", groupName: "Game Settings Bottom")]
    public bool OpenPyramid { get; set; }

    [DynamicFormFieldCheckBox("Generate race seed", groupName: "Race Settings")]
    public bool Race { get; set; }

    [DynamicFormFieldCheckBox("Disable spoiler log", groupName: "Race Settings")]
    public bool DisableSpoilerLog { get; set; }

    [DynamicFormFieldCheckBox("Disable tracker hints", groupName: "Race Settings")]
    public bool DisableTrackerHints { get; set; }

    [DynamicFormFieldCheckBox("Disable tracker spoilers", groupName: "Race Settings")]
    public bool DisableTrackerSpoilers { get; set; }

    [DynamicFormFieldCheckBox("Disable cheats", groupName: "Race Settings")]
    public bool DisableCheats { get; set; }

    public bool IsMetroidKeysanity => KeysanityMode is KeysanityMode.Both or KeysanityMode.SuperMetroid;

    public bool IsZeldaKeysanity => KeysanityMode is KeysanityMode.Both or KeysanityMode.Zelda;
    public bool AlwaysHidden => false;

    public bool StaticCrystalBossCounts => CrystalBossCountStyle == StaticRewardsText;
    public bool RandomizedCrystalBossCounts => CrystalBossCountStyle == RandomizedRewardsText;
    public string[] CrystalBossCountOptions => [StaticRewardsText, RandomizedRewardsText];

    public bool IsMultiplayer
    {
        get;
        set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(IsSingleplayer));
        }
    }

    public bool IsSingleplayer => !IsMultiplayer;
}
