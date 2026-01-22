using ReactiveUI.SourceGenerators;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class SpriteDownloadWindowViewModel : ViewModelBase
{
    [Reactive] public partial int NumCompleted { get; set; }
    [Reactive] public partial int NumTotal { get; set; } = 1;
}
