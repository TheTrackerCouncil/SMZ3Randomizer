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
    }

    public void SetTrackerState(TrackerState state)
    {
        _currentGameService.TrackerState = state;
    }

    public PlayerTrackedLocationEventHandler? PlayerTrackedLocation;

    public PlayerTrackedItemEventHandler? PlayerTrackedItem;

    public PlayerTrackedBossEventHandler? PlayerTrackedBoss;

    public PlayerTrackedDungeonEventHandler? PlayerTrackedDungeon;

    private void ClientOnDungeonTracked(MultiplayerPlayerState playerState, string dungeonName)
    {
        PlayerTrackedDungeon?.Invoke(_currentGameService.PlayerTrackedDungeon(playerState, dungeonName,
            playerState.Guid == _client.CurrentPlayerGuid));
    }

    private void ClientOnBossTracked(MultiplayerPlayerState playerState, BossType bossType)
    {
        PlayerTrackedBoss?.Invoke(_currentGameService.PlayerTrackedBoss(playerState, bossType,
            playerState.Guid == _client.CurrentPlayerGuid));
    }

    private void ClientOnItemTracked(MultiplayerPlayerState playerState, ItemType itemType, int trackingValue)
    {
        PlayerTrackedItem?.Invoke(_currentGameService.PlayerTrackedItem(playerState, itemType, trackingValue,
            playerState.Guid == _client.CurrentPlayerGuid));
    }

    private void ClientOnLocationTracked(MultiplayerPlayerState playerState, int locationId)
    {
        PlayerTrackedLocation?.Invoke(_currentGameService.PlayerTrackedLocation(playerState, locationId,
            playerState.Guid == _client.CurrentPlayerGuid));
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
        var seedData = _currentGameService.GenerateSeed(players, out var error);

        if (!string.IsNullOrEmpty(error) || seedData == null)
        {
            _logger.LogError("{Error}", error);
            return error;
        }

        var seed = seedData.Seed;
        var hash = _currentGameService.GetValidationHash(seedData.Worlds.Select(x => x.World));
        _logger.LogInformation("Hash: {Hash}", hash);
        var playerList = players.ToList();
        foreach (var player in playerList)
        {
            var state = _currentGameService.GetPlayerDefaultState(player,
                seedData.Worlds.Select(x => x.World).Single(x => x.Guid == player.Guid));
            await _client.UpdatePlayerState(state, false);
        }

        await _client.StartGame(seed, hash);
        return null;
    }

    public SeedData? RegenerateSeed(string seed, string validationHash, out string error)
    {
        var players = _client.Players!;
        var localPlayer = _client.LocalPlayer!;
        var seedData = _currentGameService.RegenerateSeed(players, localPlayer, seed, out error);

        if (!string.IsNullOrEmpty(error) || seedData == null)
        {
            _logger.LogError("{Error}", error);
            return null;
        }

        var hash = _currentGameService.GetValidationHash(seedData.Worlds.Select(x => x.World));
        _logger.LogInformation("Generated Seed Hash: {Hash} | Expected Hash: {PreviousHash}", hash, validationHash);
        if (hash != validationHash)
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
}
