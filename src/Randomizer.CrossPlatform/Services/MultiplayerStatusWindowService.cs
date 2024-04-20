using AvaloniaControls.ControlServices;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.CrossPlatform.Views;

namespace Randomizer.CrossPlatform.Services;

public class MultiplayerStatusWindowService : ControlService
{
    private MultiplayerStatusWindow _window = null!;
    private MultiplayerStatusWindowViewModel _model = new();

    public MultiplayerStatusWindowViewModel GetViewModel(MultiplayerStatusWindow window, MultiplayerRomViewModel rom)
    {
        _window = window;
        return _model;
    }
}
