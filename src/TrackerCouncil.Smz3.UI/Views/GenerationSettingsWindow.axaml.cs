using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Controls;
using AvaloniaControls.Models;
using Microsoft.Extensions.DependencyInjection;
using TrackerCouncil.Smz3.Data.Interfaces;
using TrackerCouncil.Smz3.Data.ParsedRom;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class GenerationSettingsWindow : ScalableWindow
{
    private readonly GenerationSettingsWindowService? _generationSettingsWindowService;
    private readonly IServiceProvider? _serviceProvider;
    private readonly GenerationWindowViewModel? _model;

    public GenerationSettingsWindow()
    {
        InitializeComponent();
        DataContext = new GenerationWindowViewModel();
    }

    public GenerationSettingsWindow(GenerationSettingsWindowService generationSettingsWindowService, IServiceProvider serviceProvider)
    {
        _generationSettingsWindowService = generationSettingsWindowService;
        _serviceProvider = serviceProvider;
        InitializeComponent();
        DataContext = BasicPanel.Data = _model = _generationSettingsWindowService.GetViewModel();
        BasicPanel.SetServices(serviceProvider, generationSettingsWindowService);
    }

    public bool DialogResult { get; private set; }

    public GeneratedRom? GeneratedRom { get; private set; }

    public void EnableMultiplayerMode()
    {
        if (_model == null)
        {
            return;
        }
        _model.IsMultiplayer = true;
    }

    public void EnableImportMode(ParsedRomDetails importDetails)
    {
        if (_model == null)
        {
            return;
        }

        _model.IsImportMode = true;
        _model.ImportDetails = importDetails;
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

    private async void GenerateGameButton_OnClick(object sender, RoutedEventArgs e)
    {
        await GenerateRom();
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

            window.Closed += (_, _) =>
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

        statGenerator.StatProgressUpdated += (_, args) =>
        {
            statWindow.Report(args.Current / (double)args.Total);
        };

        statGenerator.StatsCompleted += (_, args) =>
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

        window.Closed += (_, _) =>
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

    private void TopLevel_OnOpened(object? sender, EventArgs e)
    {
        var val = Avalonia.VisualTree
            .VisualExtensions
            .GetTransformedBounds(this);

        // Fix window on smaller displays
        if (val is { Bounds.Height: < 750 })
        {
            Height = val.Value.Bounds.Height - 100;
            var grid = this.Find<Grid>("MainGrid")!;
            grid.IsVisible = false;
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(0.25));
                Dispatcher.UIThread.Invoke(() =>
                {
                    grid.IsVisible = true;
                });

            });
        }

        if (Position.Y is < 0 and > -200)
        {
            var newPos = new PixelPoint(Position.X, 0);
            Position = newPos;
        }
    }
}

