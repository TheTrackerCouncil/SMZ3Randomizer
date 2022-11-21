using System;

namespace Randomizer.Shared.Multiplayer;

public class MultiplayerGameState
{
    public string Guid { get; set; } = "";
    public string Url { get; set; } = "";
    public MultiplayerGameStatus Status { get; set; }
    public MultiplayerGameType Type { get; set; }
    public DateTime LastMessage { get; set; }
    public string Seed { get; set; } = "";
    public string ValidationHash { get; set; } = "";
    public bool HasGameStarted => Status != MultiplayerGameStatus.Created;
}
