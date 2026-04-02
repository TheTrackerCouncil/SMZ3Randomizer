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
    [DynamicFormFieldComboBox(label: "Game mode:")]
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

    [DynamicFormFieldComboBox(label: "Randomize goal values:", visibleWhenTrue: nameof(AlwaysHidden))] // TODO: remove always hidden
    public YesNoEnum RandomizeGoalCounts
    {
        get;
        set
        {
            SetField(ref field, value);
            UpdateBoolProperties();
        }
    }

    [DynamicFormFieldCheckBox("Ganon/GT/Tourian requirements hidden", visibleWhenTrue: nameof(AlwaysHidden))]
    public bool HideCrystalBossCount { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Crystals needed for GT:", visibleWhenTrue: nameof(StaticGoalCounts))]
    public int GanonsTowerCrystalCount { get; set; }

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Minimum crystals needed for GT:", visibleWhenTrue: nameof(RandomizedGoalCounts))]
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

    [DynamicFormFieldSlider(0, 7, 0, 1, label: "Maximum crystals needed for GT:", visibleWhenTrue: nameof(RandomizedGoalCounts))]
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

    [DynamicFormFieldComboBox(label: "Skip Ganon/Tourian by lifting off:")]
    public YesNoEnum LiftOffOnGoalCompletion
    {
        get;
        set
        {
            SetField(ref field, value);
            UpdateBoolProperties();
        }
    }

    public bool AlwaysHidden => false;
    public bool ShowStaticCrystalBossCounts => StaticGoalCounts && IsDefaultGameMode;
    public bool ShowRandomizedCrystalBossCounts => RandomizedGoalCounts && IsDefaultGameMode;
    public bool ShowStaticSpazerCounts => StaticGoalCounts && IsSpazerHunt;
    public bool ShowRandomizedSpazerCounts => RandomizedGoalCounts && IsSpazerHunt;
    public bool StaticGoalCounts => RandomizeGoalCounts == YesNoEnum.No;
    public bool RandomizedGoalCounts => RandomizeGoalCounts == YesNoEnum.Yes;
    public bool IsDefaultGameMode => SelectedGameModeType == GameModeType.Vanilla;
    public bool IsAltGameMode => SelectedGameModeType != GameModeType.Vanilla;
    public bool IsSpazerHunt => false && SelectedGameModeType == GameModeType.SpazerHunt; // TODO: Remove false
    public Dictionary<GameModeType, string> GameModeTypeDescriptions = new();

    public void UpdateViewModel(GameModeOptions config)
    {
        var configProperties = config.GetType().GetProperties();
        var viewModelProperties = GetType().GetProperties().ToDictionary(x => x.Name, x => x);

        foreach (var configProperty in configProperties)
        {
            if (viewModelProperties.TryGetValue(configProperty.Name, out var viewModelProperty))
            {
                var newValue = configProperty.GetValue(config);
                if (configProperty.PropertyType == typeof(bool))
                {
                    newValue = newValue as bool? == true ? YesNoEnum.Yes : YesNoEnum.No;
                }
                viewModelProperty.SetValue(this, newValue);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Config property {configProperty.Name} does not exist in view model");
            }
        }
    }

    public void UpdateConfig(GameModeOptions config)
    {
        var configProperties = config.GetType().GetProperties();
        var viewModelProperties = GetType().GetProperties().ToDictionary(x => x.Name, x => x);

        foreach (var configProperty in configProperties)
        {
            if (viewModelProperties.TryGetValue(configProperty.Name, out var viewModelProperty))
            {
                var newValue = viewModelProperty.GetValue(this);
                if (configProperty.PropertyType == typeof(bool))
                {
                    newValue = newValue as YesNoEnum? == YesNoEnum.Yes;
                }
                configProperty.SetValue(config, newValue);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Config property {configProperty.Name} does not exist in view model");
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
