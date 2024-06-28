namespace TrackerCouncil.Smz3.Data.Configuration;

public interface IHasPostLoadFunction
{
    /// <summary>
    /// Called after loading the config file
    /// </summary>
    public void OnPostLoad();
}
