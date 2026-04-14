using System;
using System.Linq;
using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Main")]
public class GenerationWindowAdditionalSettingsViewModel : ViewModelBase
{
    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Crystals needed for GT:", visibleWhenTrue: nameof(ShowStaticGtCounts))]
    public int GanonsTowerCrystalCount { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Minimum crystals needed for GT:", visibleWhenTrue: nameof(ShowRandomizedGtCounts))]
    public int MinGanonsTowerCrystalCount
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value > MaxGanonsTowerCrystalCount)
            {
                MaxGanonsTowerCrystalCount = value;
            }
        }
    }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Maximum crystals needed for GT:", visibleWhenTrue: nameof(ShowRandomizedGtCounts))]
    public int MaxGanonsTowerCrystalCount
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value < MinGanonsTowerCrystalCount)
            {
                MinGanonsTowerCrystalCount = value;
            }
        }
    }

    [DynamicFormFieldBoolComboBox("Pyramid access:", falseText: "Opens after Ganon's Tower is completed", trueText: "Open by default", trueFirst: false)]
    public bool OpenPyramid { get; set; }

    [DynamicFormFieldBoolComboBox("Metroid boss tokens:", falseText: "Disabled", trueText: "Shuffled with dungeon rewards", trueFirst: false)]
    public bool ShuffleMetroidBossTokens { get; set; }

    [DynamicFormFieldComboBox(label: "Keysanity:")]
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

    [DynamicFormFieldBoolComboBox("Metroid statue room access:", falseText: "Require Crateria boss keycard", trueText: "Open by default", visibleWhenTrue: nameof(IsMetroidKeysanity))]
    public bool SkipTourianBossDoor { get; set; }

    [DynamicFormFieldBoolComboBox("Ganon's Tower big key location:", falseText: "Fully randomized", trueText: "In Ganon's Tower", trueFirst: false, visibleWhenTrue: nameof(IsZeldaKeysanity))]
    public bool PlaceGTBigKeyInGT { get; set; }

    public bool IsMetroidKeysanity => KeysanityMode is KeysanityMode.Both or KeysanityMode.SuperMetroid;

    public bool IsZeldaKeysanity => KeysanityMode is KeysanityMode.Both or KeysanityMode.Zelda;

    public bool AlwaysHidden => false;
    public bool RandomizeGoalCounts
    {
        get;
        set
        {
            SetField(ref field, value);
            OnPropertyChanged(nameof(ShowStaticGtCounts));
            OnPropertyChanged(nameof(ShowRandomizedGtCounts));
        }
    }
    public bool ShowStaticGtCounts => !RandomizeGoalCounts;
    public bool ShowRandomizedGtCounts => RandomizeGoalCounts;

   public void UpdateViewModel(GameModeOptions config)
    {
        var configProperties = config.GetType().GetProperties();
        var viewModelProperties = GetType().GetProperties().ToDictionary(x => x.Name, x => x);

        foreach (var configProperty in configProperties)
        {
            if (viewModelProperties.TryGetValue(configProperty.Name, out var viewModelProperty) && viewModelProperty.CanWrite)
            {
                var newValue = configProperty.GetValue(config);
                viewModelProperty.SetValue(this, newValue);
            }
        }
    }

    public void UpdateConfig(GameModeOptions config)
    {
        var configProperties = config.GetType().GetProperties();
        var viewModelProperties = GetType().GetProperties().ToDictionary(x => x.Name, x => x);

        foreach (var configProperty in configProperties)
        {
            if (viewModelProperties.TryGetValue(configProperty.Name, out var viewModelProperty) && viewModelProperty.CanWrite)
            {
                var newValue = viewModelProperty.GetValue(this);
                configProperty.SetValue(config, newValue);
            }
        }
    }
}
