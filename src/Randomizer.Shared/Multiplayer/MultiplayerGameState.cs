using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Randomizer.Shared.Multiplayer;

public class MultiplayerGameState
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [JsonIgnore]
    public long Id { get; set; }
    public string Guid { get; set; } = "";
    public string Url { get; set; } = "";
    public string Version { get; set; } = "";
    public MultiplayerGameStatus Status { get; set; }
    public MultiplayerGameType Type { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastMessage { get; set; }
    public string Seed { get; set; } = "";
    public string ValidationHash { get; set; } = "";
    public bool SaveToDatabase { get; set; }
    public bool SendItemsOnComplete { get; set; }
    public ICollection<MultiplayerPlayerState> Players { get; set; } = new List<MultiplayerPlayerState>();
    public bool HasGameStarted => Status != MultiplayerGameStatus.Created;

    /// <summary>
    /// Copies properties from the provided state
    /// </summary>
    /// <param name="other"></param>
    public void Copy(MultiplayerGameState other)
    {
        Status = other.Status;
        LastMessage = other.LastMessage;
        ValidationHash = other.ValidationHash;
    }
}
