using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Randomizer.Multiplayer.Client.EventHandlers;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client;

/// <summary>
/// Service for interacting with the SMZ3 multiplayer server
/// </summary>
public class MultiplayerClientService
{
    private readonly ILogger<MultiplayerClientService> _logger;
    private HubConnection? _connection;
    private readonly RandomizerContext _dbContext;

    public MultiplayerClientService(ILogger<MultiplayerClientService> logger, RandomizerContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public event MultiplayerErrorEventHandler? Error;
    public event MultiplayerGenericEventHandler? Connected;
    public event MultiplayerErrorEventHandler? ConnectionClosed;
    public event MultiplayerGenericEventHandler? GameCreated;
    public event MultiplayerGenericEventHandler? GameJoined;
    public event MultiplayerGenericEventHandler? GameRejoined;
    public event MultiplayerGenericEventHandler? PlayerListUpdated;
    public event MultiplayerGenericEventHandler? GameStateUpdated;
    public event MultiplayerGenericEventHandler? PlayerStateRequested;
    public event PlayerFinishedEventHandler? PlayerForfeited;
    public event PlayerFinishedEventHandler? PlayerCompleted;
    public event PlayerUpdatedEventHandler? PlayerUpdated;
    public event GameStartedEventHandler? GameStarted;
    public event TrackLocationEventHandler? LocationTracked;
    public event TrackItemEventHandler? ItemTracked;
    public event TrackDungeonEventHandler? DungeonTracked;
    public event TrackBossEventHandler? BossTracked;

    public string? CurrentGameGuid { get; private set; }
    public string? CurrentPlayerGuid { get; private set; }
    public string? CurrentPlayerKey { get; private set; }
    public MultiplayerGameState? CurrentGameState { get; private set; }
    public MultiplayerGameStatus? GameStatus => CurrentGameState?.Status;
    public MultiplayerGameType? GameType => CurrentGameState?.Type;
    public List<MultiplayerPlayerState>? Players { get; set; }
    public MultiplayerPlayerState? LocalPlayer => Players?.FirstOrDefault(x => x.Guid == CurrentPlayerGuid);
    public string? GameUrl => CurrentGameState?.Url;
    public string? ConnectionUrl { get; private set; }
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;
    public bool HasJoinedGame()
    {
        if (string.IsNullOrEmpty(CurrentGameGuid) || string.IsNullOrEmpty(CurrentPlayerGuid) ||
            string.IsNullOrEmpty(CurrentPlayerKey))
        {
            _logger.LogWarning("Player has not joined game");
            return false;
        }

        return true;
    }
    public MultiplayerGameDetails? DatabaseGameDetails { get; set; }

    #region Commands

    /// <summary>
    /// Connects to a given SignalR server to start sending messages back and forth
    /// </summary>
    /// <param name="url">The server to connect to</param>
    public async Task Connect(string url)
    {
        if (_connection != null && url == ConnectionUrl && _connection.State == HubConnectionState.Disconnected)
        {
            await Reconnect();
            return;
        }

        ConnectionUrl = url.SubstringBeforeCharacter('?') ?? url;

        if (_connection != null && _connection.State != HubConnectionState.Disconnected)
        {
            await _connection.DisposeAsync();
        }

        try
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();
        }
        catch (UriFormatException e)
        {
            _logger.LogError(e, "Invalid url {Url}", url);
            Error?.Invoke($"Invalid url {url}", e);
            return;
        }

        _connection.On<CreateGameResponse>("CreateGame", OnCreateGame);

        _connection.On<JoinGameResponse>("JoinGame", OnJoinGame);

        _connection.On<RejoinGameResponse>("RejoinGame", OnRejoinGame);

        _connection.On<ForfeitPlayerGameResponse>("ForfeitPlayerGame", OnForfeitPlayerGame);

        _connection.On<CompletePlayerGameResponse>("CompletePlayerGame", OnCompletePlayerGame);

        _connection.On<PlayerSyncResponse>("PlayerSync", OnPlayerSync);

        _connection.On<PlayerListSyncResponse>("PlayerListSync", OnPlayerListSync);

        _connection.On<UpdateGameStateResponse>("UpdateGameState", OnUpdateGameState);

        _connection.On<TrackLocationResponse>("TrackLocation", OnTrackLocation);

        _connection.On<TrackItemResponse>("TrackItem", OnTrackItem);

        _connection.On<TrackDungeonResponse>("TrackDungeon", OnTrackDungeon);

        _connection.On<TrackBossResponse>("TrackBoss", OnTrackBoss);

        _connection.On<StartGameResponse>("StartGame", OnStartGame);

        _connection.On<ErrorResponse>("Error", OnError);

        _connection.On<RequestPlayerUpdateRequest>("RequestPlayerUpdate", OnRequestPlayerUpdate);

        _connection.On<MultiplayerRequest>("Disconnect", OnDisconnectRequest);

        _connection.Closed += ConnectionOnClosed;

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(e, "Unable to connect to {Url}", url);
            Error?.Invoke($"Unable to connect to {url}", e);
            return;
        }

