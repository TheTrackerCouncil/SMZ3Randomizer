using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.Shared.Models;

namespace Randomizer.CrossPlatform.Views;

public partial class MultiplayerConnectWindow : ScalableWindow
{
    private MultiplayerConnectWindowViewModel _model;
    private MultiplayerConnectWindowService? _service;

    public MultiplayerConnectWindow() : this(true)
    {

    }

    public MultiplayerConnectWindow(bool isCreateWindow)
    {
        if (Design.IsDesignMode)
        {
            _model = new MultiplayerConnectWindowViewModel() { IsCreatingGame = isCreateWindow };
        }
        else
        {
            _service = IControlServiceFactory.GetControlService<MultiplayerConnectWindowService>();
            _model = _service?.GetViewModel(this, isCreateWindow) ??
                     new MultiplayerConnectWindowViewModel() { IsCreatingGame = isCreateWindow };
        }

        InitializeComponent();
        DataContext = _model;

        foreach (var server in _model.DefaultServers)
        {
            var menuItem = new MenuItem() { Header = server, Tag = server };
            menuItem.Click += (sender, args) =>
            {
                _model.Url = server;
            };
            ServerListContextMenu.Items.Add(menuItem);
        }
    }

    public void Close(bool dialogResult, MultiplayerGameDetails? multiplayerGameDetails)
    {
        DialogResult = dialogResult;
        MultiplayerGameDetails = multiplayerGameDetails;
        Dispatcher.UIThread.Invoke(Close);
    }

    public bool DialogResult { get; private set; }

    public MultiplayerGameDetails? MultiplayerGameDetails { get; private set; }

    private void NewGameButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.Connect();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ServerListButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ServerListButton.ContextMenu?.Open();
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _service?.Dispose();
    }
}

