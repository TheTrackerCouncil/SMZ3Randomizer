using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Randomizer.Shared.Multiplayer;

public class MultiplayerLocationState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [JsonIgnore]
    public long Id { get; set; }
    public long GameId { get; set; }
    [JsonIgnore] public virtual MultiplayerGameState Game { get; init; } = null!;
    public long PlayerId { get; set; }
    [JsonIgnore] public virtual MultiplayerPlayerState Player { get; init; } = null!;
    public int LocationId { get; init; }
    public bool Tracked { get; set; }
    public DateTimeOffset? TrackedTime { get; set; }
}
