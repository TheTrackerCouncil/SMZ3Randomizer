using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ReactiveUI.SourceGenerators;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class ProgressWindow : Window
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly DispatcherTimer? _timer;
    private DateTimeOffset? _shown;
    private ProgressWindowViewModel _model;

    public ProgressWindow()
    {
        DataContext = _model = new ProgressWindowViewModel("Generating stats...");
    }

    public ProgressWindow(Window owner, string message)
    {
        InitializeComponent();
        _timer = new(DispatcherPriority.Render);
        _timer.Tick += TimerOnTick;
        _timer.Interval = TimeSpan.FromSeconds(1);

        Owner = owner;

        DataContext = _model = new ProgressWindowViewModel(message);
    }

    public void StartTimer()
    {
        _shown = DateTimeOffset.Now;
        _timer?.Start();
        TimerOnTick(this, EventArgs.Empty);
    }

    public void Report(double value)
    {
        _model.Percentage = value;
    }

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    private void TimerOnTick(object? sender, EventArgs e)
    {
        var elapsed = DateTimeOffset.Now - _shown;
        TimeElapsedText.Text = $"{elapsed:m\\:ss}";
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        _cancellationTokenSource.Cancel();
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

public partial class ProgressWindowViewModel(string mainText) : ViewModelBase
{
    public string MainText => mainText;

    [Reactive] public partial string Elapsed { get; set; } = "0:00";

    [Reactive] public partial double Percentage { get; set; }
}

