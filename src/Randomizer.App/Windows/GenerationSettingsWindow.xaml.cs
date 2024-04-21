using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Randomizer.Data.Interfaces;
using Randomizer.Data.Options;
using Randomizer.Data.Services;
using Randomizer.Data.ViewModels;

namespace Randomizer.App.Windows;

public partial class GenerationSettingsWindow : Window
{
    private GenerationSettingsWindowService _service;
    private IServiceProvider _serviceProvider;
    private MsuUiService _msuUiService;
    private GenerationWindowViewModel _model = new();

    public GenerationSettingsWindow(GenerationSettingsWindowService service, IServiceProvider serviceProvider, MsuUiService msuUiService, OptionsFactory optionsFactory)
    {
        _service = service;
        _serviceProvider = serviceProvider;
        _msuUiService = msuUiService;
        optionsFactory.Create();
        InitializeComponent();
        DataContext = _model = _service.GetViewModel();
        BasicPanel.SetServices(serviceProvider, service);
        BasicPanel.DataContext = DataContext;
        ItemPanel.DataContext = DataContext;
    }

    public void SetPlandoConfig(PlandoConfig config)
    {
        _service.LoadPlando(config);
    }

    public void SetMultiplayerEnabled()
    {
        _model.IsMultiplayer = true;
    }

    private void GenerationSettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _msuUiService.LookupMsus();
    }

    private void GenerateStatsMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        _service.SaveSettings();
        var statGenerator = _serviceProvider.GetRequiredService<IStatGenerator>();
        var statWindow = new ProgressDialog(this, "Generating stats test");

        statGenerator.StatProgressUpdated += (o, args) =>
        {
            statWindow.Report(args.Current / (double)args.Total);
        };

        statGenerator.StatsCompleted += (o, args) =>
        {
            statWindow.Close();
            MessageBox.Show(Window.GetWindow(this)!, args.Message, "SMZ3 Cas’ Randomizer", MessageBoxButton.OK);
        };

        statGenerator.GenerateStatsAsync(5, _service.GetConfig(),
            statWindow.CancellationToken);
        statWindow.StartTimer();
        statWindow.ShowDialog();
    }

    private void SavePresetMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var window = new RandomizerPresetWindow();
        var result = window.ShowDialog();
        if (result == true)
        {
            if (!_service.CreatePreset(window.PresetName, out var error))
            {
                ShowErrorMessageBox(error ?? "Could not create preset");
            }
        }
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

        if (_model.IsMultiplayer)
        {
            DialogResult = true;
            Close();
            return;
        }

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

    private MessageBoxResult ShowErrorMessageBox(string message)
    {
        return MessageBox.Show(Window.GetWindow(this)!, message, "SMZ3 Cas’ Randomizer", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

