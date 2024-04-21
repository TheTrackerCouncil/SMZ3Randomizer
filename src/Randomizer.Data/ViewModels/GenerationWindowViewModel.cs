using Randomizer.Data.Options;

namespace Randomizer.Data.ViewModels;

public class GenerationWindowViewModel : ViewModelBase
{
    private PlandoConfig? _plandoConfig;
    private bool _isMultiplayer;

    public GenerationWindowBasicViewModel Basic { get; set; } = new();
    public GenerationWindowGameSettingsViewModel GameSettings { get; set; } = new();
    public GenerationWindowLogicViewModel Logic { get; set; } = new();
    public GenerationWindowItemsViewModel Items { get; set; } = new();
    public GenerationWindowCustomizationViewModel Customizations { get; set; } = new();

    public PlandoConfig? PlandoConfig
    {
        get => _plandoConfig;
        set
        {
            SetField(ref _plandoConfig, value);
            OnPropertyChanged(nameof(IsPlando));
            OnPropertyChanged(nameof(IsRandomizedGame));
            OnPropertyChanged(nameof(CanSetSeed));
            Logic.IsNotPlando = false;
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
        }
    }

    public bool CanSetSeed => !IsPlando && !IsMultiplayer;
    public bool IsPlando => PlandoConfig != null;
    public bool IsRandomizedGame => !IsPlando;
    public bool IsSingleplayer => !IsMultiplayer;
}
