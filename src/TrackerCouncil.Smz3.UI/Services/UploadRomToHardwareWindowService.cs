using AvaloniaControls.Services;
using SnesConnectorLibrary;
using SnesConnectorLibrary.Requests;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Services;

public class UploadRomToHardwareWindowService(ISnesConnectorService snesConnectorService) : ControlService
{
    private UploadRomToHardwareWindowViewModel _model = new UploadRomToHardwareWindowViewModel();

    public UploadRomToHardwareWindowViewModel GetViewModel()
    {
        return _model;
    }

    public void StartUpload(string rom, string target)
    {
        snesConnectorService.UploadFile(new SnesUploadFileRequest()
        {
            LocalFilePath = rom,
            TargetFilePath = target,
            OnComplete = () =>
            {
                _model.MainText = "Complete";
                _model.IsLoading = false;
                _model.ButtonText = "Close";

                snesConnectorService.Disconnect();
            }
        });
    }
}
