using System.Text.Json.Serialization;
using TrackerCouncil.Smz3.Chat.Integration.Models;

namespace TrackerCouncil.Smz3.Chat.Twitch.Models;

public class TwitchAnnouncement : TwitchAPIResponse
{
    [JsonPropertyName("broadcaster_id")]
    public string? BroadcasterId { get; set; }

    [JsonPropertyName("moderator_id")]
    public string? ModeratorId { get; set; }

    [JsonPropertyName("message")]
    public new string? Message { get; set; }
}
