using System;
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

    public void OpenStatusWindow()
    {
        var window = new MultiplayerStatusWindow();
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
        OpenStatusWindow();
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
