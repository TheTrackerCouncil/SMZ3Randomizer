using Avalonia;
using Avalonia.Media;
using ReactiveUI.SourceGenerators;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class TrackerSpeechWindowViewModel : ViewModelBase
{
    [Reactive] public partial string? TrackerImage { get; set; }
    [Reactive] public partial Thickness AnimationMargin { get; set; } = new(0, 0, 0, 0);
    [Reactive] public partial SolidColorBrush Background { get; set; } = new(new Color(0xFF, 0x48, 0x3D, 0x8B));
    [Reactive] public partial bool IsTrackerImageVisible { get; set; } = false;
}
