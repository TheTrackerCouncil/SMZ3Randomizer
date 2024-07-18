using AvaloniaControls.ControlServices;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class SpriteDownloadWindowService(IGitHubSpriteDownloaderService gitHubSpriteDownloaderService) : ControlService
{
    private SpriteDownloadWindowViewModel _model = new();

    public SpriteDownloadWindowViewModel InitializeModel()
    {
        gitHubSpriteDownloaderService.SpriteDownloadUpdate += (sender, args) =>
        {
            _model.NumTotal = args.Total;
            _model.NumCompleted = args.Completed;
        };

        return _model;
    }

    public void CancelDownload()
    {
        gitHubSpriteDownloaderService.CancelDownload();
    }
}
