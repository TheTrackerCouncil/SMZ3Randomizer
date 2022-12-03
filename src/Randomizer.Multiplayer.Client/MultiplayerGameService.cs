using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Multiplayer.Client.GameServices;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client;

public class MultiplayerGameService
{
    private readonly ILogger<MultiplayerGameService> _logger;
    private readonly IEnumerable<MultiplayerGameTypeService> _gameTypeServices;
    private readonly MultiplayerClientService _client;
    private MultiplayerGameTypeService _currentGameService;

    public MultiplayerGameService(MultiplayerClientService clientService, IServiceProvider serviceProvider, ILogger<MultiplayerGameService> logger)
    {
        _gameTypeServices = serviceProvider.GetServices<MultiplayerGameTypeService>();
        _currentGameService = _gameTypeServices.Single(x => x is MultiworldGameService);
        _logger = logger;
        _client = clientService;
        _client.LocationTracked += ClientOnLocationTracked;
        _client.ItemTracked += ClientOnItemTracked;
        _client.BossTracked += ClientOnBossTracked;
        _client.DungeonTracked += ClientOnDungeonTracked;
        _client.PlayerUpdated += ClientOnPlayerUpdated;
        _client.PlayerStateRequested += ClientOnPlayerStateRequested;
        _client.PlayerForfeited += ClientOnPlayerForfeited;
        _client.PlayerCompleted += ClientOnPlayerCompleted;
    }

    public void SetTrackerState(TrackerState state)
    {
        _currentGameService.TrackerState = state;
    }

    public PlayerTrackedLocationEventHandler? PlayerTrackedLocation;

    public PlayerTrackedItemEventHandler? PlayerTrackedItem;

    public PlayerTrackedBossEventHandler? PlayerTrackedBoss;

    public PlayerTrackedDungeonEventHandler? PlayerTrackedDungeon;

    public PlayerSyncReceivedEventHandler? PlayerSyncReceived;

    public PlayerEndedGameEventHandler? PlayerEndedGame;

    private void ClientOnDungeonTracked(MultiplayerPlayerState playerState, string dungeonName)
    {
        var args = _currentGameService.PlayerTrackedDungeon(playerState, dungeonName,
            playerState.Guid == _client.CurrentPlayerGuid);
        if (args != null) PlayerTrackedDungeon?.Invoke(args);
    }

    private void ClientOnBossTracked(MultiplayerPlayerState playerState, BossType bossType)
    {
        var args = _currentGameService.PlayerTrackedBoss(playerState, bossType,
            playerState.Guid == _client.CurrentPlayerGuid);
        if (args != null) PlayerTrackedBoss?.Invoke(args);
    }

    private void ClientOnItemTracked(MultiplayerPlayerState playerState, ItemType itemType, int trackingValue)
    {
        var args = _currentGameService.PlayerTrackedItem(playerState, itemType, trackingValue,
            playerState.Guid == _client.CurrentPlayerGuid);
        if (args != null) PlayerTrackedItem?.Invoke(args);
    }

    private void ClientOnLocationTracked(MultiplayerPlayerState playerState, int locationId)
    {
        var args = _currentGameService.PlayerTrackedLocation(playerState, locationId,
            playerState.Guid == _client.CurrentPlayerGuid);
        if (args != null) PlayerTrackedLocation?.Invoke(args);
    }

    private void ClientOnPlayerUpdated(MultiplayerPlayerState playerState, MultiplayerPlayerState? previousState, bool isLocalPlayer)
    {
        var args = _currentGameService.PlayerSyncReceived(playerState, previousState, isLocalPlayer);
        if (args != null) PlayerSyncReceived?.Invoke(args);
    }

    private void ClientOnPlayerCompleted(MultiplayerPlayerState state, bool isLocalPlayer)
    {
        PlayerEndedGame?.Invoke(_currentGameService.PlayerEndedGame(state, isLocalPlayer, false, true));
    }

    private void ClientOnPlayerForfeited(MultiplayerPlayerState state, bool isLocalPlayer)
    {
        PlayerEndedGame?.Invoke(_currentGameService.PlayerEndedGame(state, isLocalPlayer, true, false));
    }

