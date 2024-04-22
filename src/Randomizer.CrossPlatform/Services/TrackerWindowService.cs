using System.IO;
using Avalonia.Controls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using Randomizer.Abstractions;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.CrossPlatform.Views;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Generation;

namespace Randomizer.CrossPlatform.Services;

public class TrackerWindowService(
    Smz3GeneratedRomLoader romLoader,
    SharedCrossplatformService sharedCrossplatformService,
    TrackerBase tracker,
    OptionsFactory optionsFactory) : ControlService
{
    private RandomizerOptions? _options;
    private TrackerWindow _window = null!;
    private TrackerWindowViewModel _model = new();

    public TrackerWindowViewModel GetViewModel(TrackerWindow parent)
    {
        _window = parent;
        return _model;
    }
    public void SetRom(GeneratedRom rom)
    {
        if (!GeneratedRom.IsValid(rom))
        {
            return;
        }

        sharedCrossplatformService.LookupMsus();
        var romPath = Path.Combine(Options.RomOutputPath, rom.RomPath);
        tracker.Load(rom, romPath);
    }

    public void StartTracker()
    {
        if (!tracker.TryStartTracking())
        {
            OpenMessageWindow("There was a problem with loading one or more of the tracker modules.\n" +
                              "Some tracking functionality may be limited.", MessageWindowIcon.Warning);
        }

        tracker.AutoTracker?.SetConnector(Options.GeneralOptions.SnesConnectorSettings);
    }

    private MessageWindow DisplayError(string message) => OpenMessageWindow(message);

    private MessageWindow OpenMessageWindow(string message, MessageWindowIcon icon = MessageWindowIcon.Error, MessageWindowButtons buttons = MessageWindowButtons.OK)
    {
        var window = new MessageWindow(new MessageWindowRequest()
        {
            Message = message,
            Title = "SMZ3 Cas' Randomizer",
            Icon = icon,
            Buttons = buttons
        });

        window.ShowDialog(_window);

        return window;
    }

    private RandomizerOptions Options
    {
        get
        {
            if (_options != null)
            {
                return _options;
            }

            _options = optionsFactory.Create();
            return _options;
        }
    }
}
