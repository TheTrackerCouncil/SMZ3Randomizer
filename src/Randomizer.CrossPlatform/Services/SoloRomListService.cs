using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Controls;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.CrossPlatform.Views;
using Randomizer.Data.Interfaces;
using Randomizer.Data.Options;
using Randomizer.Data.Services;

namespace Randomizer.CrossPlatform.Services;

public class SoloRomListService(IRomGenerationService romGenerationService,
    IGameDbService gameDbService,
    OptionsFactory optionsFactory,
    SharedCrossplatformService sharedCrossplatformService) : ControlService
{
    private SoloRomListViewModel _model = new();
    private SoloRomListPanel _panel = null!;
    private Window? _parentWindow;
    private RandomizerOptions _options = null!;

    public SoloRomListViewModel GetViewModel(SoloRomListPanel panel)
    {
        _panel = panel;
        sharedCrossplatformService.ParentControl = _panel;
        _options = optionsFactory.Create();
        UpdateList();
        sharedCrossplatformService.LookupMsus();
        return _model;
    }

    public void UpdateList()
    {
        var roms = gameDbService.GetGeneratedRomsList()
            .OrderByDescending(x => x.Id)
            .ToList();
        _model.Roms = roms.Select(x => new GeneratedRomViewModel(x)).ToList();
    }

    public void DeleteRom(GeneratedRomViewModel rom)
    {
        if (sharedCrossplatformService.DeleteRom(rom.Rom))
        {
            UpdateList();
        }
    }

    public void UpdateRomLabel(GeneratedRomViewModel rom, string text)
    {
        if (gameDbService.UpdateGeneratedRom(rom.Rom, label: text))
        {
            UpdateList();
        }

        rom.IsEditTextBoxVisible = false;
    }

    public void OpenFolder(GeneratedRomViewModel rom)
    {
        sharedCrossplatformService.OpenFolder(rom.Rom);
    }

    public void PlayRom(GeneratedRomViewModel rom)
    {
        sharedCrossplatformService.PlayRom(rom.Rom);
    }

    public async Task GenerateRom()
    {
        if (await sharedCrossplatformService.OpenGenerationWindow() != null)
        {
            UpdateList();
        }
    }

    public async Task GeneratePlando()
    {
        var storageItem = await CrossPlatformTools.OpenFileDialogAsync(ParentWindow, FileInputControlType.OpenFile,
            "Yaml files (*.yaml, *.yml)|*.yaml;*.yml|All files (*.*)|*.*", _options.RomOutputPath);

        var pathString = HttpUtility.UrlDecode(storageItem?.Path.AbsolutePath);
        if (string.IsNullOrEmpty(pathString) || !File.Exists(pathString))
        {
            return;
        }

        if (await sharedCrossplatformService.OpenGenerationWindow(plandoConfig: pathString) != null)
        {
            UpdateList();
        }
    }

    public async Task QuickPlay()
    {
        var result = await romGenerationService.GenerateRandomRomAsync(optionsFactory.Create());
        if (!string.IsNullOrEmpty(result.GenerationError))
        {
            OpenMessageWindow(result.GenerationError);
            return;
        }
        else if (result.Rom == null)
        {
            OpenMessageWindow("Could not generate rom");
            return;
        }

        sharedCrossplatformService.LaunchRom(result.Rom);
    }

    public void CopyRomSeed(GeneratedRomViewModel rom)
    {
        sharedCrossplatformService.CopyRomSeed(rom.Rom);
    }

    public void CopyRomConfigString(GeneratedRomViewModel rom)
    {
        sharedCrossplatformService.CopyRomConfigString(rom.Rom);
    }

    public void OpenSpoilerLog(GeneratedRomViewModel rom)
    {
        sharedCrossplatformService.OpenSpoilerLog(rom.Rom);
    }

    public void OpenProgressionHistory(GeneratedRomViewModel rom)
    {
        sharedCrossplatformService.OpenProgressionHistory(rom.Rom);
    }

    public void LaunchRom(GeneratedRomViewModel rom)
    {
        sharedCrossplatformService.LaunchRom(rom.Rom);
    }

    public void LaunchTracker(GeneratedRomViewModel rom)
    {
        sharedCrossplatformService.LaunchTracker(rom.Rom);
    }

    private void OpenMessageWindow(string message, MessageWindowIcon icon = MessageWindowIcon.Error, MessageWindowButtons buttons = MessageWindowButtons.OK)
    {
        var window = new MessageWindow(new MessageWindowRequest()
        {
            Message = message,
            Title = "SMZ3 Cas' Randomizer",
            Icon = icon,
            Buttons = buttons
        });

        window.ShowDialog(ParentWindow);
    }

    private Window ParentWindow
    {
        get
        {
            if (_parentWindow != null)
            {
                return _parentWindow;
            }

            _parentWindow = (Window)TopLevel.GetTopLevel(_panel)!;

            if (_parentWindow.Owner is Window window)
            {
                _parentWindow = window;
            }

            return _parentWindow;
        }
    }
}
