using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MSURandomizerLibrary;
using TrackerCouncil.Smz3.Data.Options;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.UI.Legacy.Windows;

namespace TrackerCouncil.Smz3.UI.Legacy.Controls;

public partial class GameSettingsBasicPanel : UserControl
{
    private IServiceProvider? _serviceProvider;
    private GenerationSettingsWindowService? _service;
    private MsuUiService? _msuUiService;

    public GameSettingsBasicPanel()
    {
        InitializeComponent();
    }

    public void SetServices(IServiceProvider serviceProvider,
        GenerationSettingsWindowService generationSettingsWindowService)
    {
        _serviceProvider = serviceProvider;
        _service = generationSettingsWindowService;
        _service.ConfigError += ServiceOnConfigError;
        _msuUiService = _serviceProvider.GetRequiredService<MsuUiService>();
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data),
            propertyType: typeof(GenerationWindowViewModel),
            ownerType: typeof(GameSettingsBasicPanel),
            typeMetadata: new PropertyMetadata());

    public GenerationWindowViewModel Data
    {
        get { return (GenerationWindowViewModel)GetValue(DataProperty); }
        set { SetValue(DataProperty, value); }
    }

    private void ServiceOnConfigError(object? sender, EventArgs e)
    {
        ShowErrorMessageBox("Unable to parse randomizer settings string");
    }

    private void LinkSpriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_serviceProvider == null || _service == null)
        {
            return;
        }

        var spriteWindow = _serviceProvider.GetRequiredService<SpriteWindow>();
        spriteWindow.SetSpriteType(SpriteType.Link);
        var result = spriteWindow.ShowDialog();
        _service.SaveSpriteSettings(result == true, spriteWindow.SelectedSprite ?? Sprite.DefaultLink,
            spriteWindow.SelectedSpriteOptions, spriteWindow.Model.SearchText, spriteWindow.Model.SpriteFilter);
    }

    private void SamusSpriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_serviceProvider == null || _service == null)
        {
            return;
        }

        var spriteWindow = _serviceProvider.GetRequiredService<SpriteWindow>();
        spriteWindow.SetSpriteType(SpriteType.Samus);
        var result = spriteWindow.ShowDialog();
        _service.SaveSpriteSettings(result == true, spriteWindow.SelectedSprite ?? Sprite.DefaultSamus,
            spriteWindow.SelectedSpriteOptions, spriteWindow.Model.SearchText, spriteWindow.Model.SpriteFilter);
    }

    private void ShipSpriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_serviceProvider == null || _service == null)
        {
            return;
        }

        var spriteWindow = _serviceProvider.GetRequiredService<SpriteWindow>();
        spriteWindow.SetSpriteType(SpriteType.Ship);
        var result = spriteWindow.ShowDialog();
        _service.SaveSpriteSettings(result == true, spriteWindow.SelectedSprite ?? Sprite.DefaultShip,
            spriteWindow.SelectedSpriteOptions, spriteWindow.Model.SearchText, spriteWindow.Model.SpriteFilter);
    }

    private void SelectMsuButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_msuUiService == null || _service == null)
        {
            return;
        }

        if (_msuUiService.OpenMsuWindow(Window.GetWindow(this)!, SelectionMode.Single, null))
        {
            Data.Basic.MsuRandomizationStyle = null;
        }

        _service.UpdateMsuText();
    }

    private void MsuOptionsButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_msuUiService == null || _service == null)
        {
            return;
        }

        if (MsuOptionsButton.ContextMenu == null) return;
        MsuOptionsButton.ContextMenu.IsOpen = true;
    }

    private void RandomMsuMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_msuUiService == null || _service == null)
        {
            return;
        }

        if (_msuUiService.OpenMsuWindow(Window.GetWindow(this)!, SelectionMode.Multiple, MsuRandomizationStyle.Single))
        {
            Data.Basic.MsuRandomizationStyle = MsuRandomizationStyle.Single;
        }
        _service.UpdateMsuText();
    }

    private void ShuffledMsuMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_msuUiService == null || _service == null)
        {
            return;
        }

        if (_msuUiService.OpenMsuWindow(Window.GetWindow(this)!, SelectionMode.Multiple,
                MsuRandomizationStyle.Shuffled))
        {
            Data.Basic.MsuRandomizationStyle = MsuRandomizationStyle.Shuffled;
        }
        _service.UpdateMsuText();
    }

    private void ContinuousShuffleMsuMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_msuUiService == null || _service == null)
        {
            return;
        }

        if (_msuUiService.OpenMsuWindow(Window.GetWindow(this)!, SelectionMode.Multiple,
                MsuRandomizationStyle.Continuous))
        {
            Data.Basic.MsuRandomizationStyle = MsuRandomizationStyle.Continuous;
        }
        _service.UpdateMsuText();
    }

    private void SelectMsuFileMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (_serviceProvider == null || _service == null)
        {
            return;
        }

        var options = _serviceProvider.GetRequiredService<OptionsFactory>().Create();

        var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = "MSU-1 files (*.msu)|*.msu|All files (*.*)|*.*",
            Title = "Select MSU-1 file",
            InitialDirectory = Directory.Exists(options.GeneralOptions.MsuPath) ? options.GeneralOptions.MsuPath : null
        };

        if (dialog.ShowDialog(Window.GetWindow(this)!) == true && File.Exists(dialog.FileName))
        {
            _service.SetMsuPath(dialog.FileName);
            Data.Basic.MsuRandomizationStyle = null;
        }
    }

    private void VanillaMusicMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _service?.ClearMsu();
    }

    private void ApplyPresetButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_service?.ApplySelectedPreset() == false)
        {
            ShowErrorMessageBox("Unable to apply preset");
        }
    }

    private void DeletePresetButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_service?.DeleteSelectedPreset(out var error) == false)
        {
            ShowErrorMessageBox(error ?? "Unable to delete preset");
        }
    }

    private void ImportSeedButton_OnClick(object sender, RoutedEventArgs e)
    {
        _service?.ApplyConfig(Data.Basic.ImportString, true);
    }

    private void ImportSettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        _service?.ApplyConfig(Data.Basic.ImportString, false);
    }

    private void ClearSeedButton_OnClick(object sender, RoutedEventArgs e)
    {
        Data.Basic.Seed = "";
    }

    protected MessageBoxResult ShowErrorMessageBox(string message)
    {
        return MessageBox.Show(Window.GetWindow(this)!, message, "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void GameSettingsBasicPanel_OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = Data;
        _service?.UpdateSummaryText();
    }

    private void ImportStringTextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Data.Basic.ImportString = ImportStringTextBox.Text;
    }

    private void SeedTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Data.Basic.Seed = SeedTextBox.Text;
    }
}

