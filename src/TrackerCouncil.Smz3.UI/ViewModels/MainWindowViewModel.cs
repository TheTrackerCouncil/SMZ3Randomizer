using ReactiveUI.Fody.Helpers;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {

    }

    [Reactive] public bool DisplayNewVersionBanner { get; set; }
    [Reactive] public bool DisplayDownloadLink { get; set; }

    public string NewVersionGitHubUrl { get; set; } = "";
    public string? NewVersionDownloadUrl { get; set; }

    public bool OpenSetupWindow { get; set; }

    public bool OpenDesktopFileWindow { get; set; }
}
