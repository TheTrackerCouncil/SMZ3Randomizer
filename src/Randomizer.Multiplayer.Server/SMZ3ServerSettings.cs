namespace Randomizer.Multiplayer.Server;

public class SMZ3ServerSettings
{
    public string ServerUrl { get; set; } = "";
    public int GameMemoryExpirationInMinutes { get; set; } = 60;
    public int GameCheckFrequencyInMinutes { get; set; } = 15;
    public int GameDatabaseExpirationInDays { get; set; } = 30;
    public string SQLiteFilePath { get; set; } = "";
}
