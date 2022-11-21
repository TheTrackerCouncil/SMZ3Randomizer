using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Client
{
    public class MultiplayerClientService
    {
        private readonly ILogger<MultiplayerClientService> _logger;
        private HubConnection? _connection;

        public MultiplayerClientService(ILogger<MultiplayerClientService> logger)
        {
            _logger = logger;
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
        public event SeedDetailsRetrievedEventHandler? SeedDataGenerated;

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

            _connection.On<SubmitConfigResponse>("SubmitConfig", OnSubmitConfig);

            _connection.On<PlayerSyncResponse>("PlayerSync", OnPlayerSync);

            _connection.On<PlayerListSyncResponse>("PlayerListSync", OnPlayerListSync);

            _connection.On<UpdateGameStatusResponse>("UpdateGameStatus", OnUpdateGameStatus);

            _connection.On<ErrorResponse>("Error", OnError);

            _connection.Reconnected += ConnectionOnReconnected;

            _connection.Reconnecting += ConnectionOnReconnecting;

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

        private void OnPlayerListSync(PlayerListSyncResponse response)
        {
            _logger.LogInformation("Player list sync from server. Num Players: {PlayerCount}", response.AllPlayers.Count);
            UpdatePlayerList(response.AllPlayers);
            UpdateGameState(response.GameState);
        }

        private void OnError(ErrorResponse response)
        {
            Error?.Invoke(response.Error);
        }

        private void OnUpdateGameStatus(UpdateGameStatusResponse response)
        {
            UpdateGameState(response.GameState);
            GameStateUpdated?.Invoke();
        }

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

        private Task ConnectionOnReconnecting(Exception? arg)
        {
            _logger.LogInformation("Reconnecting");
            return Task.CompletedTask;
        }

        private Task ConnectionOnClosed(Exception? arg)
        {
            _logger.LogWarning("Connection closed");
            ConnectionClosed?.Invoke("Connection closed", arg);
            return Task.CompletedTask;
        }

        private async Task ConnectionOnReconnected(string? arg)
        {
            _logger.LogInformation("Reconnected");
            await RejoinGame();
        }

        public async Task Disconnect()
        {
            CurrentGameGuid = null;
            CurrentPlayerGuid = null;
            CurrentPlayerKey = null;
            Players = null;
            CurrentGameState = null;
            if (_connection == null) return;
            await _connection!.DisposeAsync();
            _connection = null;
        }

        public async Task CreateGame(string playerName, MultiplayerGameType gameType)
        {
            await MakeRequest("CreateGame", new CreateGameRequest(playerName, gameType));
        }

        public async Task JoinGame(string gameGuid, string playerName)
        {
            await MakeRequest("JoinGame", new JoinGameRequest(gameGuid, playerName));
        }

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

        public async Task UpdatePlayerState(MultiplayerPlayerState state, bool propogate = true)
        {
            if (LocalPlayer == null)
            {
                Error?.Invoke($"There is no active local player");
                return;
            }

            await MakeRequest("UpdatePlayerState", new UpdatePlayerStateRequest(CurrentGameGuid!, CurrentPlayerGuid!, CurrentPlayerKey!, state, propogate), true);
        }

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

        public async Task ForfeitGame(string? playerGuid)
        {
            playerGuid ??= CurrentPlayerGuid;
            await MakeRequest("ForfeitGame",
                new ForfeitGameRequest(CurrentGameGuid ?? "", CurrentPlayerGuid ?? "", CurrentPlayerKey ?? "", playerGuid ?? ""));
        }

        public async Task UpdateGameStatus(MultiplayerGameStatus gameStatus)
        {
            await MakeRequest("UpdateGameStatus",
                new UpdateGameStatusRequest(CurrentGameGuid ?? "", CurrentPlayerGuid ?? "", CurrentPlayerKey ?? "", gameStatus));
        }

        public async Task StartGame(string seed, string validationHash)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("Not connected");
                return;
            }

            await MakeRequest("StartGame", new StartGameRequest(CurrentGameGuid ?? "", CurrentPlayerGuid ?? "", CurrentPlayerKey ?? "", seed, validationHash));
        }

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

        private void OnCreateGame(CreateGameResponse response)
        {
            _logger.LogInformation("Game Created | Game Id: {GameGuid} | Player Guid: {PlayergGuid} | Player Key: {PlayerKey}", response.GameState.Guid, response.PlayerGuid, response.PlayerKey);
            _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Select(x => x.PlayerName)));
            CurrentGameGuid = response.GameState.Guid;
            CurrentPlayerGuid = response.PlayerGuid;
            CurrentPlayerKey = response.PlayerKey;
            GameCreated?.Invoke();
            UpdatePlayerList(response.AllPlayers);
            UpdateGameState(response.GameState);
        }

        private void OnJoinGame(JoinGameResponse response)
        {
            _logger.LogInformation("Game joined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
            _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Select(x => x.PlayerName)));
            CurrentGameGuid = response.GameState.Guid;
            CurrentPlayerGuid = response.PlayerGuid;
            CurrentPlayerKey = response.PlayerKey;
            GameJoined?.Invoke();
            UpdatePlayerList(response.AllPlayers);
            UpdateGameState(response.GameState);
        }

        private void OnRejoinGame(RejoinGameResponse response)
        {
            _logger.LogInformation("Game rejoined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
            _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Select(x => x.PlayerName)));
            CurrentGameGuid = response.GameState.Guid;
            CurrentPlayerGuid = response.PlayerGuid;
            CurrentPlayerKey = response.PlayerKey;
            GameRejoined?.Invoke();
            UpdatePlayerList(response.AllPlayers);
            UpdateGameState(response.GameState);
        }

        private void OnForfeitGame(ForfeitGameResponse response)
        {
            _logger.LogInformation("Forfeit game");
            UpdateGameState(response.GameState);
            GameForfeit?.Invoke();
        }

        private void OnSubmitConfig(SubmitConfigResponse response)
        {
            _logger.LogInformation("Player submitted config");
        }

        private void OnPlayerSync(PlayerSyncResponse response)
        {
            _logger.LogInformation("Received state for {PlayerName}", response.PlayerState.PlayerName);
            var previous = Players?.FirstOrDefault(x => x.Guid == response.PlayerState.Guid);
            if (previous != null) Players?.Remove(previous);
            Players?.Add(response.PlayerState);
            UpdateGameState(response.GameState);
            PlayerUpdated?.Invoke(response.PlayerState);
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

        private void UpdatePlayerList(List<MultiplayerPlayerState>? players)
        {
            Players = players;
            PlayerListUpdated?.Invoke();
        }

        private void UpdateGameState(MultiplayerGameState gameState)
        {
            if (CurrentGameState?.Status is MultiplayerGameStatus.Created or MultiplayerGameStatus.Generating &&
                gameState.Status == MultiplayerGameStatus.Started)
            {
                _logger.LogInformation("Rom generation information sent | Seed: {Seed} | Hash: {Hash}", gameState.Seed, gameState.ValidationHash);
                SeedDataGenerated?.Invoke(gameState.Seed, gameState.ValidationHash);
            }
            CurrentGameState = gameState;
            GameStateUpdated?.Invoke();
        }
    }
}
