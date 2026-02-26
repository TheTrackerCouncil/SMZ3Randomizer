using ReactiveUI.SourceGenerators;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {

    }

    [Reactive] public partial bool DisplayNewVersionBanner { get; set; }
    [Reactive] public partial bool DisplayDownloadLink { get; set; }
    [Reactive] public partial bool IsLoadingSprites { get; set; } = true;

    public string NewVersionGitHubUrl { get; set; } = "";
    public string? NewVersionDownloadUrl { get; set; }

    public bool OpenSetupWindow { get; set; }

    public bool OpenDesktopFileWindow { get; set; }
}
