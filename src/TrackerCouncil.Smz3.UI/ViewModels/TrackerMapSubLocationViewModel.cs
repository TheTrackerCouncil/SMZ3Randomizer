using ReactiveUI.SourceGenerators;
using TrackerCouncil.Smz3.Data.WorldData;
using TrackerCouncil.Smz3.Data.WorldData.Regions;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public partial class TrackerMapSubLocationViewModel : ViewModelBase
{
    public required string Name { get; set; }
    public required Location Location { get; set; }
    public Room? Room { get; set; }
    public Region? Region { get; set; }
    public bool IsHint { get; set; }
    public bool IsSpoiler { get; set; }
    [Reactive] public partial bool IsVisible { get; set; } = true;
}
