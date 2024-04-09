using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MSURandomizerLibrary;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.ViewModels;

namespace Randomizer.App.Windows;

public partial class GenerationSettingsWindow : Window
{
    private GenerationSettingsWindowService _service;
    private IServiceProvider _serviceProvider;
    private MsuUiService _msuUiService;
    private RandomizerOptions _options;
    private GenerationWindowViewModel _model;

    public GenerationSettingsWindow(GenerationSettingsWindowService service, IServiceProvider serviceProvider, MsuUiService msuUiService, OptionsFactory optionsFactory)
    {
        _service = service;
        _serviceProvider = serviceProvider;
        _msuUiService = msuUiService;
        _options = optionsFactory.Create();
        InitializeComponent();
        DataContext = _service.GetViewModel();
    }

    public void SetPlandoConfig(PlandoConfig config)
    {
        _model.SetPlandoConfig(config);
    }

    public void SetMultiplayerEnabled()
    {
        _model.SetMultiplayerEnabled();
    }

    private void LinkSpriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var spriteWindow = _serviceProvider.GetRequiredService<SpriteWindow>();
        spriteWindow.SetSpriteType(SpriteType.Link);
        var result = spriteWindow.ShowDialog();
        _service.SaveSpriteSettings(result == true, spriteWindow.SelectedSprite ?? Sprite.DefaultLink,
            spriteWindow.SelectedSpriteOptions, spriteWindow.Model.SearchText, spriteWindow.Model.SpriteFilter);
    }

    private void SamusSpriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var spriteWindow = _serviceProvider.GetRequiredService<SpriteWindow>();
        spriteWindow.SetSpriteType(SpriteType.Samus);
        var result = spriteWindow.ShowDialog();
        _service.SaveSpriteSettings(result == true, spriteWindow.SelectedSprite ?? Sprite.DefaultSamus,
            spriteWindow.SelectedSpriteOptions, spriteWindow.Model.SearchText, spriteWindow.Model.SpriteFilter);
    }

    private void ShipSpriteButton_OnClick(object sender, RoutedEventArgs e)
    {
        var spriteWindow = _serviceProvider.GetRequiredService<SpriteWindow>();
        spriteWindow.SetSpriteType(SpriteType.Ship);
        var result = spriteWindow.ShowDialog();
        _service.SaveSpriteSettings(result == true, spriteWindow.SelectedSprite ?? Sprite.DefaultShip,
            spriteWindow.SelectedSpriteOptions, spriteWindow.Model.SearchText, spriteWindow.Model.SpriteFilter);
    }

    private void SelectMsuButton_OnClick(object sender, RoutedEventArgs e)
    {
        _msuUiService.OpenMsuWindow(this, SelectionMode.Single, null);
        _service.UpdateMsuText();
    }

    private void MsuOptionsButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (MsuOptionsButton.ContextMenu == null) return;
        MsuOptionsButton.ContextMenu.IsOpen = true;
    }

    private void RandomMsuMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _msuUiService.OpenMsuWindow(this, SelectionMode.Multiple, MsuRandomizationStyle.Single);
        _service.UpdateMsuText();
    }

    private void ShuffledMsuMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _msuUiService.OpenMsuWindow(this, SelectionMode.Multiple, MsuRandomizationStyle.Shuffled);
        _service.UpdateMsuText();
    }

    private void ContinuousShuffleMsuMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _msuUiService.OpenMsuWindow(this, SelectionMode.Multiple, MsuRandomizationStyle.Continuous);
        _service.UpdateMsuText();
    }

    private void GenerationSettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _msuUiService.LookupMsus();
    }

    private void SelectMsuFileMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = "MSU-1 files (*.msu)|*.msu|All files (*.*)|*.*",
            Title = "Select MSU-1 file",
            InitialDirectory = Directory.Exists(_options.GeneralOptions.MsuPath) ? _options.GeneralOptions.MsuPath : null
        };

        if (dialog.ShowDialog(this) == true && File.Exists(dialog.FileName))
        {
            _service.SetMsuPath(dialog.FileName);
        }
    }

    private void VanillaMusicMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _service.ClearMsu();
    }

    private void GenerateStatsMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void SavePresetMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void GenerateMenuButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (GenerateMenuButton.ContextMenu == null) return;
        GenerateMenuButton.ContextMenu.IsOpen = true;
    }

    private void GenerateGameButton_OnClick(object sender, RoutedEventArgs e)
    {
        _ = GenerateRom();
    }

    private async Task GenerateRom()
    {
        _service.SaveSettings();
        var generatedRom = await _service.GenerateRom();

        if (!string.IsNullOrEmpty(generatedRom.GenerationError))
        {
            MessageBox.Show(this, generatedRom.GenerationError, "SMZ3 Cas' Randomizer", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        if (generatedRom.Rom != null)
        {
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

