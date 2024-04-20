using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using AvaloniaControls.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.ViewModels;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.CrossPlatform.Views;

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


    private void QuickPlayButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = _service?.QuickPlay();
    }

    private void StartPlandoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = _service?.GeneratePlando();
    }

    private void GenerateRomButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _ = _service?.GenerateRom();
    }

    private void LaunchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<Button>(sender, out var rom, out _))
        {
            return;
        }

        _service?.LaunchRom(rom!);
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
        throw new NotImplementedException();
    }

    private void OpenSpoilerMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out var menuItem))
        {
            return;
        }

        _service?.OpenSpoilerLog(rom!);
    }

    private void OpenProgressionMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out var menuItem))
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
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out var menuItem))
        {
            return;
        }

        _service?.CopyRomSeed(rom!);
    }

    private void CopySettingsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out var menuItem))
        {
            return;
        }

        _service?.CopyRomConfigString(rom!);
    }

    private void DeleteRomMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!GetRomFromControl<MenuItem>(sender, out var rom, out var textBox))
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
}

