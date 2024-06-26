using System.Collections.Generic;
using System.Windows;
using TrackerCouncil.Smz3.Abstractions;

namespace TrackerCouncil.Smz3.UI.Legacy.Windows;

/// <summary>
/// Interaction logic for TrackerHelpWindow.xaml
/// </summary>
public partial class TrackerHelpWindow : Window
{
    public TrackerHelpWindow(TrackerBase tracker)
    {
        SpeechRecognitionSyntax = tracker.Syntax;

        InitializeComponent();
    }

    public IReadOnlyDictionary<string, IEnumerable<string>> SpeechRecognitionSyntax { get; }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
