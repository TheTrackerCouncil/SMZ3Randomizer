using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.SideBySide)]
public class GenerationWindowGameModeOptionsViewModel : ViewModelBase
{
    [DynamicFormFieldComboBox(label: "Alternative game mode:")]
    public GameModeType SelectedGameModeType
    {
        get;
        set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(IsSpazerHunt));
        }
    }

    public bool IsSpazerHunt => SelectedGameModeType == GameModeType.SpazerHunt;

    [DynamicFormFieldSlider(5, 30, 0, 1, label: "Num Spazers in pool:", visibleWhenTrue: nameof(IsSpazerHunt))]
    public int NumSpazersInPool { get; set; } = 30;

    [DynamicFormFieldSlider(5, 30, 0, 1, label: "Required Spazer count", visibleWhenTrue: nameof(IsSpazerHunt))]
    public int RequiredSpazerCount { get; set; } = 20;

    public void FromGameModeOptions(GameModeOptions gameModeOptions)
    {
        SelectedGameModeType = gameModeOptions.SelectedGameModeType;
        NumSpazersInPool = gameModeOptions.NumSpazersInPool;
        RequiredSpazerCount = gameModeOptions.RequiredSpazerCount;
    }

    public GameModeOptions ToGameModeOptions()
    {
        GameModeOptions toReturn = new()
        {
            SelectedGameModeType = SelectedGameModeType,
            NumSpazersInPool = NumSpazersInPool,
            RequiredSpazerCount = RequiredSpazerCount
        };
        return toReturn;
    }
}
