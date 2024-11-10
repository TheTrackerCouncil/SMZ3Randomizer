using System;
using System.IO;
using System.Linq;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using Microsoft.Extensions.DependencyInjection;
using MSURandomizer.Views;
using MSURandomizerLibrary;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.ViewModels;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class GenerationSettingsBasicPanel : UserControl
{
    private IServiceProvider? _serviceProvider;
    private GenerationSettingsWindowService? _generationSettingsWindowService;
    private RandomizerOptions? _options;

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
        _options = _serviceProvider.GetRequiredService<OptionsFactory>().Create();
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
        if (_serviceProvider == null || _generationSettingsWindowService == null)
        {
            return;
        }

        if (!_generationSettingsWindowService.IsUserMsuPathValid)
        {
            var messageWindow = new MessageWindow(new MessageWindowRequest()
            {
                Message = "No parent MSU folder is currently specified. Would you like to specify a folder where all of your MSUs are located now?",
                Title = "SMZ3 Cas' Randomizer",
                Buttons = MessageWindowButtons.YesNo,
                Icon = MessageWindowIcon.Info
            });

            messageWindow.Closed += (sender, args) =>
            {
                if (messageWindow.DialogResult?.PressedAcceptButton != true)
                {
                    return;
                }

                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var storagePath = await CrossPlatformTools.OpenFileDialogAsync(ParentWindow, FileInputControlType.Folder, "", _options?.GeneralOptions.RomOutputPath,
                        title: "Select parent MSU folder");

                    var path =  Uri.UnescapeDataString(storagePath?.Path.AbsolutePath ?? "");

                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    {
                        _generationSettingsWindowService.UpdateUserMsuPath(path);
                        OpenMsuSelectionWindow(randomizationStyle);
                    }
                });
            };

            _ = messageWindow.ShowDialog(ParentWindow);

            return;
        }

        var window = _serviceProvider.GetRequiredService<MsuWindow>();
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        window.Closed += (sender, args) =>
        {
            if (!window.DialogResult)
            {
                return;
            }

            _generationSettingsWindowService.SetMsuPaths(window.GetSelectedMsus().ToList(), randomizationStyle);
        };

        window.ShowDialog(ParentWindow, randomizationStyle == null, _generationSettingsWindowService.GetMsuDirectory());
    }

    private Window ParentWindow => (Window)TopLevel.GetTopLevel(this)!;

    private void ImportSeedButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_generationSettingsWindowService == null)
        {
            return;
        }

        if (_generationSettingsWindowService.VerifyConfigVersion(Data.Basic.ImportString, out var versionMismatch))
        {
            _generationSettingsWindowService?.ApplyConfig(Data.Basic.ImportString, true);
        }
        else if (versionMismatch)
        {
            var messageWindow = new MessageWindow(new MessageWindowRequest()
            {
                Message = "The randomizer version of the import settings string does not match the current application's version. Using the seed number in this version may produce different results from the original seed. Do you want to continue?",
                Title = "SMZ3 Cas' Randomizer",
                Buttons = MessageWindowButtons.YesNo,
                Icon = MessageWindowIcon.Warning
            });

            messageWindow.Closed += (o, args) =>
            {
                if (messageWindow.DialogResult?.PressedAcceptButton == true)
                {
                    _generationSettingsWindowService?.ApplyConfig(Data.Basic.ImportString, true);
                }
            };

            messageWindow.ShowDialog(ParentWindow);
        }

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

