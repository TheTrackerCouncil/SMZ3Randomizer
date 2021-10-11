using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Randomizer.SMZ3.Tracking;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for TrackerWindow.xaml
    /// </summary>
    public partial class TrackerWindow : Window
    {
        private Tracker _tracker;

        public TrackerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _tracker = new Tracker(Log);
            _tracker.StartTracking();
        }

        protected virtual void Log(string text)
        {
            Dispatcher.Invoke(() =>
            {
                TrackerDebugOutput.AppendText(text + Environment.NewLine);
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _tracker.StopTracking();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _tracker?.Dispose();
        }
    }
}
