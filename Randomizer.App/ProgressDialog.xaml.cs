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
using System.Windows.Threading;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window, IProgress<double>
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public ProgressDialog(Window owner, string title)
        {
            InitializeComponent();

            Owner = owner;
            Title = owner.Title;

            MainInstructionText.Text = title;
            MainProgressBar.Minimum = 0d;
            MainProgressBar.Maximum = 1d;
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public void Report(double value)
        {
            Dispatcher.Invoke(() =>
            {
                if (double.IsNaN(value))
                {
                    MainProgressBar.IsIndeterminate = true;
                }
                else
                {
                    MainProgressBar.IsIndeterminate = false;
                    MainProgressBar.Value = value;
                }
            }, DispatcherPriority.Render, CancellationToken);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
