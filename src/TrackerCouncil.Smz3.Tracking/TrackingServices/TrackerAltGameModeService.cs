using System;
using System.Collections.Generic;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Services;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.SeedGenerator.AltGameModes;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerAltGameModeService(AltGameModeFactory altGameModeFactory, IWorldAccessor worldAccessor) : TrackerService, ITrackerAltGameModeService
{
    private GameModeType _gameModeType = GameModeType.Vanilla;
    private AltGameModeBase? _gameMode;

    public GameModeType GameModeType => _gameModeType;
    public bool HasAltGameMode => _gameModeType != GameModeType.Vanilla;
    public bool IsAltGameModeComplete { get; private set; }
    public event EventHandler? GoalStateChanged;

    public void MarkAltGameModeAsComplete()
    {
        IsAltGameModeComplete = true;
        Tracker.GameStateTracker.HasFinishedGoal = true;
        Tracker.GameService?.TrySetGameModeComplete();
        Tracker.UpdateAllAccessibility(false);
    }

    public bool IsAltGameModeKnowinglyComplete()
    {
        return HasAltGameMode && _gameMode!.IsKnowinglyComplete(Tracker.World);
    }

    public void OnViewingPyramidText()
    {
        if (!HasAltGameMode)
        {
            return;
        }

        if (_gameMode!.OnViewingPyramidText(Tracker.World))
        {
            _ = Tracker.SaveAsync();
            Tracker.UpdateAllAccessibility(false);
            Tracker.AltGameModeService.NotifyOfGoalStateChange();
        }
    }

    public void NotifyOfGoalStateChange()
    {
        GoalStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<GoalUiDetails> GetGoalUiDetails()
    {
        var state = _gameMode?.GetGoalUiDetails(Tracker.World);
        if (state?.Count > 0) return state;
        var progression = Tracker.PlayerProgressionService.GetProgression(false);
        var crystals = progression.CrystalCount;
        var bosses = progression.MetroidBossCount;
        var crystalsRequired = Tracker.World.State?.MarkedGanonCrystalCount != null
            ? $"{Tracker.World.State?.MarkedGanonCrystalCount}"
            : "?";
        var bossesRequired = Tracker.World.State?.MarkedTourianBossCount != null
            ? $"{Tracker.World.State?.MarkedTourianBossCount}"
            : "?";
        return
        [
            new GoalUiDetails { IconCategory = "Dungeons", Icon = "crystal.png", Text = $"{crystals}/{crystalsRequired}" },
            new GoalUiDetails { IconCategory = "Dungeons", Icon = "metroid boss token.png", Text = $"{bosses}/{bossesRequired}" }
        ];
    }

    public override void PostInitialize()
    {
        _gameModeType = worldAccessor.World.Config.GameModeOptions.SelectedGameModeType;
        if (_gameModeType == GameModeType.Vanilla)
        {
            return;
        }
        _gameMode = altGameModeFactory.GetGameMode(_gameModeType);

        IsAltGameModeComplete = _gameMode.IsComplete(worldAccessor.World);
        Tracker.GameStateTracker.HasFinishedGoal = IsAltGameModeComplete;

        _gameMode.InitializeTracker(Tracker);
    }
}
