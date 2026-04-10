using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.Vertical, "Random")]
[DynamicFormGroupGroupBox(DynamicFormLayout.Vertical, "Game Mode")]
[DynamicFormGroupGroupBox(DynamicFormLayout.Vertical, "Additional Settings")]
[DynamicFormGroupGroupBox(DynamicFormLayout.TwoColumns, "Race Settings", visibleWhenTrue: nameof(IsSingleplayer))]
public class GenerationWindowGameSettingsViewModel : ViewModelBase
{
    [DynamicFormFieldCheckBox(checkBoxText: "Randomize numeric values", groupName: "Random",
        alignment: DynamicFormAlignment.Center)]
    public bool RandomizeNumericValues
    {
        get;
        set
        {
            SetField(ref field, value);
            GoalSettings.RandomizeGoalCounts = value;
            AdditionalSettings.RandomizeGoalCounts = value;
        }
    }

    [DynamicFormObject(groupName: "Game Mode")]
    public GenerationWindowGoalSettingsViewModel GoalSettings { get; set; } = new();

    [DynamicFormObject(groupName: "Additional Settings")]
    public GenerationWindowAdditionalSettingsViewModel AdditionalSettings { get; set; } = new();

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
