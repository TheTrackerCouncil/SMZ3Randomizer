using ReactiveUI.Fody.Helpers;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class SpriteDownloadWindowViewModel : ViewModelBase
{
    [Reactive] public int NumCompleted { get; set; }
    [Reactive] public int NumTotal { get; set; } = 1;
}
