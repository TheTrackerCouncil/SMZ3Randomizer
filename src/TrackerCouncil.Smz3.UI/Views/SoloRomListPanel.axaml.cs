using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using AvaloniaControls.Controls;
using AvaloniaControls.Services;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class SoloRomListPanel : UserControl
{
    private SoloRomListService? _service;
    private SoloRomListViewModel _model;

    public SoloRomListPanel()
    {
        if (Design.IsDesignMode)
        {
            _model = new SoloRomListViewModel();
        }
        else
        {
            _service = IControlServiceFactory.GetControlService<SoloRomListService>();
            _model = _service?.GetViewModel(this) ?? new SoloRomListViewModel();
        }

        InitializeComponent();
        DataContext = _model;
    }

    public void Reload()
    {
        _service?.UpdateList();
    }

    public async Task OpenGenerationWindow()
    {
        if (_service == null)
        {
            return;
        }
        await _service.GenerateRom();
    }

    private async void QuickPlayButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null)
        {
            return;
        }
        await _service.QuickPlay();
    }

    private async void StartPlandoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_service == null)
        {
            return;
        }
        await _service.GeneratePlando();
    }

    private void GenerateRomButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = OpenGenerationWindow();
    }

    private void LaunchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<Button>(sender, out var rom, out _))
        {
            return;
        }

        _ = _service?.LaunchRom(rom!);
    }

    private void PlayRomMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out _))
        {
            return;
        }

        _service?.PlayRom(rom!);
    }

    private void OpenFolderMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out _))
        {
            return;
        }

        _service?.OpenFolder(rom!);
    }

    private void OpenTrackerMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out _))
        {
            return;
        }

        _ = _service?.LaunchTracker(rom!);
    }

    private void OpenSpoilerMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out _))
        {
            return;
        }

        _service?.OpenSpoilerLog(rom!);
    }

    private void OpenProgressionMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out _))
        {
            return;
        }

        _service?.OpenProgressionHistory(rom!);
    }

    private void EditLabelMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out var menuItem))
        {
            return;
        }

        rom!.IsEditTextBoxVisible = true;
        var parent = menuItem!.Parent?.Parent?.Parent?.Parent as ListBoxItem;
        var grid = parent?.GetLogicalChildren().First() as Grid;
        var textBox = grid?.GetLogicalChildren().FirstOrDefault(x => x is TextBox) as TextBox;
        textBox?.Focus();
    }

    private void CopySeedMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out _))
        {
            return;
        }

        _service?.CopyRomSeed(rom!);
    }

    private void CopySettingsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out _))
        {
            return;
        }

        _service?.CopyRomConfigString(rom!);
    }

    private void DeleteRomMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out _))
        {
            return;
        }

        _service?.DeleteRom(rom!);
    }

    private void EditLabelTextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<TextBox>(sender, out var rom, out var textBox))
        {
            return;
        }

        _service?.UpdateRomLabel(rom!, textBox!.Text ?? "");
    }

    private void EditLabelTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return)
            return;

        if (!GetRomFromControl<TextBox>(sender, out var rom, out var textBox))
        {
            return;
        }

        _service?.UpdateRomLabel(rom!, textBox!.Text ?? "");
    }

    private bool GetRomFromControl<T>(object? control, out GeneratedRomViewModel? model, out T? castedControl) where T : Control
    {
        castedControl = control as T;
        if (castedControl == null)
        {
            model = null;
            return false;
        }

        if (castedControl.Tag is not GeneratedRomViewModel rom)
        {
            model = null;
            return false;
        }

        model = rom;
        return true;
    }

    private async void ArchipelagoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_service == null) return;
            await _service.OpenArchipelagoModeAsync();
        }
        catch
        {
            await MessageWindow.ShowErrorDialog("Invalid ROM file. Please select a valid generated SMZ3 rom.");
        }
    }
}