    private async void ClientOnPlayerStateRequested()
    {
        if (_client.LocalPlayer == null || _currentGameService.TrackerState == null)
        {
            _logger.LogWarning("Player state update requested, but either the local player multiplayer state or the tracker state is unavailable");
            return;
        }
        _currentGameService.UpdatePlayerState(_client.LocalPlayer, _currentGameService.TrackerState);
        await _client.UpdatePlayerState(_client.LocalPlayer);
    }

    public void UpdateGameType(MultiplayerGameType type)
    {
        _currentGameService = type switch
        {
            _ => _gameTypeServices.Single(x => x is MultiworldGameService)
        };
    }

    public async Task<string?> GenerateSeed()
    {
        var players = _client.Players!;
        var error = "";
        SeedData? seedData = null;

        await Task.Run(() => seedData = _currentGameService.GenerateSeed(_client.CurrentGameState!.Seed, players, _client.LocalPlayer!, out error));

        if (!string.IsNullOrEmpty(error) || seedData == null)
        {
            _logger.LogError("{Error}", error);
            return error;
        }

        var hash = _currentGameService.GetValidationHash(seedData.WorldGenerationData.Worlds);
        _logger.LogInformation("Hash: {Hash}", hash);
        var playerList = players.ToList();
        foreach (var player in playerList)
        {
            var state = _currentGameService.GetPlayerDefaultState(player,
                seedData.WorldGenerationData.GetWorld(player.Guid).World);
            _logger.LogInformation("Pushing state");
            await _client.UpdatePlayerState(state, false);
            await _client.SubmitPlayerGenerationData(player.Guid,
                seedData.WorldGenerationData.GetWorld(player.Guid).GetPlayerGenerationData());
        }

        await _client.StartGame(hash);
        return null;
    }

    public SeedData? RegenerateSeed(List<MultiplayerPlayerGenerationData> playerGenerationData, out string error)
    {
        var players = _client.Players!;
        var localPlayer = _client.LocalPlayer!;
        var seedData = _currentGameService.RegenerateSeed(_client.CurrentGameState!.Seed, playerGenerationData, players, localPlayer, out error);

        if (!string.IsNullOrEmpty(error) || seedData == null)
        {
            _logger.LogError("{Error}", error);
            return null;
        }

        var hash = _currentGameService.GetValidationHash(seedData.WorldGenerationData.Worlds);
        _logger.LogInformation("Generated Seed Hash: {Hash} | Expected Hash: {PreviousHash}", hash, _client.CurrentGameState.ValidationHash);
        if (hash != _client.CurrentGameState.ValidationHash)
        {
            error = "Generated seed does not match. Make sure you are using the same version of the randomizer.";
            _logger.LogError("{Error}", error);
            return null;
        }

        return seedData;
    }

    public async Task TrackLocation(Location location)
    {
        await _currentGameService.TrackLocation(location);
    }

    public async Task TrackItem(Item item)
    {
        await _currentGameService.TrackItem(item);
    }

    public async Task TrackDungeon(IDungeon dungeon)
    {
        await _currentGameService.TrackDungeon(dungeon);
    }

    public async Task TrackBoss(Boss boss)
    {
        await _currentGameService.TrackBoss(boss);
    }

    public async Task CompletePlayerGame()
    {
        await _client.CompletePlayerGame();
    }

    public void OnTrackingStarted()
    {
        _currentGameService.OnTrackingStarted();
    }

    public void OnAutoTrackingConnected()
    {
        if (_client.Players == null) return;
        foreach (var playerState in _client.Players.Where(x => x != _client.LocalPlayer))
        {
            var args = _currentGameService.PlayerSyncReceived(playerState, null, false);
            if (args != null) PlayerSyncReceived?.Invoke(args);
        }
    }

    public void UpdatePlayerState(MultiplayerPlayerState state, TrackerState trackerState)
    {
        _currentGameService.UpdatePlayerState(state, trackerState);
    }
}
