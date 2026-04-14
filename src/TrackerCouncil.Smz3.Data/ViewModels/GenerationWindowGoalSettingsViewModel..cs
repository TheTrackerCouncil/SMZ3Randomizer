using System;
using System.Collections.Generic;
using System.Linq;
using DynamicForms.Library.Core;
using DynamicForms.Library.Core.Attributes;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Data.ViewModels;

[DynamicFormGroupBasic(DynamicFormLayout.SideBySide, "Main")]
public class GenerationWindowGoalSettingsViewModel : ViewModelBase
{
    [DynamicFormFieldComboBox(label: "Goal:")]
    public GameModeType SelectedGameModeType
    {
        get;
        set
        {
            SetField(ref field, value);
            UpdateBoolProperties();
            Description = GameModeTypeDescriptions[value];
            OnPropertyChanged(nameof(Description));
        }
    }

    [DynamicFormFieldText]
    public string Description { get; set; } = "";

    [DynamicFormFieldCheckBox("Ganon/GT/Tourian requirements hidden", visibleWhenTrue: nameof(AlwaysHidden))]
    public bool HideCrystalBossCount { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Crystals needed for Ganon:", visibleWhenTrue: nameof(ShowStaticCrystalBossCounts))]
    public int GanonCrystalCount { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Minimum crystals needed for Ganon:", visibleWhenTrue: nameof(ShowRandomizedCrystalBossCounts))]
    public int MinGanonCrystalCount
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value > MaxGanonCrystalCount)
            {
                MaxGanonCrystalCount = value;
            }
        }
    }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Maximum crystals needed for Ganon:", visibleWhenTrue: nameof(ShowRandomizedCrystalBossCounts))]
    public int MaxGanonCrystalCount
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value < MinGanonCrystalCount)
            {
                MinGanonCrystalCount = value;
            }
        }
    }

    [DynamicFormFieldSlider(0, 4, 0, 1, label: "Bosses needed for Tourian:", visibleWhenTrue: nameof(ShowStaticCrystalBossCounts))]
    public int TourianBossCount { get; set; }

    [DynamicFormFieldSlider(0, 4, 0, 1, label: "Minimum bosses needed for Tourian:", visibleWhenTrue: nameof(ShowRandomizedCrystalBossCounts))]
    public int MinTourianBossCount
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value > MaxTourianBossCount)
            {
                MaxTourianBossCount = value;
            }
        }
    }

    [DynamicFormFieldSlider(0, 4, 0, 1, label: "Maximum bosses needed for Tourian:", visibleWhenTrue: nameof(ShowRandomizedCrystalBossCounts))]
    public int MaxTourianBossCount
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value < MinTourianBossCount)
            {
                MinTourianBossCount = value;
            }
        }
    }

    [DynamicFormFieldSlider(1, 80, 0, 1, label: "Spazer count in pool:", visibleWhenTrue: nameof(ShowStaticSpazerCounts))]
    public int SpazersInPool
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value < SpazersRequired)
            {
                SpazersRequired = value;
            }
        }
    }

    [DynamicFormFieldSlider(1, 80, 0, 1, label: "Minimum spazer count in pool:", visibleWhenTrue: nameof(ShowRandomizedSpazerCounts))]
    public int MinSpazersInPool
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value > MaxSpazersInPool)
            {
                MaxSpazersInPool = value;
            }

            if (value < MinSpazersRequired)
            {
                MinSpazersRequired = value;
            }
        }
    }

    [DynamicFormFieldSlider(1, 80, 0, 1, label: "Maximum spazer count in pool:", visibleWhenTrue: nameof(ShowRandomizedSpazerCounts))]
    public int MaxSpazersInPool
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value < MinSpazersInPool)
            {
                MinSpazersInPool = value;
            }

            if (value < MaxSpazersRequired)
            {
                MaxSpazersRequired = value;
            }
        }
    }

    [DynamicFormFieldSlider(1, 80, 0, 1, label: "Spazers needed:", visibleWhenTrue: nameof(ShowStaticSpazerCounts))]
    public int SpazersRequired
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value > SpazersInPool)
            {
                SpazersInPool = value;
            }
        }
    }

    [DynamicFormFieldSlider(1, 80, 0, 1, label: "Minimum spazers needed:", visibleWhenTrue: nameof(ShowRandomizedSpazerCounts))]
    public int MinSpazersRequired
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value > MaxSpazersRequired)
            {
                MaxSpazersRequired = value;
            }

            if (value > MinSpazersInPool)
            {
                MinSpazersInPool = value;
            }
        }
    }

    [DynamicFormFieldSlider(1, 80, 0, 1, label: "Maximum spazers needed:", visibleWhenTrue: nameof(ShowRandomizedSpazerCounts))]
    public int MaxSpazersRequired
    {
        get;
        set
        {
            SetField(ref field, value);
            if (value < MinSpazersRequired)
            {
                MinSpazersRequired = value;
            }

            if (value > MaxSpazersInPool)
            {
                MaxSpazersInPool = value;
            }
        }
    }

    [DynamicFormFieldBoolComboBox(label: "Game completion:", falseText: "Defeat Mother Brain and Ganon", trueText: "Enter ship after obtaining goal", trueFirst: false)]
    public bool LiftOffOnGoalCompletion
    {
        get;
        set
        {
            SetField(ref field, value);
            UpdateBoolProperties();
        }
    }

    public bool AlwaysHidden => false;
    public bool RandomizeGoalCounts
    {
        get;
        set
        {
            SetField(ref field, value);
            UpdateBoolProperties();
        }
    }
    public bool ShowStaticCrystalBossCounts => StaticGoalCounts && IsDefaultGameMode;
    public bool ShowRandomizedCrystalBossCounts => RandomizeGoalCounts && IsDefaultGameMode;
    public bool ShowStaticSpazerCounts => StaticGoalCounts && IsSpazerHunt;
    public bool ShowRandomizedSpazerCounts => RandomizeGoalCounts && IsSpazerHunt;
    public bool StaticGoalCounts => !RandomizeGoalCounts;
    public bool IsDefaultGameMode => SelectedGameModeType == GameModeType.Vanilla;
    public bool IsAltGameMode => SelectedGameModeType != GameModeType.Vanilla;
    public bool IsSpazerHunt => SelectedGameModeType == GameModeType.SpazerHunt;
    public Dictionary<GameModeType, string> GameModeTypeDescriptions = new();

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

    private void UpdateBoolProperties()
    {
        foreach (var prop in this.GetType().GetProperties().Where(x => x.PropertyType == typeof(bool)))
        {
            OnPropertyChanged(prop.Name);
        }
    }
}
