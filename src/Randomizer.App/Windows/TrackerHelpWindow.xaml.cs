using System;
using System.Collections.Generic;
using System.Windows;
using Randomizer.Abstractions;
using Randomizer.SMZ3.Tracking;

namespace Randomizer.App
{
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
}
