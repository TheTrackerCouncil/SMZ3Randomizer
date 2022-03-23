namespace Randomizer.SMZ3.Tracking.Configuration.Providers
{
    public interface IConfigProvider
    {
        LocationConfig GetLocationConfig();
        TrackerMapConfig GetMapConfig();
        TrackerConfig GetTrackerConfig();
    }
}
