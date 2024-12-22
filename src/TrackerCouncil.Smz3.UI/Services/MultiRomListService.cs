using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaControls.Controls;
using AvaloniaControls.ControlServices;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.UI.ViewModels;
using TrackerCouncil.Smz3.UI.Views;

namespace TrackerCouncil.Smz3.UI.Services;

public class MultiRomListService(IGameDbService gameDbService, SharedCrossplatformService sharedCrossplatformService, OptionsFactory options) : ControlService
{
    private MultiRomListViewModel _model = new();
    private MultiRomListPanel _panel = null!;
    private Window? _parentWindow;

    public MultiRomListViewModel GetViewModel(MultiRomListPanel panel)
    {
        _panel = panel;
        sharedCrossplatformService.ParentControl = _panel;
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
        var window = new MultiplayerStatusWindow(model);
        window.Show(ParentWindow);
    }

    public async Task OpenConnectWindow(bool isCreate)
    {
        if (!options.Create().GeneralOptions.Validate())
        {
            await MessageWindow.ShowErrorDialog("You must select the A Link to the Past and Super Metroid roms in the Options window before creating or joining multiplayer games.");
            return;
        }

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
        if (sharedCrossplatformService.DeleteRom(rom.Details))
        {
            UpdateList();
        }
    }

    public void OpenFolder(MultiplayerRomViewModel rom)
    {
        sharedCrossplatformService.OpenFolder(rom.Details.GeneratedRom);
    }

    public void OpenSpoilerLog(MultiplayerRomViewModel rom)
    {
        sharedCrossplatformService.OpenSpoilerLog(rom.Details.GeneratedRom);
    }

    public void OpenProgressionHistory(MultiplayerRomViewModel rom)
    {
        sharedCrossplatformService.OpenProgressionHistory(rom.Details.GeneratedRom);
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
