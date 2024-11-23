using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;

namespace TrackerCouncil.Smz3.Data.ViewModels;

public class GenerationWindowViewModel : ViewModelBase
{
    private PlandoConfig? _plandoConfig;
    private bool _isMultiplayer;
    private bool _isImportMode;

    public GenerationWindowBasicViewModel Basic { get; set; } = new();
    public GenerationWindowGameSettingsViewModel GameSettings { get; set; } = new();
    public GenerationWindowLogicViewModel Logic { get; set; } = new();
    public GenerationWindowItemsViewModel Items { get; set; } = new();
    public GenerationWindowCustomizationViewModel Customizations { get; set; } = new();
    public ParsedRomDetails? ImportDetails { get; set; }

    public PlandoConfig? PlandoConfig
    {
        get => _plandoConfig;
        set
        {
            SetField(ref _plandoConfig, value);
            OnPropertyChanged(nameof(IsPlando));
            OnPropertyChanged(nameof(IsRandomizedGame));
            OnPropertyChanged(nameof(CanChangeGameSettings));
            OnPropertyChanged(nameof(CanSetSeed));
            Logic.CanChangeGameSettings = false;
        }
    }

    public bool IsMultiplayer
    {
        get => _isMultiplayer;
        set
        {
            SetField(ref _isMultiplayer, value);
            GameSettings.IsMultiplayer = value;
            OnPropertyChanged(nameof(IsSingleplayer));
            OnPropertyChanged(nameof(CanSetSeed));
            OnPropertyChanged(nameof(CanChangeGameSettings));
            OnPropertyChanged(nameof(ShowGenerateSplitButton));
        }
    }

    public bool IsImportMode
    {
        get => _isImportMode;
        set
        {
            SetField(ref _isImportMode, value);
            GameSettings.IsMultiplayer = value;
            OnPropertyChanged(nameof(IsSingleplayer));
            OnPropertyChanged(nameof(CanSetSeed));
            OnPropertyChanged(nameof(CanChangeGameSettings));
            OnPropertyChanged(nameof(ShowGenerateSplitButton));
            Logic.CanChangeGameSettings = false;
        }
    }

    public bool CanSetSeed => !IsPlando && !IsMultiplayer;
    public bool IsPlando => PlandoConfig != null;
    public bool IsRandomizedGame => !IsPlando;
    public bool IsSingleplayer => !IsMultiplayer;
    public bool CanChangeGameSettings => !IsPlando && !IsImportMode;
    public bool ShowGenerateSplitButton => IsSingleplayer && !IsImportMode;


}
