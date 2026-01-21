using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSURandomizerLibrary;
using MSURandomizerLibrary.Services;
using TrackerCouncil.Smz3.Data;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.SeedGenerator.Generation;
using TrackerCouncil.Smz3.SeedGenerator.Infrastructure;
using TrackerCouncil.Smz3.Shared.Models;
using TrackerCouncil.Smz3.Tracking.Services;
using TrackerCouncil.Smz3.UI.Views;

namespace TrackerCouncil.Smz3.UI.Services;

public class SharedCrossplatformService(
    IGameDbService gameDbService,
    OptionsFactory optionsFactory,
    RomLauncherService romLauncherService,
    IServiceProvider serviceProvider,
    IMsuLookupService msuLookupService,
    Smz3GeneratedRomLoader smz3GeneratedRomLoader,
    IMsuTypeService msuTypeService,
    IMetadataService metadataService,
    ILogger<SharedCrossplatformService> logger)
{
    private static TrackerWindow? s_trackerWindow;
    private RandomizerOptions? _options;
    private Window? _parentWindow;

    public Control? ParentControl { get; set; }

    public void OpenProgressionHistory(GeneratedRom? rom)
    {
        if (rom?.TrackerState == null)
        {
            DisplayError("There is no history for the selected rom.");
            return;
        }

        var history = gameDbService.GetGameHistory(rom);

        if (rom.TrackerState?.History == null || rom.TrackerState.History.Count == 0)
        {
            DisplayError("There is no history for the selected rom.");
            return;
        }

        var path = Path.Combine(Options.RomOutputPath, rom.SpoilerPath).Replace("Spoiler_Log", "Progression_Log");
        var historyText = HistoryService.GenerateHistoryText(rom, history.ToList());

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
                logger.LogError(e, "Could not open progression history file at {Path}", path);
                DisplayError($"Could not open progression history file at {path}");
            }
        }
        else
        {
            DisplayError($"Could not find progression history file at {path}");
        }
    }

    public void OpenSpoilerLog(GeneratedRom? rom)
    {
        if (rom == null)
        {
            DisplayError("Invalid rom");
            return;
        }

        var path = Path.Combine(Options.RomOutputPath, rom.SpoilerPath);
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
                logger.LogError(e, "Could not open spoiler file at {Path}", path);
                DisplayError($"Could not open spoiler file at {path}");
            }
        }
        else
        {
            DisplayError($"Could not find spoiler file at {path}");
        }
    }

    public void PlayRom(GeneratedRom? rom)
    {
        if (rom == null)
        {
            DisplayError("Invalid rom");
            return;
        }

        try
        {
            romLauncherService.LaunchRom(rom);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Could not launch rom");
            DisplayError(
                "There was an issue launching the rom. Make sure the rom file still exists and that you have a valid application set to launch roms in either the randomizer options or your operating system.");
        }

    }

    public void OpenFolder(GeneratedRom? rom)
    {
        if (rom == null)
        {
            DisplayError("Invalid rom");
            return;
        }

        if (!CrossPlatformTools.OpenDirectory(
                Path.Combine(Options.GeneralOptions.RomOutputPath, rom.RomPath), true))
        {
            DisplayError("Could not open rom folder");
        }
    }

    public async Task<TrackerWindow?> LaunchTrackerAsync(GeneratedRom? rom, bool force = false)
    {
        if (rom == null)
        {
            DisplayError("Invalid rom");
            return null;
        }

        metadataService.ReloadConfigs();

        if (!force && rom.GeneratorVersion != RandomizerVersion.MajorVersion)
        {
            var result = await MessageWindow.ShowYesNoDialog(
                "This seed was generated with a different version of the randomizer and may not work as expected. Do you want to continue?",
                "SMZ3 Cas' Randomizer", ParentWindow);
            if (!result)
            {
                return null;
            }
        }

        if (s_trackerWindow != null)
        {
            DisplayError("Tracker window already open");
            return null;
        }

        try
        {
            var scope = serviceProvider.CreateScope();
            var trackerOptionsAccessor = scope.ServiceProvider.GetRequiredService<TrackerOptionsAccessor>();
            trackerOptionsAccessor.Options = Options.GeneralOptions.GetTrackerOptions();

            var metadataService = scope.ServiceProvider.GetRequiredService<IMetadataService>();
            smz3GeneratedRomLoader.LoadGeneratedRom(rom, metadataService);

            s_trackerWindow = scope.ServiceProvider.GetRequiredService<TrackerWindow>();
            s_trackerWindow.Closed += (_, _) =>
            {
                s_trackerWindow = null;
                scope.Dispose();
            };
            s_trackerWindow.Rom = rom;
            s_trackerWindow.Show();
            return s_trackerWindow;
        }
        catch (YamlDotNet.Core.SemanticErrorException ex)
        {
            logger.LogError(ex, "A YAML parsing error occurred");
            DisplayError(ex.Message + "\n\n" + ex.InnerException?.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred when starting the tracker.");
            DisplayError("Unknown error opening tracker");
            return null;
        }
    }

    public async Task<TrackerWindow?> LaunchRom(GeneratedRom? rom)
    {
        if (rom == null)
        {
            DisplayError("Invalid rom");
            return null;
        }

        var launchButtonOptions = Options.GeneralOptions.LaunchButtonOption;
        TrackerWindow? trackerWindow = null;

        if (launchButtonOptions is LaunchButtonOptions.PlayAndTrack or LaunchButtonOptions.OpenFolderAndTrack or LaunchButtonOptions.TrackOnly)
        {
            trackerWindow = await LaunchTrackerAsync(rom);
        }

        if (launchButtonOptions is LaunchButtonOptions.OpenFolderAndTrack or LaunchButtonOptions.OpenFolderOnly)
        {
            OpenFolder(rom);
        }

        if (launchButtonOptions is LaunchButtonOptions.PlayAndTrack or LaunchButtonOptions.PlayOnly)
        {
            PlayRom(rom);
        }

        return trackerWindow;
    }

    public void CopyRomSeed(GeneratedRom? rom)
    {
        if (rom == null)
        {
            DisplayError("Invalid rom");
            return;
        }

        CopyTextToClipboard($"{rom.Seed}");
    }

    public void CopyRomConfigString(GeneratedRom? rom)
    {
        if (rom == null)
        {
            DisplayError("Invalid rom");
            return;
        }

        CopyTextToClipboard($"{rom.Settings}");
    }

    public bool DeleteRom(GeneratedRom? rom)
    {
        if (rom == null)
        {
            DisplayError("Invalid rom");
            return false;
        }

        gameDbService.DeleteGeneratedRom(rom, out var error);

        if (!string.IsNullOrEmpty(error))
        {
            DisplayError(error);
            return false;
        }

        return true;
    }

    public bool DeleteRom(MultiplayerGameDetails? details)
    {
        if (details == null)
        {
            DisplayError("Invalid rom");
            return false;
        }

        gameDbService.DeleteMultiplayerGame(details, out var error);

        if (!string.IsNullOrEmpty(error))
        {
            DisplayError(error);
            return false;
        }

        return true;
    }

    public async Task<RandomizerOptions?> OpenGenerationWindow(string? plandoConfig = null, bool isMultiplayer = false, ParsedRomDetails? importDetails = null)
    {
        LookupMsus();

        using var scope = serviceProvider.CreateScope();

        var window = scope.ServiceProvider.GetRequiredService<GenerationSettingsWindow>();

        if (plandoConfig != null && !window.LoadPlando(plandoConfig, out var error))
        {
            DisplayError(error ?? "Could not load plando file");
            return null;
        }
        else if (isMultiplayer)
        {
            window.EnableMultiplayerMode();
        }
        else if (importDetails != null)
        {
            window.EnableImportMode(importDetails);
        }

        await window.ShowDialog(ParentWindow);

        if (window.DialogResult)
        {
            return Options;
        }

        return null;
    }

    public void LookupMsus()
    {
        var msuDirectory = Options.GeneralOptions.MsuPath;
        if (string.IsNullOrEmpty(msuDirectory) || !Directory.Exists(msuDirectory) || msuLookupService.Status == MsuLoadStatus.Loading)
        {
            return;
        }

        ITaskService.Run(() =>
        {
            var smz3MsuType = msuTypeService.GetSMZ3MsuType() ?? throw new InvalidOperationException();
            msuLookupService.LookupMsus(msuDirectory, new Dictionary<string, string>()
            {
                { msuDirectory, smz3MsuType.DisplayName }
            });
        });
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

    private void CopyTextToClipboard(string text)
    {
        ParentWindow.Clipboard?.SetTextAsync(text);
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

        window.ShowDialog(ParentWindow);

        return window;
    }

    private Window ParentWindow
    {
        get
        {
            if (_parentWindow != null)
            {
                return _parentWindow;
            }
            else
            {
                _parentWindow = TopLevel.GetTopLevel(ParentControl) as Window ?? MessageWindow.GlobalParentWindow ?? throw new InvalidOperationException();

                if (_parentWindow.Owner is Window window)
                {
                    _parentWindow = window;
                }

                return _parentWindow;
            }
        }
    }
}
