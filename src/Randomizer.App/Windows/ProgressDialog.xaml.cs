using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Windows.Threading;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    [NotAService]
    public partial class ProgressDialog : Window, IProgress<double>
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly DispatcherTimer _timer;
        private DateTimeOffset? _shown;

        public ProgressDialog(Window owner, string title)
        {
            InitializeComponent();

            _timer = new(DispatcherPriority.Render);
            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(1);

            Owner = owner;
            Title = owner.Title;

            MainInstructionText.Text = title;
            MainProgressBar.Minimum = 0d;
            MainProgressBar.Maximum = 1d;

            TaskbarItemInfo = new TaskbarItemInfo()
            {
                
            };
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var elapsed = DateTimeOffset.Now - _shown;
            TimeElapsedText.Text = $"{elapsed:m\\:ss}";
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public void StartTimer()
        {
            _shown = DateTimeOffset.Now;
            _timer.Start();
            Timer_Tick(this, new EventArgs());
        }

        public void Report(double value)
        {
            Dispatcher.Invoke(() =>
            {
                if (double.IsNaN(value))
                {
                    MainProgressBar.IsIndeterminate = true;
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                }
                else
                {
                    MainProgressBar.IsIndeterminate = false;
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    MainProgressBar.Value = value;
                    TaskbarItemInfo.ProgressValue = value;
                }
            }, DispatcherPriority.Render, CancellationToken);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
