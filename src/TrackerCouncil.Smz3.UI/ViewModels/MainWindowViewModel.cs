using ReactiveUI.Fody.Helpers;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {

    }

    [Reactive] public bool DisplayNewVersionBanner { get; set; }

    public string NewVersionGitHubUrl { get; set; } = "";

    public bool HasInvalidOptions { get; set; }
}
