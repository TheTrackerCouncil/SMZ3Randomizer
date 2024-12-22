using Avalonia.Interactivity;
using AvaloniaControls;
using AvaloniaControls.Controls;

namespace TrackerCouncil.Smz3.UI.Views;

public partial class AutoTrackingHelpWindow : ScalableWindow
{
    public AutoTrackingHelpWindow()
    {
        InitializeComponent();
    }

    private void Snes9xButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://github.com/gocha/snes9x-rr/releases");
    }

    private void BizHawkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://tasvideos.org/Bizhawk");
    }

    private void RetroArchButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://www.retroarch.com/");
    }

    private void Snes9xEmuNwaButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://github.com/Skarsnik/snes9x-emunwa");
    }

    private void QUsb2SnesButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://skarsnik.github.io/QUsb2snes/");
    }

    private void SniButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformTools.OpenUrl("https://github.com/alttpo/sni");
    }
}

