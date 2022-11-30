using System;

namespace Randomizer.Shared.Multiplayer;

public class MultiplayerGameState
{
    public string Guid { get; set; } = "";
    public string Url { get; set; } = "";
    public string Version { get; set; } = "";
    public MultiplayerGameStatus Status { get; set; }
    public MultiplayerGameType Type { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastMessage { get; set; }
    public string Seed { get; set; } = "";
    public string ValidationHash { get; set; } = "";
    public bool HasGameStarted => Status != MultiplayerGameStatus.Created;
}
