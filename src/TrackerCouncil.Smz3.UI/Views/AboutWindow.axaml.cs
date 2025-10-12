using System.Diagnostics;
using System.Reflection;
using Avalonia.Interactivity;
using AvaloniaControls;
using AvaloniaControls.Controls;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class AboutWindow : ScalableWindow
{
    public AboutWindow()
    {
        InitializeComponent();

        TextBlockVersion.Text = $"Version {App.Version}";
    }

    private void SMZ3Button_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://samus.link/");
    }

    private void BetusButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://twitch.tv/the_betus");
    }

    private void PinkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://twitch.tv/pinkkittyrose");
    }
}

