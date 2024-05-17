using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaControls.Extensions;
using Randomizer.CrossPlatform.Services;
using Randomizer.CrossPlatform.ViewModels;

namespace Randomizer.CrossPlatform.Views;

public partial class TrackerHelpWindow : Window
{
    public TrackerHelpWindow()
    {
        InitializeComponent();
        DataContext = new TrackerHelpWindowViewModel
        {
            SpeechRecognitionSyntax = new List<TrackerHelpWindowSyntaxItem>
            {
                new() { Key = "Test", Values = ["Option 1", "Option 2"] },
            }
        };
    }

    public TrackerHelpWindow(TrackerHelpWindowService service)
    {
        InitializeComponent();
        DataContext = service.GetViewModel();
    }

    private void OkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

