using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.CrossPlatform.Views;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.CrossPlatform.Services;

public class MultiRomListService(IServiceProvider serviceProvider, OptionsFactory optionsFactory, IGameDbService gameDbService) : ControlService
{
    private MultiRomListViewModel _model = new();
    private MultiRomListPanel _panel = null!;
    private Window? _parentWindow;
    private RandomizerOptions _options = null!;

    public MultiRomListViewModel GetViewModel(MultiRomListPanel panel)
    {
        _panel = panel;
        _options = optionsFactory.Create();
        UpdateList();
        return _model;
    }

    public void UpdateList()
    {
        _model.UpdateList(gameDbService.GetMultiplayerGamesList()
            .OrderByDescending(x => x.Id).ToList());
    }

    public void OpenStatusWindow(MultiplayerRomViewModel model)
    {
        var window = new MultiplayerStatusWindow(model, ParentWindow);
        window.Show();
    }

    public async Task OpenConnectWindow(bool isCreate)
    {
        var window = new MultiplayerConnectWindow(isCreate);
        await window.ShowDialog(ParentWindow);
        if (!window.DialogResult)
        {
            return;
        }
        UpdateList();

        if (window.MultiplayerGameDetails != null)
        {
            OpenStatusWindow(new MultiplayerRomViewModel(window.MultiplayerGameDetails));
        }
    }

    public void DeleteRom(MultiplayerRomViewModel rom)
    {
        if (!gameDbService.DeleteMultiplayerGame(rom.Details, out var error))
        {
            DisplayError(error);
            return;
        }

        if (!string.IsNullOrEmpty(error))
        {
            DisplayError(error);
        }
        else
        {
            UpdateList();
        }
    }

    public void OpenFolder(MultiplayerRomViewModel rom)
    {
        if (string.IsNullOrEmpty(rom.Details.GeneratedRom?.RomPath))
        {
            return;
        }

        if (!CrossPlatformTools.OpenDirectory(
                Path.Combine(optionsFactory.Create().GeneralOptions.RomOutputPath, rom.Details.GeneratedRom.RomPath), true))
        {
            DisplayError("Could not open rom folder");
        }
    }

    public void OpenSpoilerLog(MultiplayerRomViewModel rom)
    {
        if (string.IsNullOrEmpty(rom.Details.GeneratedRom?.RomPath))
        {
            return;
        }

        var path = Path.Combine(optionsFactory.Create().RomOutputPath, rom.Details.GeneratedRom.SpoilerPath);
        if (File.Exists(path))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception e)
            {
                DisplayError($"Could not open spoiler file at {path}");
            }
        }
        else
        {
            DisplayError($"Could not find spoiler file at {path}");
        }
    }

    public void OpenProgressionHistory(MultiplayerRomViewModel rom)
    {
        if (rom.Details.GeneratedRom?.TrackerState == null)
        {
            DisplayError("There is no history for the selected rom.");
            return;
        }

        var history = gameDbService.GetGameHistory(rom.Details.GeneratedRom);

        if (rom.Details.GeneratedRom.TrackerState?.History == null || rom.Details.GeneratedRom.TrackerState.History.Count == 0)
        {
            DisplayError("There is no history for the selected rom.");
            return;
        }

        var path = Path.Combine(optionsFactory.Create().RomOutputPath, rom.Details.GeneratedRom.SpoilerPath).Replace("Spoiler_Log", "Progression_Log");
        var historyText = HistoryService.GenerateHistoryText(rom.Details.GeneratedRom, history.ToList());

        try
        {
            File.WriteAllText(path, historyText);
        }
        catch
        {
            DisplayError($"Could not write progression history to file {path}");
            return;
        }

        if (File.Exists(path))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception e)
            {
                DisplayError($"Could not open progression history file at {path}");
            }
        }
        else
        {
            DisplayError($"Could not find progression history file at {path}");
        }
    }

    private void DisplayError(string error)
    {
        var window = new MessageWindow(new MessageWindowRequest()
        {
            Message = error,
            Title = "SMZ3 Cas' Randomizer",
            Icon = MessageWindowIcon.Error,
            Buttons = MessageWindowButtons.OK
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
            return _parentWindow;
        }
    }
}
