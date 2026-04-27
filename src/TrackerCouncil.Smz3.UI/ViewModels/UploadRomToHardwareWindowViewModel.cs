using ReactiveUI.SourceGenerators;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class UploadRomToHardwareWindowViewModel : ViewModelBase
{
    [Reactive]
    public partial string MainText { get; set; } = "Uploading...";

    [Reactive]
    public partial bool IsLoading { get; set; } = true;

    [Reactive] public partial string ButtonText { get; set; } = "Cancel";
}
