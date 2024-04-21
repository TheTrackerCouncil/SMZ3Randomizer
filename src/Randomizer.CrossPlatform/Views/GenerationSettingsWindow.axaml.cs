using System;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using Microsoft.Extensions.DependencyInjection;
using Randomizer.Data.Interfaces;
using Randomizer.Data.Services;
using Randomizer.Data.ViewModels;
using Randomizer.Shared.Models;

namespace Randomizer.CrossPlatform.Views;

public partial class GenerationSettingsWindow : ScalableWindow
{
    private GenerationSettingsWindowService? _generationSettingsWindowService;
    private IServiceProvider? _serviceProvider;
    private IStatGenerator? _statGenerator;
    private GenerationWindowViewModel? _model;

    public GenerationSettingsWindow()
    {
        InitializeComponent();
        DataContext = new GenerationWindowViewModel();
    }

    public GenerationSettingsWindow(GenerationSettingsWindowService generationSettingsWindowService, IServiceProvider serviceProvider, IStatGenerator statGenerator)
    {
        _generationSettingsWindowService = generationSettingsWindowService;
        _serviceProvider = serviceProvider;
        _statGenerator = statGenerator;
        InitializeComponent();
        DataContext = BasicPanel.Data = _model = _generationSettingsWindowService.GetViewModel();
        BasicPanel.SetServices(serviceProvider, generationSettingsWindowService);
    }

    public bool DialogResult { get; private set; }

    public GeneratedRom? GeneratedRom { get; private set; }

    public void EnableMultiplayerMode()
    {
        _model?.SetMultiplayerEnabled();
    }

    public bool LoadPlando(string file, out string? error)
    {
        error = null;

        if (_generationSettingsWindowService == null)
        {
            return false;
        }

        if (!_generationSettingsWindowService.LoadPlando(file, out error))
        {
            return false;
        }

        return true;
    }

    private void GenerateMenuButton_OnClick(object sender, RoutedEventArgs e)
    {
        GenerateMenuButton.ContextMenu?.Open();
    }

    private void GenerateGameButton_OnClick(object sender, RoutedEventArgs e)
    {
        _ = GenerateRom();
    }

    private async Task GenerateRom()
    {
        if (_generationSettingsWindowService == null)
        {
            return;
        }

        _generationSettingsWindowService.SaveSettings();

        if (_model?.IsMultiplayer == true)
        {
            DialogResult = true;
            Close();
            return;
        }

        var generatedRom = await _generationSettingsWindowService.GenerateRom();

        if (!string.IsNullOrEmpty(generatedRom.GenerationError))
        {
            var window = new MessageWindow(new MessageWindowRequest()
            {
                Message = generatedRom.GenerationError,
                Title = "SMZ3 Cas' Randomizer",
                Buttons = MessageWindowButtons.OK,
                Icon = generatedRom.Rom == null ? MessageWindowIcon.Error : MessageWindowIcon.Warning
            });

            window.ShowDialog();

            window.Closed += (sender, args) =>
            {
                if (generatedRom.Rom != null)
                {
                    DialogResult = true;
                    GeneratedRom = generatedRom.Rom;
                    Close();
                }
            };
        }

        if (generatedRom.Rom != null)
        {
            DialogResult = true;
            GeneratedRom = generatedRom.Rom;
            Close();
        }
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void GenerateStatsMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_generationSettingsWindowService == null || _serviceProvider == null)
        {
            return;
        }
        _generationSettingsWindowService.SaveSettings();
        var statGenerator = _serviceProvider.GetRequiredService<IStatGenerator>();
        var statWindow = new ProgressWindow(this, "Generating stats test");

        statGenerator.StatProgressUpdated += (o, args) =>
        {
            statWindow.Report(args.Current / (double)args.Total);
        };

        statGenerator.StatsCompleted += (o, args) =>
        {
            statWindow.Close();
            var messageWindow = new MessageWindow(new MessageWindowRequest()
            {
                Message = args.Message, Buttons = MessageWindowButtons.OK, Title = "SMZ3 Cas' Randomizer"
            });
            messageWindow.ShowDialog(this);
        };

        statWindow.ShowDialog(this);
        statWindow.StartTimer();

        statGenerator.GenerateStatsAsync(5, _generationSettingsWindowService.GetConfig(),
            statWindow.CancellationToken);
    }


    private void SavePresetMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_generationSettingsWindowService == null)
        {
            return;
        }

        var window = new MessageWindow(new MessageWindowRequest()
        {
            Message = "Enter a name for the preset",
            Title = "SMZ3 Cas' Randomizer",
            Buttons = MessageWindowButtons.OK,
            DisplayTextBox = true,
        });

        window.Closed += (o, args) =>
        {
            if (window.DialogResult?.PressedAcceptButton != true)
            {
                return;
            }

            if (string.IsNullOrEmpty(window.DialogResult.ResponseText))
            {
                new MessageWindow(new MessageWindowRequest()
                {
                    Message = "You did not enter a preset name",
                    Title = "SMZ3 Cas' Randomizer",
                    Buttons = MessageWindowButtons.OK,
                    Icon = MessageWindowIcon.Error
                }).ShowDialog(this);

                return;
            }

            if (_generationSettingsWindowService.CreatePreset(window.DialogResult.ResponseText, out var error) == false)
            {
                new MessageWindow(new MessageWindowRequest()
                {
                    Message = error ?? "Unable to create preset",
                    Title = "SMZ3 Cas' Randomizer",
                    Buttons = MessageWindowButtons.OK,
                    Icon = MessageWindowIcon.Error
                }).ShowDialog(this);
            }
        };

        window.ShowDialog(this);
    }

    private void BasicPanel_OnDisplayed(object? sender, EventArgs e)
    {
        _generationSettingsWindowService?.UpdateSummaryText();
    }
}

