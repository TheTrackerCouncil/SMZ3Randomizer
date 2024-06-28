using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace TrackerCouncil.Smz3.UI.Legacy.Windows;

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        Version = version.ProductVersion ?? "";

        if (Version.Contains('+'))
        {
            Version = Version[..Version.IndexOf('+')];
        }

        InitializeComponent();
    }

    public string Version { get; }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.ToString(),
            UseShellExecute = true
        });
    }
}
