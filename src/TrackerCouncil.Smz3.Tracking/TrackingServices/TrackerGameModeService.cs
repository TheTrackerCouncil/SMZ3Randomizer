using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Data.Tracking;
using TrackerCouncil.Smz3.SeedGenerator.Contracts;
using TrackerCouncil.Smz3.SeedGenerator.GameModes;
using TrackerCouncil.Smz3.Shared.Enums;

namespace TrackerCouncil.Smz3.Tracking.TrackingServices;

internal class TrackerGameModeService(GameModeFactory gameModeFactory, IWorldAccessor worldAccessor) : TrackerService, ITrackerGameModeService
{
    public GameModeType GetCurrentGameModeType() => _gameModeType;

    private GameModeType _gameModeType = GameModeType.None;
    private GameModeBase? _gameMode;


    public override void PostInitialize()
    {
        _gameModeType = worldAccessor.World.Config.GameModeOptions.SelectedGameModeType;
        if (_gameModeType == GameModeType.None)
        {
            return;
        }
        _gameMode = gameModeFactory.GetGameMode(_gameModeType);

        Tracker.ItemTracker.ItemTracked += TrackerItemServiceOnItemTracked;
    }

    private void TrackerItemServiceOnItemTracked(object? sender, ItemTrackedEventArgs e)
    {
        if (!e.AutoTracked)
        {
            return;
        }
    }
}
