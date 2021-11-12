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

using Randomizer.App.ViewModels;

namespace Randomizer.App
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    [NotAService]
    public partial class OptionsWindow : Window
    {
        public OptionsWindow(GeneralOptions options)
        {
            InitializeComponent();

            Options = options;
            DataContext = Options;
        }

        public GeneralOptions Options { get; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
