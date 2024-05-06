using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizer.Views;
using MSURandomizerLibrary;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.ViewModels;

namespace Randomizer.CrossPlatform.Views;

public partial class GenerationSettingsBasicPanel : UserControl
{
    private IServiceProvider? _serviceProvider;
    private GenerationSettingsWindowService? _generationSettingsWindowService;

    public GenerationSettingsBasicPanel()
    {
        InitializeComponent();
    }

    public void SetServices(IServiceProvider serviceProvider,
        GenerationSettingsWindowService generationSettingsWindowService)
    {
        _serviceProvider = serviceProvider;
        _generationSettingsWindowService = generationSettingsWindowService;
        _generationSettingsWindowService.ConfigError += GenerationSettingsWindowServiceOnConfigError;
    }

    private void GenerationSettingsWindowServiceOnConfigError(object? sender, EventArgs e)
    {
        new MessageWindow(new MessageWindowRequest()
        {
            Message = "Unable to parse randomizer settings string",
            Title = "SMZ3 Cas' Randomizer",
            Buttons = MessageWindowButtons.OK,
            Icon = MessageWindowIcon.Error
        }).ShowDialog(ParentWindow);
    }

    public static readonly StyledProperty<GenerationWindowViewModel> DataProperty = AvaloniaProperty.Register<GenerationSettingsItemPanel, GenerationWindowViewModel>(
        nameof(Data));

    public GenerationWindowViewModel Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public event EventHandler? Displayed;

    private async void LinkSpriteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_serviceProvider == null || _generationSettingsWindowService == null)
        {
            return;
        }
        var spriteWindow = _serviceProvider.GetRequiredService<SpriteWindow>();
        spriteWindow.SetSpriteType(SpriteType.Link);
        await spriteWindow.ShowDialog((GenerationSettingsWindow)TopLevel.GetTopLevel(this)!);
        var result = spriteWindow.DialogResult;
        _generationSettingsWindowService.SaveSpriteSettings(result == true, spriteWindow.SelectedSprite ?? Sprite.DefaultLink,
            spriteWindow.SelectedSpriteOptions, spriteWindow.Model.SearchText, spriteWindow.Model.SpriteFilter);
    }

    private async void SamusSpriteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_serviceProvider == null || _generationSettingsWindowService == null)
        {
            return;
        }
        var spriteWindow = _serviceProvider.GetRequiredService<SpriteWindow>();
        spriteWindow.SetSpriteType(SpriteType.Samus);
        await spriteWindow.ShowDialog((GenerationSettingsWindow)TopLevel.GetTopLevel(this)!);
        var result = spriteWindow.DialogResult;
        _generationSettingsWindowService.SaveSpriteSettings(result == true, spriteWindow.SelectedSprite ?? Sprite.DefaultSamus,
            spriteWindow.SelectedSpriteOptions, spriteWindow.Model.SearchText, spriteWindow.Model.SpriteFilter);
    }

    private async void ShipSpriteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_serviceProvider == null || _generationSettingsWindowService == null)
        {
            return;
        }
        var spriteWindow = _serviceProvider.GetRequiredService<SpriteWindow>();
        spriteWindow.SetSpriteType(SpriteType.Ship);
        await spriteWindow.ShowDialog((GenerationSettingsWindow)TopLevel.GetTopLevel(this)!);
        var result = spriteWindow.DialogResult;
        _generationSettingsWindowService.SaveSpriteSettings(result == true, spriteWindow.SelectedSprite ?? Sprite.DefaultShip,
            spriteWindow.SelectedSpriteOptions, spriteWindow.Model.SearchText, spriteWindow.Model.SpriteFilter);
    }

    private void SelectMsuButton_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenMsuSelectionWindow(null);
    }

    private void MsuOptionsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        MsuOptionsButton.ContextMenu?.Open();
    }

    private void RandomMsuMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenMsuSelectionWindow(MsuRandomizationStyle.Single);
    }

    private void ShuffledMsuMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenMsuSelectionWindow(MsuRandomizationStyle.Shuffled);
    }

    private void ContinuousShuffleMsuMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenMsuSelectionWindow(MsuRandomizationStyle.Continuous);
    }

    private async void SelectMsuFileMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var options = _serviceProvider?.GetRequiredService<OptionsFactory>().Create();
        var path = options?.PatchOptions.MsuPaths.FirstOrDefault() ?? options?.GeneralOptions.MsuPath;
        var storagePath = await CrossPlatformTools.OpenFileDialogAsync(ParentWindow, FileInputControlType.OpenFile, "MSU files (*.msu)|*.msu|All files (*.*)|*.*", path);

        var pathString = HttpUtility.UrlDecode(storagePath?.Path.AbsolutePath);
        if (string.IsNullOrEmpty(pathString) || !File.Exists(pathString))
        {
            return;
        }

        _generationSettingsWindowService?.SetMsuPaths([pathString], null);
    }

    private void VanillaMusicMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        _generationSettingsWindowService?.SetMsuPaths([], null);
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        DataContext = Data;
        Displayed?.Invoke(this, EventArgs.Empty);
    }

    private void OpenMsuSelectionWindow(MsuRandomizationStyle? randomizationStyle)
    {
        if (_serviceProvider == null)
        {
            return;
        }

        var window = _serviceProvider.GetRequiredService<MsuWindow>();

        window.Closed += (sender, args) =>
        {
            if (!window.DialogResult)
            {
                return;
            }

            _generationSettingsWindowService?.SetMsuPaths(window.GetSelectedMsus().ToList(), randomizationStyle);
        };

        window.ShowDialog((Window)TopLevel.GetTopLevel(this)!, randomizationStyle == null, _generationSettingsWindowService?.GetMsuDirectory());
    }

    private Window ParentWindow => (Window)TopLevel.GetTopLevel(this)!;

    private void ImportSeedButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _generationSettingsWindowService?.ApplyConfig(Data.Basic.ImportString, true);
    }

    private void ImportSettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _generationSettingsWindowService?.ApplyConfig(Data.Basic.ImportString, false);
    }

    private void ClearSeedButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Data.Basic.Seed = "";
    }

    private void ApplyPresetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_generationSettingsWindowService?.ApplySelectedPreset() == false)
        {
            new MessageWindow(new MessageWindowRequest()
            {
                Message = "Unable to apply preset",
                Title = "SMZ3 Cas' Randomizer",
                Buttons = MessageWindowButtons.OK,
                Icon = MessageWindowIcon.Error
            }).ShowDialog(ParentWindow);
        }
    }

    private void DeletePresetButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_generationSettingsWindowService?.DeleteSelectedPreset(out var error) == false)
        {
            new MessageWindow(new MessageWindowRequest()
            {
                Message = error ?? "Unable to delete preset",
                Title = "SMZ3 Cas' Randomizer",
                Buttons = MessageWindowButtons.OK,
                Icon = MessageWindowIcon.Error
            }).ShowDialog(ParentWindow);
        }
    }
}

