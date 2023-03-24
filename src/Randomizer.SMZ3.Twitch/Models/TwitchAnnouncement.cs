using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Randomizer.SMZ3.ChatIntegration.Models;

namespace Randomizer.SMZ3.Twitch.Models
{
    public class TwitchAnnouncement : TwitchAPIResponse
    {
        [JsonPropertyName("broadcaster_id")]
        public string? BroadcasterId { get; set; }

        [JsonPropertyName("moderator_id")]
        public string? ModeratorId { get; set; }

        [JsonPropertyName("message")]
        public new string? Message { get; set; }
    }
}