        if (_connection.State == HubConnectionState.Connected)
        {
            _logger.LogInformation("Connected to {Url}", url);
            Connected?.Invoke();
        }
        else
        {
            _logger.LogError("Unable to connect to {Url} - Connection state: {State}", url, _connection.State);
            Error?.Invoke($"Unable to connect to {url}");
        }
    }

    private async Task OnDisconnectRequest(MultiplayerRequest obj)
    {
        _logger.LogInformation("Server requested player to disconnected.");
        if (_connection == null) return;
        await _connection!.DisposeAsync();
        _connection = null;
        ConnectionClosed?.Invoke("Connection closed");
    }

    /// <summary>
    /// Reconnects to the server using previously saved details
    /// </summary>
    /// <param name="gameDetails">The database game details to reload</param>
    public async Task Reconnect(MultiplayerGameDetails gameDetails)
    {
        DatabaseGameDetails = gameDetails;
        CurrentGameGuid = gameDetails.GameGuid;
        CurrentPlayerGuid = gameDetails.PlayerGuid;
        CurrentPlayerKey = gameDetails.PlayerKey;
        await Connect(gameDetails.ConnectionUrl);
    }

    /// <summary>
    /// Reconnects to the previously established connection
    /// </summary>
    public async Task Reconnect()
    {
        if (string.IsNullOrEmpty(ConnectionUrl))
        {
            Error?.Invoke($"Could not reconnect as you were not previously connected.");
            return;
        }

        if (_connection == null)
        {
            await Connect(ConnectionUrl);
            return;
        }

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(e, "Unable to connect to {Url}", ConnectionUrl);
            Error?.Invoke($"Unable to connect to {ConnectionUrl}", e);
            return;
        }

        if (_connection.State == HubConnectionState.Connected)
        {
            _logger.LogInformation("Connected to {Url}", ConnectionUrl);
            Connected?.Invoke();
        }
        else
        {
            _logger.LogError("Unable to connect to {Url} - Connection state: {State}", ConnectionUrl, _connection.State);
            Error?.Invoke($"Unable to connect to {ConnectionUrl}");
        }
    }

    /// <summary>
    /// Disconnects from the connection and clears out all saved values
    /// </summary>
    public async Task Disconnect()
    {
        DatabaseGameDetails = null;
        CurrentGameGuid = null;
        CurrentPlayerGuid = null;
        CurrentPlayerKey = null;
        Players = null;
        CurrentGameState = null;
        if (_connection == null) return;
        await _connection!.DisposeAsync();
        _connection = null;
    }

    /// <summary>
    /// Creates a new multiplayer game on the server
    /// </summary>
    /// <param name="playerName">The requested player name</param>
    /// <param name="phoneticName">The requested player name for tracker to say</param>
    /// <param name="gameType">The requested game type</param>
    /// <param name="randomizerVersion">The local SMZ3 version to ensure compatibility between all players</param>
    /// <param name="saveToDatabase">If the game should be saved ot the database</param>
    /// <param name="sendItemsOnComplete">If items in a player's world should be auto distributed on beating the game</param>
    public async Task CreateGame(string playerName, string phoneticName, MultiplayerGameType gameType, string randomizerVersion, bool saveToDatabase, bool sendItemsOnComplete)
    {
        await MakeRequest("CreateGame", new CreateGameRequest(playerName, phoneticName, gameType, randomizerVersion, MultiplayerVersion.Id, saveToDatabase, sendItemsOnComplete));
    }

    /// <summary>
    /// Joins a multiplayer game
    /// </summary>
    /// <param name="gameGuid">The unique identifier of the game</param>
    /// <param name="playerName">The requested player name</param>
    /// <param name="phoneticName">The requested player name for tracker to say</param>
    /// <param name="randomizerVersion">The local SMZ3 version to ensure compatibility between all players</param>
    public async Task JoinGame(string gameGuid, string playerName, string phoneticName, string randomizerVersion)
    {
        await MakeRequest("JoinGame", new JoinGameRequest(gameGuid, playerName, phoneticName, randomizerVersion, MultiplayerVersion.Id));
    }

    /// <summary>
    /// Submits the provided config string to the server for the local player
    /// </summary>
    /// <param name="config">The compressed config string</param>
    public async Task SubmitConfig(string config)
    {
        if (LocalPlayer == null)
        {
            Error?.Invoke($"There is no active local player");
            return;
        }

        await MakeRequest("UpdatePlayerConfig", new UpdatePlayerConfigRequest(LocalPlayer.Guid, config), true);
    }

    /// <summary>
    /// Updates the local player's state
    /// </summary>
    /// <param name="state">The player state to with all updated details</param>
    /// <param name="propogate">The if the update should immediately be sent to all players</param>
    public async Task UpdatePlayerState(MultiplayerPlayerState state, bool propogate = true)
    {
        if (LocalPlayer == null)
        {
            Error?.Invoke($"There is no active local player");
            return;
        }

        await MakeRequest("UpdatePlayerState", new UpdatePlayerStateRequest(state, propogate), true);
    }

    /// <summary>
    /// Updates the local player's state
    /// </summary>
    /// <param name="player">The player state to with all updated details</param>
    /// <param name="world"></param>
    /// <param name="propogate">The if the update should immediately be sent to all players</param>
    public async Task UpdatePlayerWorld(MultiplayerPlayerState player, MultiplayerWorldState world, bool propogate = true)
    {
        if (LocalPlayer == null)
        {
            Error?.Invoke($"There is no active local player");
            return;
        }

        await MakeRequest("UpdatePlayerWorld", new UpdatePlayerWorldRequest(player.Guid, world, propogate), true);
    }

    /// <summary>
    /// Rejoins a previously joined game
    /// </summary>
    /// <param name="gameGuid">The unique identifier for the game to rejoin. Uses the latest joined game guid if a value is not provided</param>
    /// <param name="playerGuid">The unique identifier for the player. Uses the latest joined player guid if a value is not provided</param>
    /// <param name="playerKey">The validation key for the player. Uses the latest joined player key if a value is not provided</param>
    public async Task RejoinGame(string? gameGuid = null, string? playerGuid = null, string? playerKey = null)
    {
        if (!string.IsNullOrEmpty(gameGuid))
            CurrentGameGuid = gameGuid;
        if (!string.IsNullOrEmpty(playerGuid))
            CurrentPlayerGuid = playerGuid;
        if (!string.IsNullOrEmpty(playerKey))
            CurrentPlayerKey = playerKey;
        await MakeRequest("RejoinGame", new RejoinGameRequest(CurrentGameGuid!, CurrentPlayerGuid!, CurrentPlayerKey!, MultiplayerVersion.Id));
    }

    /// <summary>
    /// Forfeits a player from a game. If the game hasn't started, it will simply remove them from the game. If the game has started
    /// it will update the player state so that all other players know to collect all their items
    /// </summary>
    /// <param name="playerGuid">The player to forfeit. If no value is provided, it will use the local player's guid</param>
    public async Task ForfeitPlayerGame(string? playerGuid)
    {
        playerGuid ??= CurrentPlayerGuid;
        await MakeRequest("ForfeitPlayerGame",
            new ForfeitPlayerGameRequest(playerGuid ?? ""));
    }

    /// <summary>
    /// Marks the player as having completed the game
    /// </summary>
    public async Task CompletePlayerGame()
    {
        await MakeRequest("CompletePlayerGame", new CompletePlayerGameRequest());
    }

    /// <summary>
    /// Updates the current game's status
    /// </summary>
    /// <param name="gameStatus">The new status to update. Set to null to not update.</param>
    public async Task UpdateGameStatus(MultiplayerGameStatus? gameStatus)
    {
        await MakeRequest("UpdateGameState",
            new UpdateGameStateRequest(gameStatus));
    }

    /// <summary>
    /// Starts the game by sending rom generation details
    /// </summary>
    /// <param name="validationHash">A hash with all location data for verifying that all players are synced up</param>
    public async Task StartGame(string validationHash)
    {
        await MakeRequest("StartGame", new StartGameRequest(validationHash));
    }

    /// <summary>
    /// Notifies the server that a location has been cleared
    /// </summary>
    /// <param name="locationId">The id of the location that was cleared</param>
    /// <param name="playerGuid">The player whose dungeon was cleared. Set to null to use the local player.</param>
    public async Task TrackLocation(int locationId, string? playerGuid = null)
    {
        playerGuid ??= CurrentPlayerGuid;
        await MakeRequest("TrackLocation", new TrackLocationRequest(playerGuid ?? CurrentPlayerGuid!, locationId));
    }

    /// <summary>
    /// Notifies the server that an item has been tracked
    /// </summary>
    /// <param name="itemType">The item type that has been received</param>
    /// <param name="trackedValue">The new tracking state of the item</param>
    /// <param name="playerGuid">The player whose dungeon was cleared. Set to null to use the local player.</param>
    public async Task TrackItem(ItemType itemType, int trackedValue, string? playerGuid = null)
    {
        playerGuid ??= CurrentPlayerGuid;
        await MakeRequest("TrackItem", new TrackItemRequest(playerGuid ?? CurrentPlayerGuid!, itemType, trackedValue));
    }

    /// <summary>
    /// Notifies the server that a dungeon has been cleared
    /// </summary>
    /// <param name="dungeonName">The name of the dungeon that has been cleared</param>
    /// <param name="playerGuid">The player whose dungeon was cleared. Set to null to use the local player.</param>
    public async Task TrackDungeon(string dungeonName, string? playerGuid = null)
    {
        playerGuid ??= CurrentPlayerGuid;
        await MakeRequest("TrackDungeon", new TrackDungeonRequest(playerGuid ?? CurrentPlayerGuid!, dungeonName));
    }

    /// <summary>
    /// Notifies the server that a boss has been defeated
    /// </summary>
    /// <param name="bossType">The boss that has been defeated</param>
    /// <param name="playerGuid">The player whose dungeon was cleared. Set to null to use the local player.</param>
    public async Task TrackBoss(BossType bossType, string? playerGuid = null)
    {
        playerGuid ??= CurrentPlayerGuid;
        await MakeRequest("TrackBoss", new TrackBossRequest(playerGuid ?? CurrentPlayerGuid!, bossType));
    }

    /// <summary>
    /// Submits the world data for a player so that they can generate their seed
    /// </summary>
    /// <param name="playerGuid">The player guid the data is applicable to</param>
    /// <param name="data">The object of all of the details needed for the player to generate their rom</param>
    public async Task SubmitPlayerGenerationData(string playerGuid, MultiplayerPlayerGenerationData data)
    {
        await MakeRequest("SubmitPlayerGenerationData", new SubmitPlayerGenerationDataRequest(playerGuid, data.WorldId, MultiplayerPlayerGenerationData.ToString(data)));
    }

    /// <summary>
    /// Requests the current player list from the server
    /// </summary>
    /// <param name="sendToAllPlayers">If the list should be sent to all players</param>
    public async Task RequestPlayerList(bool sendToAllPlayers)
    {
        await MakeRequest("PlayerListSync", new PlayerListSyncRequest(sendToAllPlayers));
    }

    /// <summary>
    /// Requests a player to send an update
    /// </summary>
    /// <param name="playerGuid">The player guid to request</param>
    public async Task RequestPlayerUpdate(string? playerGuid)
    {
        await MakeRequest("RequestPlayerUpdate", new RequestPlayerUpdateRequest(playerGuid));
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// The connection to the server has been closed
    /// </summary>
    /// <param name="arg">The exception generated from the connection being closed</param>
    /// <returns></returns>
    private Task ConnectionOnClosed(Exception? arg)
    {
        _logger.LogWarning("Connection closed");
        ConnectionClosed?.Invoke("Connection closed", arg);
        return Task.CompletedTask;
    }

    /// <summary>
    /// On retrieving a full list of all player states from the servers
    /// </summary>
    /// <param name="response"></param>
    private async Task OnPlayerListSync(PlayerListSyncResponse response)
    {
        _logger.LogInformation("Player list sync from server. Num Players: {PlayerCount}", response.AllPlayers.Count);
        UpdatePlayerList(response.AllPlayers);
        await UpdateLocalGameState(response.GameState);
    }

    /// <summary>
    /// On retrieving an error message from the server to display to the user
    /// </summary>
    /// <param name="response"></param>
    private void OnError(ErrorResponse response)
    {
        Error?.Invoke(response.Error);
    }

    /// <summary>
    /// On retrieving an update to the game's state
    /// </summary>
    /// <param name="response"></param>
    private async Task OnUpdateGameState(UpdateGameStateResponse response)
    {
        await UpdateLocalGameState(response.GameState);
        GameStateUpdated?.Invoke();
    }

    /// <summary>
    /// On successfully creating a new game
    /// </summary>
    /// <param name="response"></param>
    private async Task OnCreateGame(CreateGameResponse response)
    {
        _logger.LogInformation("Game Created | Game Id: {GameGuid} | Player Guid: {PlayergGuid} | Player Key: {PlayerKey}", response.GameState.Guid, response.PlayerGuid, response.PlayerKey);
        _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Select(x => x.PlayerName)));
        CurrentGameGuid = response.GameState.Guid;
        CurrentPlayerGuid = response.PlayerGuid;
        CurrentPlayerKey = response.PlayerKey;
        GameCreated?.Invoke();
        UpdatePlayerList(response.AllPlayers);
        await UpdateLocalGameState(response.GameState);
        await SaveGameToDatabase(response.GameState, response.PlayerGuid, response.PlayerKey);
    }

    /// <summary>
    /// On successfully joining an active game
    /// </summary>
    /// <param name="response"></param>
    private async Task OnJoinGame(JoinGameResponse response)
    {
        _logger.LogInformation("Game joined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
        _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Select(x => x.PlayerName)));
        CurrentGameGuid = response.GameState.Guid;
        CurrentPlayerGuid = response.PlayerGuid;
        CurrentPlayerKey = response.PlayerKey;
        GameJoined?.Invoke();
        UpdatePlayerList(response.AllPlayers);
        await UpdateLocalGameState(response.GameState);
        await SaveGameToDatabase(response.GameState, response.PlayerGuid, response.PlayerKey);
    }

    /// <summary>
    /// On successfully rejoining a game
    /// </summary>
    /// <param name="response"></param>
    private async Task OnRejoinGame(RejoinGameResponse response)
    {
        _logger.LogInformation("Game rejoined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
        _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Select(x => x.PlayerName)));
        CurrentGameGuid = response.GameState.Guid;
        CurrentPlayerGuid = response.PlayerGuid;
        CurrentPlayerKey = response.PlayerKey;
        UpdatePlayerList(response.AllPlayers);
        await UpdateLocalGameState(response.GameState);
        GameRejoined?.Invoke();

        // If the game has started but we don't have any generation data, request it from the server
        if (response.GameState.Status == MultiplayerGameStatus.Started && DatabaseGameDetails?.GeneratedRom == null)
        {
            await MakeRequest("RequestPlayerGenerationData", new MultiplayerRequest());
        }
    }

    /// <summary>
    /// On forfeiting from a game
    /// </summary>
    /// <param name="response"></param>
    private async Task OnForfeitPlayerGame(ForfeitPlayerGameResponse response)
    {
        _logger.LogInformation("Forfeit game");
        var isLocalPlayer = response.PlayerState.Guid == CurrentPlayerGuid;
        if (isLocalPlayer && response.GameState.Status == MultiplayerGameStatus.Created && DatabaseGameDetails != null)
        {
            _dbContext.MultiplayerGames.Remove(DatabaseGameDetails);
            await _dbContext.SaveChangesAsync();
        }
        PlayerForfeited?.Invoke(response.PlayerState, isLocalPlayer);
        await UpdateLocalGameState(response.GameState);
        UpdateLocalPlayerState(response.PlayerState);

    }

    /// <summary>
    /// On forfeiting from a game
    /// </summary>
    /// <param name="response"></param>
    private async Task OnCompletePlayerGame(CompletePlayerGameResponse response)
    {
        _logger.LogInformation("Complete game");
        var isLocalPlayer = response.PlayerState.Guid == CurrentPlayerGuid;
        PlayerCompleted?.Invoke(response.PlayerState, isLocalPlayer);
        await UpdateLocalGameState(response.GameState);
        UpdateLocalPlayerState(response.PlayerState);
    }

    /// <summary>
    /// On retrieving a player's full status object of all things that have been tracked for them
    /// </summary>
    /// <param name="response"></param>
    private async Task OnPlayerSync(PlayerSyncResponse response)
    {
        _logger.LogInformation("Received state for {PlayerName}", response.PlayerState.PlayerName);
        await UpdateLocalGameState(response.GameState);
        UpdateLocalPlayerState(response.PlayerState);
    }

    /// <summary>
    /// One retrieving a player defeating a boss
    /// </summary>
    /// <param name="response"></param>
    private void OnTrackBoss(TrackBossResponse response)
    {
        var player = Players!.First(x => x.Guid == response.PlayerGuid);
        player.TrackBoss(response.BossType);
        _logger.LogInformation("{Player} tracked boss {BossType}", player.PlayerName, response.BossType);
        BossTracked?.Invoke(player, response.BossType);
    }

    /// <summary>
    /// On retrieving a player clearing a dungeon
    /// </summary>
    /// <param name="response"></param>
    private void OnTrackDungeon(TrackDungeonResponse response)
    {
        var player = Players!.First(x => x.Guid == response.PlayerGuid);
        player.TrackDungeon(response.DungeonName);
        _logger.LogInformation("{Player} tracked dungeon {DungeonName}", player.PlayerName, response.DungeonName);
        DungeonTracked?.Invoke(player, response.DungeonName);
    }

    /// <summary>
    /// On retrieving a player tracking an item
    /// </summary>
    /// <param name="response"></param>
    private void OnTrackItem(TrackItemResponse response)
    {
        var player = Players!.First(x => x.Guid == response.PlayerGuid);
        player.TrackItem(response.ItemType, response.TrackedValue);
        _logger.LogInformation("{Player} tracked item {ItemType}", player.PlayerName, response.ItemType);
        ItemTracked?.Invoke(player, response.ItemType, response.TrackedValue);
    }

    /// <summary>
    /// On retrieving a player clearing a location
    /// </summary>
    /// <param name="response"></param>
    private void OnTrackLocation(TrackLocationResponse response)
    {
        var player = Players!.First(x => x.Guid == response.PlayerGuid);
        player.TrackLocation(response.LocationId);
        _logger.LogInformation("{Player} tracked location {LocationId}", player.PlayerName, response.LocationId);
        LocationTracked?.Invoke(player, response.LocationId);
    }

    /// <summary>
    /// On retrieving details for regenerating the worlds and creating the rom for the player
    /// </summary>
    /// <param name="response"></param>
    private async Task OnStartGame(StartGameResponse response)
    {
        _logger.LogInformation("Received start game");
        await UpdateLocalGameState(response.GameState);
        UpdatePlayerList(response.AllPlayers);
        var data = response.PlayerGenerationData.Select(MultiplayerPlayerGenerationData.FromString).NonNull().ToList();
        GameStarted?.Invoke(data);
    }

    /// <summary>
    /// On a player's state is being requested by the server
    /// </summary>
    /// <param name="response"></param>
    private void OnRequestPlayerUpdate(RequestPlayerUpdateRequest response)
    {
        _logger.LogInformation("Player state requested");
        PlayerStateRequested?.Invoke();
    }
    #endregion

    #region Helper functions
    private void UpdatePlayerList(List<MultiplayerPlayerState>? players)
    {
        Players = players;
        PlayerListUpdated?.Invoke();
    }

    private async Task UpdateLocalGameState(MultiplayerGameState gameState)
    {
        CurrentGameState = gameState;
        await UpdateLocalGameStatus();
        GameStateUpdated?.Invoke();
    }

    private void UpdateLocalPlayerState(MultiplayerPlayerState playerState)
    {
        var previous = Players?.FirstOrDefault(x => x.Guid == playerState.Guid);
        if (previous != null) Players?.Remove(previous);

        if (playerState.HasForfeited && CurrentGameState?.Status == MultiplayerGameStatus.Created)
        {
            UpdatePlayerList(Players);
            return;
        }

        Players?.Add(playerState);
        PlayerUpdated?.Invoke(playerState, previous, playerState.Guid == CurrentGameGuid);
    }

    private bool VerifyConnection()
    {
        if (IsConnected) return true;
        Error?.Invoke($"You are not currently connected to a server.");
        return false;
    }

    private bool VerifyJoinedGame()
    {
        if (HasJoinedGame()) return true;
        Error?.Invoke($"You are not currently connected to a game.");
        return false;
    }

    private async Task MakeRequest(string methodName, object? argument = null, bool requireJoined = false)
    {
        if (!VerifyConnection()) return;
        if (requireJoined && !VerifyJoinedGame()) return;
        try
        {
            await _connection!.InvokeAsync(methodName, argument);
        }
        catch (Exception e)
        {
            if (e is WebSocketException or HubException or TimeoutException or InvalidOperationException)
            {
                _logger.LogError(e, "Connection to server lost");
                Error?.Invoke($"Connection to server lost");
            }
            else
            {
                _logger.LogError(e, "Could not make request");
            }
        }

    }

    private async Task SaveGameToDatabase(MultiplayerGameState gameState, string playerGuid, string playerKey)
    {
        DatabaseGameDetails = new MultiplayerGameDetails
        {
            ConnectionUrl = gameState.Url,
            GameGuid = gameState.Guid,
            GameUrl = gameState.Url,
            Type = gameState.Type,
            Status = gameState.Status,
            PlayerGuid = playerGuid,
            PlayerKey = playerKey,
            JoinedDate = DateTimeOffset.Now,
        };
        _dbContext.MultiplayerGames.Add(DatabaseGameDetails);
        await _dbContext.SaveChangesAsync();
    }

    private async Task UpdateLocalGameStatus()
    {
        if (DatabaseGameDetails == null || CurrentGameState == null) return;
        DatabaseGameDetails.Status = CurrentGameState.Status;
        await _dbContext.SaveChangesAsync();
    }

    #endregion
}
