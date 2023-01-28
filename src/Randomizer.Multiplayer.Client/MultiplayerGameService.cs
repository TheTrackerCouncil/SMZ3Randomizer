using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Multiplayer.Client.EventHandlers;
using Randomizer.Multiplayer.Client.GameServices;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;
using Randomizer.SMZ3.Generation;

namespace Randomizer.Multiplayer.Client;

/// <summary>
/// Service for interacting with a current multiplayer game regardless
/// of the type of multiplayer game
/// </summary>
public class MultiplayerGameService : IDisposable
{
    private readonly ILogger<MultiplayerGameService> _logger;
    private readonly IEnumerable<MultiplayerGameTypeService> _gameTypeServices;
    private readonly MultiplayerClientService _client;
    private MultiplayerGameTypeService _currentGameService;

    public MultiplayerGameService(MultiplayerClientService clientService, IServiceProvider serviceProvider, ILogger<MultiplayerGameService> logger)
    {
        _gameTypeServices = serviceProvider.GetServices<MultiplayerGameTypeService>();
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
        _client.GameRejoined += ClientOnGameRejoined;
        _client.ConnectionClosed += ClientOnConnectionClosed;
        _currentGameService = UpdateGameService(_client.GameType ?? MultiplayerGameType.Multiworld);
    }

    private async void ClientOnConnectionClosed(string error, Exception? exception)
    {
        if (_currentGameService.TrackerState != null && _client.CurrentGameGuid != null)
        {
            await _client.Reconnect();
        }
    }

    public PlayerTrackedLocationEventHandler? PlayerTrackedLocation;

    public PlayerTrackedItemEventHandler? PlayerTrackedItem;

    public PlayerTrackedBossEventHandler? PlayerTrackedBoss;

    public PlayerTrackedDungeonEventHandler? PlayerTrackedDungeon;

    public PlayerSyncReceivedEventHandler? PlayerSyncReceived;

    public PlayerEndedGameEventHandler? PlayerEndedGame;

    public MultiplayerPlayerState? LocalPlayer => _client.LocalPlayer;

    /// <summary>
    /// Updates the tracker state for the current game service
    /// </summary>
    /// <param name="state">The tracker state</param>
    public void SetTrackerState(TrackerState state)
    {
        _currentGameService.TrackerState = state;
    }

    /// <summary>
    /// Generates seed data for the admin player to submit to the server
    /// </summary>
    /// <returns>An error if the seed failed to generate, null otherwise.</returns>
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
            var world = _currentGameService.GetPlayerDefaultState(
                seedData.WorldGenerationData.GetWorld(player.Guid).World, seedData.WorldGenerationData.Worlds);
            _logger.LogInformation("Pushing state");
            await _client.UpdatePlayerWorld(player, world, false);
            await _client.SubmitPlayerGenerationData(player.Guid,
                seedData.WorldGenerationData.GetWorld(player.Guid).GetPlayerGenerationData());
        }

        await _client.StartGame(hash);
        return null;
    }

    /// <summary>
    /// Regenerates the seed data from the player generation data sent from the server
    /// </summary>
    /// <param name="playerGenerationData">Data for each of the players needed to recreate the seed</param>
    /// <param name="error">Output error for if the seed regeneration was successful or not</param>
    /// <returns>The seed data if created successfully, null otherwise.</returns>
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

    /// <summary>
    /// Sends a location tracked event to the server
    /// </summary>
    /// <param name="location">The location that was tracked</param>
    public async Task TrackLocation(Location location)
    {
        await _currentGameService.TrackLocation(location);
    }

    /// <summary>
    /// Sends an item tracked event to the server
    /// </summary>
    /// <param name="item">The item that was tracked</param>
    public async Task TrackItem(Item item)
    {
        await _currentGameService.TrackItem(item);
    }

    /// <summary>
    /// Sends a dungeon tracked event to the server
    /// </summary>
    /// <param name="dungeon">The dungeon that was tracked</param>
    public async Task TrackDungeon(IDungeon dungeon)
    {
        await _currentGameService.TrackDungeon(dungeon);
    }

    /// <summary>
    /// Sends a boss tracked event to the server
    /// </summary>
    /// <param name="boss">The boss that was tracked</param>
    public async Task TrackBoss(Boss boss)
    {
        await _currentGameService.TrackBoss(boss);
    }

    /// <summary>
    /// Sends a player completed game event to the server so that
    /// the other players can receive their items
    /// </summary>
    public async Task CompletePlayerGame()
    {
        await _client.CompletePlayerGame();
    }

    /// <summary>
    /// Called when tracking has started
    /// </summary>
    public void OnTrackingStarted()
    {
        _currentGameService.OnTrackingStarted();
    }

    /// <summary>
    /// Called when auto tracking has successfully been connected.
    /// Used to check the current player states to see if any items need to be given to the player
    /// </summary>
    public async Task OnAutoTrackingConnected()
    {
        if (!_client.IsConnected)
        {
            await _client.Reconnect();
            return;
        }
        if (_client.Players == null) return;
        foreach (var playerState in _client.Players.Where(x => x != _client.LocalPlayer))
        {
            var args = _currentGameService.PlayerSyncReceived(playerState, null, false);
            if (args != null) PlayerSyncReceived?.Invoke(args);
        }
        _currentGameService.OnAutoTrackerConnected();
    }

    public void Dispose()
    {
        _client.LocationTracked -= ClientOnLocationTracked;
        _client.ItemTracked -= ClientOnItemTracked;
        _client.BossTracked -= ClientOnBossTracked;
        _client.DungeonTracked -= ClientOnDungeonTracked;
        _client.PlayerUpdated -= ClientOnPlayerUpdated;
        _client.PlayerStateRequested -= ClientOnPlayerStateRequested;
        _client.PlayerForfeited -= ClientOnPlayerForfeited;
        _client.PlayerCompleted -= ClientOnPlayerCompleted;
        _client.GameRejoined -= ClientOnGameRejoined;
        _client.ConnectionClosed -= ClientOnConnectionClosed;
        GC.SuppressFinalize(this);
    }

    #region Multiplayer Client Events
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
        var world = _currentGameService.GetPlayerWorldState(_client.LocalPlayer, _currentGameService.TrackerState);
        await _client.UpdatePlayerWorld(_client.LocalPlayer, world);
    }

    private async void ClientOnGameRejoined()
    {
        UpdateGameService(_client.GameType ?? MultiplayerGameType.Multiworld);
        if (_client.Players == null) return;

        // If the player rejoined, make sure they didn't miss anything
        foreach (var playerState in _client.Players.Where(x => x != _client.LocalPlayer))
        {
            var args = _currentGameService.PlayerSyncReceived(playerState, null, false);
            if (args != null) PlayerSyncReceived?.Invoke(args);
        }

        // Also push out the latest state for the local player in case they did something while not connected
        if (_client.LocalPlayer == null || _currentGameService.TrackerState == null) return;
        var world = _currentGameService.GetPlayerWorldState(_client.LocalPlayer, _currentGameService.TrackerState);
        await _client.UpdatePlayerWorld(_client.LocalPlayer, world);
    }
    #endregion

    #region Helper Functions
    private MultiplayerGameTypeService UpdateGameService(MultiplayerGameType type)
    {
        _currentGameService = type switch
        {
            _ => _gameTypeServices.Single(x => x is MultiworldGameService)
        };
        return _currentGameService;
    }
    #endregion
}
