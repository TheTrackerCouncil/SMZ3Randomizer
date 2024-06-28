using System.Collections.Generic;
using System.Text.Json.Serialization;

using TrackerCouncil.Smz3.Chat.Integration.Models;

namespace TrackerCouncil.Smz3.Chat.Twitch.Models;

public class ValidateResponse : TwitchAPIResponse
{
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("scopes")]
    public IReadOnlyCollection<string>? Scopes { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
}
