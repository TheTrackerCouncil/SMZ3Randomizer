using System.Text.Json.Serialization;

namespace TrackerCouncil.Smz3.Chat.Twitch.Models;

public class TwitchPollChoice
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("votes")]
    public int? Votes { get; set; }

}
