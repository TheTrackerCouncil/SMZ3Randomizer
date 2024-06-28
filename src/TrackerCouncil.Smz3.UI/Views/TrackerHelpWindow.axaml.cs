using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TrackerCouncil.Smz3.UI.Services;
using TrackerCouncil.Smz3.UI.ViewModels;

namespace TrackerCouncil.Smz3.UI.Views;

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

