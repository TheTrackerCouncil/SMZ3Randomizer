using System.Diagnostics;
using System.Windows;

namespace TrackerCouncil.Smz3.UI.Legacy.Windows;

/// <summary>
/// Interaction logic for AutotrackerWindow.xaml
/// </summary>
public partial class AutoTrackerWindow : Window
{
    public AutoTrackerWindow()
    {
        InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.ToString(),
            UseShellExecute = true
        });
    }
}
