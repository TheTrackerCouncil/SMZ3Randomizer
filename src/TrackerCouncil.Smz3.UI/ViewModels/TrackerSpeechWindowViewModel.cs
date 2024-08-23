using Avalonia;
using Avalonia.Media;
using ReactiveUI.Fody.Helpers;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class TrackerSpeechWindowViewModel : ViewModelBase
{
    [Reactive] public string? TrackerImage { get; set; }
    [Reactive] public Thickness AnimationMargin { get; set; } = new(0, 0, 0, 0);
    [Reactive] public SolidColorBrush Background { get; set; } = new(new Color(0xFF, 0x48, 0x3D, 0x8B));
}
