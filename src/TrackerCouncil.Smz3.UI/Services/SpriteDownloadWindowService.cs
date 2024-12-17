using AvaloniaControls.ControlServices;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class SpriteDownloadWindowService(IGitHubFileSynchronizerService gitHubFileSynchronizerService) : ControlService
{
    private SpriteDownloadWindowViewModel _model = new();

    public SpriteDownloadWindowViewModel InitializeModel()
    {
        gitHubFileSynchronizerService.SynchronizeUpdate += (sender, args) =>
        {
            _model.NumTotal = args.Total;
            _model.NumCompleted = args.Completed;
        };

        return _model;
    }

    public void CancelDownload()
    {
        gitHubFileSynchronizerService.CancelDownload();
    }
}
