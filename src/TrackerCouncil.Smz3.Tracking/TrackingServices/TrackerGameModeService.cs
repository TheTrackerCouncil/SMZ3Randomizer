using System;
using System.Collections.Generic;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.ViewModels;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.GameModes;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerGameModeService(GameModeFactory gameModeFactory, IWorldAccessor worldAccessor) : TrackerService, ITrackerGameModeService
{
    private GameModeType _gameModeType = GameModeType.Vanilla;
    private GameModeBase? _gameMode;

    public GameModeType GameModeType => _gameModeType;
    public bool HasAltGameMode => _gameModeType != GameModeType.Vanilla && _gameModeType != GameModeType.AllDungeons;
    public bool IsGameModeComplete { get; private set; }
    public event EventHandler? GoalStateChanged;

    public void MarkGameModeAsComplete()
    {
        IsGameModeComplete = true;
        Tracker.GameService?.TrySetGameModeComplete();
        Tracker.UpdateAllAccessibility(false);
    }

    public bool IsGameModeKnowinglyComplete()
    {
        return _gameMode!.IsKnowinglyComplete(Tracker.World);
    }

    public void OnViewingPyramidText()
    {
        if (_gameMode!.OnViewingPyramidText(Tracker.World))
        {
            _ = Tracker.SaveAsync();
            Tracker.UpdateAllAccessibility(false);
            Tracker.GameModeService.NotifyOfGoalStateChange();
        }
    }

    public void NotifyOfGoalStateChange()
    {
        GoalStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<GoalUiDetails> GetGoalUiDetails()
    {
        var progression = Tracker.PlayerProgressionService.GetProgression(false);
        return _gameMode?.GetGoalUiDetails(Tracker.World, progression) ?? [];
    }

    public override void PostInitialize()
    {
        _gameModeType = worldAccessor.World.Config.GameModeOptions.SelectedGameModeType;
        _gameMode = gameModeFactory.GetGameMode(_gameModeType);
        _gameMode.InitializeTracker(Tracker);
        IsGameModeComplete = _gameMode.IsComplete(worldAccessor.World);
        NotifyOfGoalStateChange();
    }
}
