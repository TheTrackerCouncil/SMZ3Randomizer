using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Randomizer.SMZ3.ChatIntegration.Models;

namespace Randomizer.SMZ3.Twitch.Models
{
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
}
