using System.Text.Json.Serialization;
using TrackerCouncil.Smz3.Chat.Integration.Models;

namespace TrackerCouncil.Smz3.Chat.Twitch.Models;

public class TwitchUser : TwitchAPIResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}
