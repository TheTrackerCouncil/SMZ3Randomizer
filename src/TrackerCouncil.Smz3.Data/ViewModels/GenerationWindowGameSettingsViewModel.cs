using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupGroupBox(DynamicFormLayout.Vertical, "Goal")]
[DynamicFormGroupGroupBox(DynamicFormLayout.Vertical, "Additional Settings")]
[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Additional Settings Top", parentGroup: "Additional Settings")]
[DynamicFormGroupBasic(DynamicFormLayout.TwoColumns, "Additional Settings Bottom", parentGroup: "Additional Settings")]
[DynamicFormGroupGroupBox(DynamicFormLayout.TwoColumns, "Race Settings", visibleWhenTrue: nameof(IsSingleplayer))]
public class GenerationWindowGameSettingsViewModel : ViewModelBase
{
    [DynamicFormObject(groupName: "Goal to defeat Ganon/Mother Brain:")]
    public GenerationWindowGoalSettingsViewModel GoalSettings { get; set; } = new();

    [DynamicFormFieldComboBox(label: "Keysanity:", groupName: "Additional Settings Top")]
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

    [DynamicFormFieldCheckBox("Require Crateria Boss Keycard for Tourian", groupName: "Additional Settings Top", visibleWhenTrue: nameof(IsMetroidKeysanity))]
    public bool RequireBossKeycardForTourian { get; set; }

    [DynamicFormFieldCheckBox("Place GT Big Key in Ganon's Tower", groupName: "Additional Settings Top", visibleWhenTrue: nameof(IsZeldaKeysanity))]
    public bool PlaceGTBigKeyInGT { get; set; }

    [DynamicFormFieldComboBox(label: "Shuffle boss tokens with dungeon rewards:", groupName: "Additional Settings Top")]
    public YesNoEnum InterGameRewards { get; set; }

    [DynamicFormFieldCheckBox("Open Ganon's pyramid by default", groupName: "Additional Settings Bottom")]
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
