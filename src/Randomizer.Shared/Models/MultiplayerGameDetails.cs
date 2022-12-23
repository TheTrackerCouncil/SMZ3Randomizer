using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Shared.Models;

public class MultiplayerGameDetails
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; set; }

    public string ConnectionUrl { get; init; } = "";
    public string GameGuid { get; init; } = "";
    public string GameUrl { get; init; } = "";
    public MultiplayerGameType Type { get; init; }
    public MultiplayerGameStatus Status { get; set; }
    public string PlayerGuid { get; init; } = "";
    public string PlayerKey { get; init; } = "";
    public DateTimeOffset JoinedDate { get; init; }
    [ForeignKey("GeneratedRom")]
    public long? GeneratedRomId { get; set; }
    public GeneratedRom? GeneratedRom { get; set; }
}
