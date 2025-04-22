using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public enum SetupWindowCloseBehavior
{
    DoNothing,
    OpenSettingsWindow,
    OpenGenerationWindow
}

public enum SetupWindowStep
{
    Roms = 1,
    AutoTracker = 2,
    Tracker = 3,
    Finish = 4
}

public partial class SetupWindow : Window
{
    private SetupWindowViewModel _model;
    private SetupWindowService? _service;

    public SetupWindow()
    {
        if (Design.IsDesignMode)
        {
            _model = new SetupWindowViewModel();
        }
        else
        {
            _service = IControlServiceFactory.GetControlService<SetupWindowService>();
            _model = _service?.GetViewModel() ?? new SetupWindowViewModel();
        }

        InitializeComponent();
        DataContext = _model;
    }

    public async Task<T?> ShowDialog<T>(Window owner, SetupWindowStep step)
    {
        _model.StepNumber = (int)step;
        return await ShowDialog<T>(owner);
    }

    private void DubzLinkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://linktr.ee/mightydubz");
    }

    private void ChangePageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || !int.TryParse(button.Tag as string, out var page))
        {
            return;
        }

        _model.StepNumber = page;
    }

    private async void SelectRomsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null)
        {
            return;
        }

        var documentsFolder = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        var storageFiles = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select SNES ROM File(s)",
            FileTypeFilter = [ new FilePickerFileType("SNES ROM File (*.sfc") { Patterns = [ "*.sfc" ] }],
            SuggestedStartLocation = documentsFolder,
            AllowMultiple = true
        });

        var files = storageFiles
            .Select(f => f.TryGetLocalPath())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .NonNull();

        if (!_service.SetRomPaths(files))
        {
            await MessageWindow.ShowErrorDialog(
                "Invalid ROM detected. Ensure your ROM is for the correct region and is the correct version.", "Invalid ROM", this);
        }
    }

    private void OpenLuaFolderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        _service?.OpenLuaFolder();
    }

    private void OpenAutoTrackingWindowButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var window = new AutoTrackingHelpWindow();
        window.ShowDialog(this);
    }

    private void NextButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SaveSettings();

        if (_model.StepNumber < 4)
        {
            _model.StepNumber++;
        }
        else
        {
            Close();
        }
    }

    private void SkipButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _model.StepNumber++;
    }

    private void PrevButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _model.StepNumber--;
    }

    private void CloseAndOpenOptionsWindowButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SaveSettings();
        Close(SetupWindowCloseBehavior.OpenSettingsWindow);
    }

    private void CloseAndOpenGenerateWindowButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.SaveSettings();
        Close(SetupWindowCloseBehavior.OpenGenerationWindow);
    }

    private void TestAutoTrackerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _service?.TestAutoTracking();
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _service?.OnClose();
    }
}

