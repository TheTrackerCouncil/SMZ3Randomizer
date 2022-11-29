using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Models;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client
{
    /// <summary>
    /// SignalR service for interacting with the SMZ3 multiplayer server
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
        public event MultiplayerGenericEventHandler? GameForfeit;
        public event MultiplayerGenericEventHandler? PlayerListUpdated;
        public event MultiplayerGenericEventHandler? GameStateUpdated;
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

            ConnectionUrl = url;

            if (_connection != null && _connection.State != HubConnectionState.Disconnected)
            {
                await _connection.DisposeAsync();
            }

            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            _connection.On<CreateGameResponse>("CreateGame", OnCreateGame);

            _connection.On<JoinGameResponse>("JoinGame", OnJoinGame);

            _connection.On<RejoinGameResponse>("RejoinGame", OnRejoinGame);

            _connection.On<ForfeitGameResponse>("ForfeitGame", OnForfeitGame);

            _connection.On<PlayerSyncResponse>("PlayerSync", OnPlayerSync);

            _connection.On<PlayerListSyncResponse>("PlayerListSync", OnPlayerListSync);

            _connection.On<UpdateGameStateResponse>("UpdateGameState", OnUpdateGameState);

            _connection.On<TrackLocationResponse>("TrackLocation", OnTrackLocation);

            _connection.On<TrackItemResponse>("TrackItem", OnTrackItem);

            _connection.On<TrackDungeonResponse>("TrackDungeon", OnTrackDungeon);

            _connection.On<TrackBossResponse>("TrackBoss", OnTrackBoss);

            _connection.On<StartGameResponse>("StartGame", OnStartGame);

            _connection.On<ErrorResponse>("Error", OnError);

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
            if (_connection == null || string.IsNullOrEmpty(ConnectionUrl))
            {
                Error?.Invoke($"Could not reconnect as you were not previously connected.");
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
        /// <param name="gameType">The requested game type</param>
        /// <param name="version">The local SMZ3 version to ensure compatibility between all players</param>
        public async Task CreateGame(string playerName, MultiplayerGameType gameType, string version)
        {
            await MakeRequest("CreateGame", new CreateGameRequest(playerName, gameType, version));
        }

        /// <summary>
        /// Joins a multiplayer game
        /// </summary>
        /// <param name="gameGuid">The unique identifier of the game</param>
        /// <param name="playerName">The requested player name</param>
        /// <param name="version">The local SMZ3 version to ensure compatibility between all players</param>
        public async Task JoinGame(string gameGuid, string playerName, string version)
        {
            await MakeRequest("JoinGame", new JoinGameRequest(gameGuid, playerName, version));
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

            LocalPlayer.Config = config;
            await UpdatePlayerState(LocalPlayer);
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
            await MakeRequest("RejoinGame", new RejoinGameRequest(CurrentGameGuid!, CurrentPlayerGuid!, CurrentPlayerKey!));
        }

        /// <summary>
        /// Forfeits a player from a game. If the game hasn't started, it will simply remove them from the game. If the game has started
        /// it will update the player state so that all other players know to collect all their items
        /// </summary>
        /// <param name="playerGuid">The player to forfeit. If no value is provided, it will use the local player's guid</param>
        public async Task ForfeitGame(string? playerGuid)
        {
            playerGuid ??= CurrentPlayerGuid;
            await MakeRequest("ForfeitGame",
                new ForfeitGameRequest(playerGuid ?? ""));
        }

        /// <summary>
        /// Updates the current game's status
        /// </summary>
        /// <param name="gameStatus">The new status to update. Set to null to not update.</param>
        public async Task UpdateGameState(MultiplayerGameStatus? gameStatus)
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

        public async Task TrackLocation(int locationId, string? playerGuid = null)
        {
            playerGuid ??= CurrentPlayerGuid;
            await MakeRequest("TrackLocation", new TrackLocationRequest(playerGuid ?? CurrentPlayerGuid!, locationId));
        }

        public async Task TrackItem(ItemType itemType, int trackedValue, string? playerGuid = null)
        {
            playerGuid ??= CurrentPlayerGuid;
            await MakeRequest("TrackItem", new TrackItemRequest(playerGuid ?? CurrentPlayerGuid!, itemType, trackedValue));
        }

        public async Task TrackDungeon(string dungeonName, string? playerGuid = null)
        {
            playerGuid ??= CurrentPlayerGuid;
            await MakeRequest("TrackDungeon", new TrackDungeonRequest(playerGuid ?? CurrentPlayerGuid!, dungeonName));
        }

        public async Task TrackBoss(BossType bossType, string? playerGuid = null)
        {
            playerGuid ??= CurrentPlayerGuid;
            await MakeRequest("TrackBoss", new TrackBossRequest(playerGuid ?? CurrentPlayerGuid!, bossType));
        }

        public async Task SubmitPlayerGenerationData(string playerGuid, MultiplayerPlayerGenerationData data)
        {
            await MakeRequest("SubmitPlayerGenerationData", new SubmitPlayerGenerationDataRequest(playerGuid, MultiplayerPlayerGenerationData.ToString(data)));
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
        private void OnPlayerListSync(PlayerListSyncResponse response)
        {
            _logger.LogInformation("Player list sync from server. Num Players: {PlayerCount}", response.AllPlayers.Count);
            UpdatePlayerList(response.AllPlayers);
            UpdateGameState(response.GameState);
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
        private void OnUpdateGameState(UpdateGameStateResponse response)
        {
            UpdateGameState(response.GameState);
            GameStateUpdated?.Invoke();
        }

        /// <summary>
        /// On successfully creating a new game
        /// </summary>
        /// <param name="response"></param>
        private async void OnCreateGame(CreateGameResponse response)
        {
            _logger.LogInformation("Game Created | Game Id: {GameGuid} | Player Guid: {PlayergGuid} | Player Key: {PlayerKey}", response.GameState.Guid, response.PlayerGuid, response.PlayerKey);
            _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Select(x => x.PlayerName)));
            CurrentGameGuid = response.GameState.Guid;
            CurrentPlayerGuid = response.PlayerGuid;
            CurrentPlayerKey = response.PlayerKey;
            GameCreated?.Invoke();
            UpdatePlayerList(response.AllPlayers);
            UpdateGameState(response.GameState);
            await SaveGameToDatabase(response.GameState, response.PlayerGuid, response.PlayerKey);
        }

        /// <summary>
        /// On successfully joining an active game
        /// </summary>
        /// <param name="response"></param>
        private async void OnJoinGame(JoinGameResponse response)
        {
            _logger.LogInformation("Game joined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
            _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Select(x => x.PlayerName)));
            CurrentGameGuid = response.GameState.Guid;
            CurrentPlayerGuid = response.PlayerGuid;
            CurrentPlayerKey = response.PlayerKey;
            GameJoined?.Invoke();
            UpdatePlayerList(response.AllPlayers);
            UpdateGameState(response.GameState);
            await SaveGameToDatabase(response.GameState, response.PlayerGuid, response.PlayerKey);
        }

        /// <summary>
        /// On successfully rejoining a game
        /// </summary>
        /// <param name="response"></param>
        private async void OnRejoinGame(RejoinGameResponse response)
        {
            _logger.LogInformation("Game rejoined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
            _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Select(x => x.PlayerName)));
            CurrentGameGuid = response.GameState.Guid;
            CurrentPlayerGuid = response.PlayerGuid;
            CurrentPlayerKey = response.PlayerKey;
            GameRejoined?.Invoke();
            UpdatePlayerList(response.AllPlayers);
            UpdateGameState(response.GameState);

            if (response.GameState.Status == MultiplayerGameStatus.Started && DatabaseGameDetails?.GeneratedRom == null)
            {
                await MakeRequest("RequestPlayerGenerationData");
            }
        }

        /// <summary>
        /// On forfeiting from a game
        /// </summary>
        /// <param name="response"></param>
        private void OnForfeitGame(ForfeitGameResponse response)
        {
            _logger.LogInformation("Forfeit game");
            UpdateGameState(response.GameState);
            GameForfeit?.Invoke();
        }

        /// <summary>
        /// On retrieving a player's full status object
        /// </summary>
        /// <param name="response"></param>
        private void OnPlayerSync(PlayerSyncResponse response)
        {
            _logger.LogInformation("Received state for {PlayerName}", response.PlayerState.PlayerName);
            var previous = Players?.FirstOrDefault(x => x.Guid == response.PlayerState.Guid);
            if (previous != null) Players?.Remove(previous);
            Players?.Add(response.PlayerState);
            UpdateGameState(response.GameState);
            PlayerUpdated?.Invoke(response.PlayerState, previous);
        }

        private void OnTrackBoss(TrackBossResponse obj)
        {
            var player = Players!.First(x => x.Guid == obj.PlayerGuid);
            player.TrackBoss(obj.BossType);
            _logger.LogInformation("{Player} tracked boss {BossType}", player.PlayerName, obj.BossType);
            BossTracked?.Invoke(player, obj.BossType);
        }

        private void OnTrackDungeon(TrackDungeonResponse obj)
        {
            var player = Players!.First(x => x.Guid == obj.PlayerGuid);
            player.TrackDungeon(obj.DungeonName);
            _logger.LogInformation("{Player} tracked dungeon {DungeonName}", player.PlayerName, obj.DungeonName);
            DungeonTracked?.Invoke(player, obj.DungeonName);
        }

        private void OnTrackItem(TrackItemResponse obj)
        {
            var player = Players!.First(x => x.Guid == obj.PlayerGuid);
            player.TrackItem(obj.ItemType, obj.TrackedValue);
            _logger.LogInformation("{Player} tracked item {ItemType}", player.PlayerName, obj.ItemType);
            ItemTracked?.Invoke(player, obj.ItemType, obj.TrackedValue);
        }

        private void OnTrackLocation(TrackLocationResponse obj)
        {
            var player = Players!.First(x => x.Guid == obj.PlayerGuid);
            player.TrackLocation(obj.LocationId);
            _logger.LogInformation("{Player} tracked location {LocationId}", player.PlayerName, obj.LocationId);
            LocationTracked?.Invoke(player, obj.LocationId);
        }

        private void OnStartGame(StartGameResponse obj)
        {
            _logger.LogInformation("Received start game");
            UpdateGameState(obj.GameState);
            UpdatePlayerList(obj.AllPlayers);
            var data = obj.PlayerGenerationData.Select(MultiplayerPlayerGenerationData.FromString).NonNull().ToList();
            GameStarted?.Invoke(data);
        }
        #endregion

        #region Helper functions
        private void UpdatePlayerList(List<MultiplayerPlayerState>? players)
        {
            Players = players;
            PlayerListUpdated?.Invoke();
        }

        private void UpdateGameState(MultiplayerGameState gameState)
        {

            CurrentGameState = gameState;
            GameStateUpdated?.Invoke();
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
            catch (Exception e) when (e is WebSocketException or HubException or TimeoutException)
            {
                _logger.LogError(e, "Connection to server lost");
                Error?.Invoke($"Connection to server lost");
            }
        }

        private async Task SaveGameToDatabase(MultiplayerGameState gameState, string playerGuid, string playerKey)
        {
            DatabaseGameDetails = new MultiplayerGameDetails
            {
                ConnectionUrl = ConnectionUrl!,
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

        #endregion
    }
}
