using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Twitch.Models
{
    public class TwitchPoll
    {
        [JsonPropertyName("broadcaster_id")]
        public string? BroadcasterId { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("choices")]
        public ICollection<TwitchPollChoice>? Choices { get; set; }

        [JsonPropertyName("duration")]
        public int? Duration { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        public bool IsComplete => !"ACTIVE".Equals(Status, StringComparison.OrdinalIgnoreCase);

        public bool Successful => WinningChoice != null && "COMPLETED".Equals(Status, StringComparison.OrdinalIgnoreCase);

        public TwitchPollChoice? WinningChoice => Choices?.OrderByDescending(x => x.Votes).FirstOrDefault();
    }
}
