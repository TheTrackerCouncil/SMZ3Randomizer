using TrackerCouncil.Smz3.Data.WorldData;

namespace TrackerCouncil.Smz3.UI.ViewModels;

public class MarkedLocationViewModel(Location? location, Item? itemData, string? itemSprite, bool isAvailable)
{
    public string? ItemSprite { get; init; } = itemSprite;
    public bool IsAvailable { get; set; } = isAvailable;
    public bool ShowOutOfLogic { get; set; }
    public double Opacity => ShowOutOfLogic || IsAvailable ? 1.0 : 0.33;
    public string Item { get; init; }= itemData?.Name ?? "";
    public string Location { get; init; }= location?.Metadata.Name?[0] ?? location?.Name ?? "";
    public string Area { get; init; } = location?.Region.Metadata.Name?[0] ?? location?.Region.Name ?? "";
}
