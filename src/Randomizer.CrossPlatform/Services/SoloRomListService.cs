using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MSURandomizerLibrary.Services;
using Randomizer.Abstractions;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.CrossPlatform.Views;
using Randomizer.Data.Interfaces;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Shared.Models;
using Randomizer.SMZ3.Infrastructure;
using Randomizer.SMZ3.Tracking.Services;

namespace Randomizer.CrossPlatform.Services;

public class SoloRomListService(IRomGenerationService romGenerationService,
    IGameDbService gameDbService,
    RomLauncherService romLauncherService,
    OptionsFactory optionsFactory,
    IMsuLookupService msuLookupService) : ControlService
{
    private SoloRomListViewModel _model = new();
    private SoloRomListPanel _panel = null!;
    private Window? _parentWindow;

    public SoloRomListViewModel GetViewModel(SoloRomListPanel panel)
    {
        _panel = panel;
        UpdateList();
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
        gameDbService.DeleteGeneratedRom(rom.Rom, out var error);

        if (!string.IsNullOrEmpty(error))
        {
            OpenMessageWindow(error);
        }
        else
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
        if (!CrossPlatformTools.OpenDirectory(
                Path.Combine(optionsFactory.Create().GeneralOptions.RomOutputPath, rom.Rom.RomPath), true))
        {
            OpenMessageWindow("Could not open rom folder");
        }
    }

    public void PlayRom(GeneratedRomViewModel rom)
    {
        romLauncherService.LaunchRom(rom.Rom);
    }

    public void CopyRomSeed(GeneratedRomViewModel rom)
    {
        CopyTextToClipboard($"{rom.Rom.Seed}");
    }

    public void CopyRomConfigString(GeneratedRomViewModel rom)
    {
        CopyTextToClipboard($"{rom.Rom.Settings}");
    }

    public void OpenSpoilerLog(GeneratedRomViewModel rom)
    {
        var path = Path.Combine(optionsFactory.Create().RomOutputPath, rom.Rom.SpoilerPath);
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
                OpenMessageWindow($"Could not open spoiler file at {path}");
            }
        }
        else
        {
            OpenMessageWindow($"Could not find spoiler file at {path}");
        }
    }

    public void OpenProgressionHistory(GeneratedRomViewModel rom)
    {
        if (rom.Rom.TrackerState == null)
        {
            OpenMessageWindow("There is no history for the selected rom.");
            return;
        }

        var history = gameDbService.GetGameHistory(rom.Rom);

        if (rom.Rom.TrackerState?.History == null || rom.Rom.TrackerState.History.Count == 0)
        {
            OpenMessageWindow("There is no history for the selected rom.");
            return;
        }

        var path = Path.Combine(optionsFactory.Create().RomOutputPath, rom.Rom.SpoilerPath).Replace("Spoiler_Log", "Progression_Log");
        var historyText = HistoryService.GenerateHistoryText(rom.Rom, history.ToList());

        try
        {
            File.WriteAllText(path, historyText);
        }
        catch
        {
            OpenMessageWindow($"Could not write progression history to file {path}");
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
                OpenMessageWindow($"Could not open progression history file at {path}");
            }
        }
        else
        {
            OpenMessageWindow($"Could not find progression history file at {path}");
        }
    }

    public void LaunchRom(GeneratedRomViewModel rom)
    {
        var launchButtonOptions = optionsFactory.Create().GeneralOptions.LaunchButtonOption;

        if (launchButtonOptions is LaunchButtonOptions.PlayAndTrack or LaunchButtonOptions.OpenFolderAndTrack or LaunchButtonOptions.TrackOnly)
        {
            // LaunchTracker(rom);
        }

        if (launchButtonOptions is LaunchButtonOptions.OpenFolderAndTrack or LaunchButtonOptions.OpenFolderOnly)
        {
            OpenFolder(rom);
        }

        if (launchButtonOptions is LaunchButtonOptions.PlayAndTrack or LaunchButtonOptions.PlayOnly)
        {
            PlayRom(rom);
        }
    }

    public void LookupMsus()
    {
        var msuDirectory = optionsFactory.Create().GeneralOptions.MsuPath;
        if (string.IsNullOrEmpty(msuDirectory) || !Directory.Exists(msuDirectory))
        {
            return;
        }

        ITaskService.Run(() =>
        {
            msuLookupService.LookupMsus(msuDirectory);
        });
    }

    private void CopyTextToClipboard(string text)
    {
        GetParentWindow().Clipboard?.SetTextAsync(text);
    }

    private MessageWindow OpenMessageWindow(string message, MessageWindowIcon icon = MessageWindowIcon.Error, MessageWindowButtons buttons = MessageWindowButtons.OK)
    {
        var window = new MessageWindow(new MessageWindowRequest()
        {
            Message = message,
            Title = "SMZ3 Cas' Randomizer",
            Icon = icon,
            Buttons = buttons
        });

        window.ShowDialog(GetParentWindow());

        return window;
    }

    private Window GetParentWindow()
    {
        if (_parentWindow != null)
        {
            return _parentWindow;
        }

        _parentWindow = TopLevel.GetTopLevel(_panel) as Window ??
                        throw new InvalidOperationException("Panel is not attached to a window");
        return _parentWindow;
    }
}
