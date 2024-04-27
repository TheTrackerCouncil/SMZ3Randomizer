using Randomizer.Data.WorldData;

namespace Randomizer.CrossPlatform.ViewModels;

public class LocationViewModel(Location? location, bool isInLogic, bool isInLogicWithKeys)
{
    public static string? KeyImage { get; set; }
    public string Name { get; init; } = location?.Metadata.Name?[0] ?? location?.Name ?? "";
    public string Area { get; init; } = location?.Region.Metadata.Name?[0] ?? location?.Region.Name ?? "";
    public double Opacity => InLogic || InLogicWithKeys ? 1.0 : 0.33;
    public bool InLogic => isInLogic;
    public bool InLogicWithKeys => isInLogicWithKeys;
    public bool ShowKeyIcon => isInLogicWithKeys && !isInLogic;
    public Location? Location => location;
}
