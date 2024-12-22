using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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

    public async Task OpenArchipelagoModeAsync()
    {
        var storageItem = await CrossPlatformTools.OpenFileDialogAsync(ParentWindow, FileInputControlType.OpenFile,
                    "Rom file (*.sfc)|*.sfc|All files (*.*)|*.*", "/home/matt/Games/Randomizers/Archipelago");

        var pathString = storageItem?.TryGetLocalPath();

        if (pathString == null)
        {
            return;
        }

        var parsedRomDetails = smz3RomParser.ParseRomFile(pathString);

        if (await sharedCrossplatformService.OpenGenerationWindow(importDetails: parsedRomDetails) != null)
        {
            UpdateList();
        }

        // archipelagoScannerService.ScanArchipelagoRom(rom);

        /*var rom = await File.ReadAllBytesAsync(pathString);
        romGenerationService.ApplyCasPatches(rom, new PatchOptions()
        {
            CasPatches = new CasPatches()
            {
                AimAnyButton = true,
                DisableFlashing = true,
                DisableScreenShake = true,
                EasierWallJumps = true,
                FastDoors = true,
                FastElevators = true,
                InfiniteSpaceJump = true,
                MetroidAutoSave = true,
                NerfedCharge = true,
                SnapMorph = true,
                Respin = true,
                RefillAtSaveStation = true,
                SandPitPlatforms = true,
            },
            MetroidControls = new MetroidControlOptions()
            {
                RunButtonBehavior = RunButtonBehavior.AutoRun,
                ItemCancelBehavior = ItemCancelBehavior.HoldSupersOnly,
                AimButtonBehavior = AimButtonBehavior.UnifiedAim,
                Shoot = MetroidButton.Y,
                Jump = MetroidButton.B,
                Dash = MetroidButton.X,
                ItemSelect = MetroidButton.Select,
                ItemCancel = MetroidButton.R,
                AimUp = MetroidButton.L,
                AimDown = MetroidButton.R
            }
        });

        var folder = Path.GetDirectoryName(pathString)!;
        var fileName = Path.GetFileNameWithoutExtension(pathString)+"_updated";
        var extension = Path.GetExtension(pathString);
        await File.WriteAllBytesAsync(Path.Combine(folder, fileName + extension), rom);*/
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
