using AvaloniaControls.ControlServices;
using Randomizer.Data.Options;

namespace Randomizer.CrossPlatform.Services;

public class CurrentTrackWindowService(OptionsFactory optionsFactory) : ControlService
{
    public void Save(bool isEnabled)
    {
        var options = optionsFactory.Create();
        options.GeneralOptions.DisplayMsuTrackWindow = isEnabled;
        options.Save();
    }
}
