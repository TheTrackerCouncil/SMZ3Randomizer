using System;
using System.Collections.Generic;
using System.Linq;
using TrackerCouncil.Smz3.Shared;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.WorldData.Regions;
using TrackerCouncil.Smz3.Shared.Enums;
using TrackerCouncil.Smz3.Shared.Models;

namespace TrackerCouncil.Smz3.Data.WorldData;

/// <summary>
/// Represents a reward for a particular player
/// </summary>
public class Reward
{
    private Accessibility _accessibility;

    public Reward(RewardType type, World world, IMetadataService? metadata)
    {
        Type = type;
        World = world;
        Metadata = metadata?.Reward(type) ?? new RewardInfo(type);
    }

    public RewardType Type { get; }

    public World World { get; }

    public RewardInfo Metadata { get; }

    public IHasReward? Region { get; set; }

    public TrackerRewardState State => Region?.RewardState ?? new TrackerRewardState();

    public bool HasReceivedReward
    {
        get => State.HasReceivedReward;
        set
        {
            State.HasReceivedReward = value;
            UpdatedRewardState?.Invoke(this, EventArgs.Empty);
        }
    }

    public RewardType? MarkedReward
    {
        get => State.MarkedReward;
        set
        {
            State.MarkedReward = value;
            UpdatedRewardState?.Invoke(this, EventArgs.Empty);
        }
    }

    public Accessibility Accessibility
    {
        get => _accessibility;
        set
        {
            if (_accessibility == value) return;
            _accessibility = value;
            UpdatedAccessibility?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UpdateAccessibility(Progression actualProgression, Progression withKeysProgression)
    {
        if (HasReceivedReward)
        {
            Accessibility = Accessibility.Cleared;
        }
        else if (Region == null)
        {
            Accessibility = Accessibility.Unknown;
        }
        else if (Region.CanRetrieveReward(actualProgression))
        {
            Accessibility = Accessibility.Available;
        }
        else if (Region.CanRetrieveReward(withKeysProgression))
        {
            Accessibility = Accessibility.AvailableWithKeys;
        }
        else
        {
            Accessibility = Accessibility.OutOfLogic;
        }
    }

    public bool HasCorrectlyMarkedReward => MarkedReward == Type;

    public event EventHandler? UpdatedRewardState;

    public event EventHandler? UpdatedAccessibility;

}
