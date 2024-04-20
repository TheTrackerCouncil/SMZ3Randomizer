using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Services;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.ViewModels;

namespace Randomizer.CrossPlatform.Views;

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
            _service = IControlServiceFactory.GetControlService<MultiRomListService>();
            _model = _service?.GetViewModel(this) ?? new MultiRomListViewModel();
        }

        InitializeComponent();
        DataContext = _model;
    }

    private void OpenFolderMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
        {
            return;
        }

        if (menuItem.Tag is not MultiplayerRomViewModel model)
        {
            return;
        }

        _service?.OpenFolder(model);
    }

    private void ViewSpoilerLogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void ViewProgressionLogMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
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
        _service?.OpenStatusWindow();
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

