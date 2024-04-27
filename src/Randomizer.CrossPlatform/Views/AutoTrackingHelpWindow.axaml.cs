using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaControls;
using AvaloniaControls.Controls;

namespace Randomizer.CrossPlatform.Views;

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
}

