using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class MultiRomListPanel : UserControl
{
    private MultiRomListService? _service;
    private MultiRomListViewModel _model;

    public MultiRomListPanel()
    {
        if (Design.IsDesignMode)
        {
            _model = new MultiRomListViewModel();
        }
        else
        {
            try
            {
                _service = IControlServiceFactory.GetControlService<MultiRomListService>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            _model = _service?.GetViewModel(this) ?? new MultiRomListViewModel();
        }

        InitializeComponent();
        DataContext = _model;
    }

    public void Reload()
    {
        _service?.UpdateList();
    }

    private void OpenFolderMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: MultiplayerRomViewModel model } )
        {
            return;
        }

        _service?.OpenFolder(model);
    }

    private void ViewSpoilerLogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: MultiplayerRomViewModel model } )
        {
            return;
        }

        _service?.OpenSpoilerLog(model);
    }

    private void ViewProgressionLogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: MultiplayerRomViewModel model } )
        {
            return;
        }

        _service?.OpenProgressionHistory(model);
    }

    private void DeleteMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
        {
            return;
        }

        if (menuItem.Tag is not MultiplayerRomViewModel model)
        {
            return;
        }

        _service?.DeleteRom(model);
    }

    private void ReconnectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        if (button.Tag is not MultiplayerRomViewModel model)
        {
            return;
        }

        _service?.OpenStatusWindow(model);
    }

    private void CreateMultiGameButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.OpenConnectWindow(true);
    }

    private void JoinMultiGameButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.OpenConnectWindow(false);
    }
}

