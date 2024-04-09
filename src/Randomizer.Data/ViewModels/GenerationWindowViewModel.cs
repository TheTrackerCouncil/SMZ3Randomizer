using Randomizer.Data.Options;

namespace Randomizer.Data.ViewModels;

public class GenerationWindowViewModel
{
    public GenerationWindowViewModel()
    {

    }

    public GenerationWindowViewModel(RandomizerOptions options)
    {
    }

    public void SetPlandoConfig(PlandoConfig plandoConfig)
    {
        PlandoConfig = plandoConfig;
    }

    public void SetMultiplayerEnabled()
    {
        IsMultiplayer = true;
    }

    public GenerationWindowBasicViewModel Basic { get; set; } = new();
    public GenerationWindowGameSettingsViewModel GameSettings { get; set; } = new();
    public GenerationWindowLogicViewModel Logic { get; set; } = new();
    public GenerationWindowItemsViewModel Items { get; set; } = new();
    public GenerationWindowCustomizationViewModel Customizations { get; set; } = new();

    public PlandoConfig? PlandoConfig { get; set; }
    public bool IsAdvancedMode { get; set; }
    public bool IsPlando => PlandoConfig != null;
    public bool IsRandomizedGame => !IsPlando;
    public bool IsMultiplayer { get; private set; }
    public bool IsSingleplayer => !IsMultiplayer;
}
