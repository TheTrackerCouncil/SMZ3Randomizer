using AvaloniaControls.Services;
using TrackerCouncil.Smz3.Data.Options;

namespace TrackerCouncil.Smz3.UI.Services;

public class CurrentTrackWindowService(OptionsFactory optionsFactory) : ControlService
{
    public void Save(bool isEnabled)
    {
        var options = optionsFactory.Create();
        options.GeneralOptions.DisplayMsuTrackWindow = isEnabled;
        options.Save();
    }
}
