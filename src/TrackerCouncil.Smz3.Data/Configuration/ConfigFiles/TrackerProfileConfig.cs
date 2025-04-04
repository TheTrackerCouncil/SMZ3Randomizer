namespace TrackerCouncil.Smz3.Data.Configuration.ConfigFiles;

public class TrackerProfileConfig
{
    public bool UseAltVoice { get; set; }
    public ResponseConfig? ResponseConfig { get; set; }
    public BossConfig? BossConfig { get; set; }
    public ItemConfig? ItemConfig { get; set; }
    public LocationConfig? LocationConfig { get; set; }
    public RegionConfig? RegionConfig { get; set; }
    public RequestConfig? RequestConfig { get; set; }
    public RewardConfig? RewardConfig { get; set; }
    public RoomConfig? RoomConfig { get; set; }
}
