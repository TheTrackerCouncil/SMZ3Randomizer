﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Randomizer.Data.Options;

namespace Randomizer.Data.Interfaces;

public interface IStatGenerator
{
    public Task GenerateStatsAsync(int numberOfSeeds, Config config, CancellationToken ct);

    public event EventHandler<StatUpdateEventaArgs>? StatProgressUpdated;

    public event EventHandler<StatsCompletedEventArgs>? StatsCompleted;
}

public class StatUpdateEventaArgs(int current, int total) : EventArgs
{
    public int Current => current;
    public int Total => total;
}

public class StatsCompletedEventArgs(string message) : EventArgs
{
    public string Message => message;
}
