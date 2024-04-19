using ReactiveUI.Fody.Helpers;

namespace Randomizer.CrossPlatform.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {

    }

    [Reactive] public bool DisplayNewVersionBanner { get; set; }

    public string NewVersionGitHubUrl { get; set; } = "";

    public bool HasInvalidOptions { get; set; }
}
