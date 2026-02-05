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
    private bool _isMultiplayer;
    private KeysanityMode _keysanityMode;

    [DynamicFormFieldComboBox(label: "Keysanity:", groupName: "Game Settings Top")]
    public KeysanityMode KeysanityMode
    {
        get => _keysanityMode;
        set
        {
            SetField(ref _keysanityMode, value);
            OnPropertyChanged(nameof(IsMetroidKeysanity));
            OnPropertyChanged(nameof(IsZeldaKeysanity));
        }
    }

    [DynamicFormFieldCheckBox("Require Crateria Boss Keycard for Tourian", groupName: "Game Settings Top", visibleWhenTrue: nameof(IsMetroidKeysanity))]
    public bool RequireBossKeycardForTourian { get; set; }

    [DynamicFormFieldCheckBox("Place GT Big Key in Ganon's Tower", groupName: "Game Settings Top", visibleWhenTrue: nameof(IsZeldaKeysanity))]
    public bool PlaceGTBigKeyInGT { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Crystals needed for GT:", groupName: "Game Settings Top")]
    public int CrystalsNeededForGT { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Crystals needed for Ganon:", groupName: "Game Settings Top")]
    public int CrystalsNeededForGanon { get; set; }

    [DynamicFormFieldSlider(0, 4, 0, 1, label: "Bosses needed for Tourian:", groupName: "Game Settings Top")]
    public int BossesNeededForTourian { get; set; }

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

    public bool IsMultiplayer
    {
        get => _isMultiplayer;
        set
        {
            SetField(ref _isMultiplayer, value);
            OnPropertyChanged(nameof(IsSingleplayer));
        }
    }

    public bool IsSingleplayer => !IsMultiplayer;
}
