using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using TrackerCouncil.Smz3.Data.Interfaces;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.SeedGenerator.Generation;
using TrackerCouncil.Smz3.UI.ViewModels;
using TrackerCouncil.Smz3.UI.Views;

namespace TrackerCouncil.Smz3.UI.Services;

public class SoloRomListService(IRomGenerationService romGenerationService,
    IGameDbService gameDbService,
    OptionsFactory optionsFactory,
    SharedCrossplatformService sharedCrossplatformService,
    Smz3RomParser smz3RomParser) : ControlService
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
        if (!_options.GeneralOptions.Validate())
        {
            await MessageWindow.ShowErrorDialog("You must select the A Link to the Past and Super Metroid roms in the Options window before generating a seed.");
            return;
        }

        if (await sharedCrossplatformService.OpenGenerationWindow() != null)
        {
            UpdateList();
        }
    }

    public async Task GeneratePlando()
    {
        if (!_options.GeneralOptions.Validate())
        {
            await MessageWindow.ShowErrorDialog("You must select the A Link to the Past and Super Metroid roms in the Options window before generating a seed.");
            return;
        }

        var storageItem = await CrossPlatformTools.OpenFileDialogAsync(ParentWindow, FileInputControlType.OpenFile,
            "Yaml files (*.yaml, *.yml)|*.yaml;*.yml|All files (*.*)|*.*", _options.RomOutputPath);

        var pathString = storageItem?.TryGetLocalPath();
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
        if (!_options.GeneralOptions.Validate())
        {
            await MessageWindow.ShowErrorDialog("You must select the A Link to the Past and Super Metroid roms in the Options window before generating a seed.");
            return;
        }

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

        _ = sharedCrossplatformService.LaunchRom(result.Rom);
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

    public async Task LaunchRom(GeneratedRomViewModel rom)
    {
        await sharedCrossplatformService.LaunchRom(rom.Rom);
    }

    public async Task LaunchTracker(GeneratedRomViewModel rom)
    {
        await sharedCrossplatformService.LaunchTrackerAsync(rom.Rom);
    }

    public async Task<bool> OpenArchipelagoModeAsync()
    {

        var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var storageItem = await CrossPlatformTools.OpenFileDialogAsync(ParentWindow, FileInputControlType.OpenFile,
                    "Rom file (*.sfc)|*.sfc|All files (*.*)|*.*", userFolder);

        var pathString = storageItem?.TryGetLocalPath();

        if (pathString == null)
        {
            return false;
        }

        var parsedRomDetails = smz3RomParser.ParseRomFile(pathString);

        if (await sharedCrossplatformService.OpenGenerationWindow(importDetails: parsedRomDetails) != null)
        {
            UpdateList();
            return true;
        }

        return false;
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
