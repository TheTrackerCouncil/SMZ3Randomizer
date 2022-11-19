using System.Net.WebSockets;
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
        public event MultiplayerGenericEventHandler? GameStatusUpdated;
        public event PlayerUpdatedEventHandler? PlayerUpdated;

        public string? CurrentGameGuid { get; private set; }
        public string? CurrentPlayerGuid { get; private set; }
        public string? CurrentPlayerKey { get; private set; }
        public List<MultiplayerPlayerState>? Players { get; set; }
        public MultiplayerPlayerState? LocalPlayer => Players?.FirstOrDefault(x => x.Guid == CurrentPlayerGuid);
        public string? GameUrl { get; private set; }
        public string? ConnectionUrl { get; private set; }
        public MultiplayerGameStatus? GameStatus { get; private set; }
        public MultiplayerGameType? GameType { get; private set; }

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

            _connection.On<PlayerJoinedResponse>("PlayerJoined", OnPlayerJoined);

            _connection.On<PlayerRejoinedResponse>("PlayerRejoined", OnPlayerRejoined);

            _connection.On<PlayerForfeitedResponse>("PlayerForfeited", OnPlayerForfeit);

            _connection.On<SubmitConfigResponse>("SubmitConfig", OnSubmitConfig);

            _connection.On<PlayerSyncResponse>("PlayerSync", OnPlayerSync);

            _connection.On<UpdateGameStatusResponse>("UpdateGameStatus", OnUpdateGameStatus);

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

        private void OnUpdateGameStatus(UpdateGameStatusResponse response)
        {
            if (response.IsValid)
            {
                GameStatus = response.GameStatus;
                GameStatusUpdated?.Invoke();
            }
            else
            {
                _logger.LogError("Unable to update game status: {Error}", response.Error);
                Error?.Invoke($"Unable to update game status: {response.Error}");
            }
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
            GameStatus = null;
            if (_connection == null) return;
            await _connection!.DisposeAsync();
            _connection = null;
        }

        public async Task CreateGame(string playerName, MultiplayerGameType gameType)
        {
            GameType = gameType;
            await MakeRequest("CreateGame", new CreateGameRequest(playerName, gameType));
        }

        public async Task JoinGame(string gameGuid, string playerName)
        {
            CurrentGameGuid = gameGuid;
            await MakeRequest("JoinGame", new JoinGameRequest(gameGuid, playerName));
        }

        public async Task SubmitConfig(string config)
        {
            await MakeRequest("SubmitConfig", new SubmitConfigRequest(CurrentGameGuid!, CurrentPlayerGuid!, CurrentPlayerKey!, config), true);
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

        public async Task StartGame(List<MultiplayerPlayerState> playerStates)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("Not connected");
                return;
            }

            await MakeRequest("StartGame", new StartGameRequest(playerStates));
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
            if (response.IsValid)
            {
                _logger.LogInformation("Game Created | Game Id: {GameGuid} | Player Guid: {PlayergGuid} | Player Key: {PlayerKey}", response.GameGuid, response.PlayerGuid, response.PlayerKey);
                _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers!.Select(x => x.PlayerName)));
                CurrentGameGuid = response.GameGuid;
                CurrentPlayerGuid = response.PlayerGuid;
                CurrentPlayerKey = response.PlayerKey;
                GameUrl = response.GameUrl;
                GameCreated?.Invoke();
                UpdateStatus(response.GameStatus);
                UpdatePlayerList(response.AllPlayers);
            }
            else
            {
                _logger.LogError("Unable to join game: {Error}", response.Error);
                Error?.Invoke($"Unable to join game: {response.Error}");
            }
        }

        private void OnJoinGame(JoinGameResponse response)
        {
            if (response.IsValid)
            {
                _logger.LogInformation("Game joined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
                _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers!.Select(x => x.PlayerName)));
                CurrentPlayerGuid = response.PlayerGuid;
                CurrentPlayerKey = response.PlayerKey;
                GameUrl = response.GameUrl;
                GameType = response.GameType;
                GameJoined?.Invoke();
                UpdateStatus(response.GameStatus);
                UpdatePlayerList(response.AllPlayers);
            }
            else
            {
                _logger.LogError("Unable to join game: {Error}", response.Error);
                Error?.Invoke($"Unable to join game: {response.Error}");
            }
        }

        private void OnRejoinGame(RejoinGameResponse response)
        {
            if (response.IsValid)
            {
                _logger.LogInformation("Game rejoined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
                _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers!.Select(x => x.PlayerName)));
                CurrentPlayerGuid = response.PlayerGuid;
                CurrentPlayerKey = response.PlayerKey;
                GameUrl = response.GameUrl;
                GameType = response.GameType;
                GameRejoined?.Invoke();
                UpdateStatus(response.GameStatus);
                UpdatePlayerList(response.AllPlayers);
            }
            else
            {
                _logger.LogError("Unable to join game: {Error}", response.Error);
                Error?.Invoke($"Unable to join game: {response.Error}");
            }
        }

        private void OnPlayerJoined(PlayerJoinedResponse response)
        {
            if (response.IsValid)
            {
                _logger.LogInformation("Player joined | Player Guid: {PlayerGuid} | Player Name: {PlayerName}", response.PlayerGuid, response.PlayerName);
                _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers!.Select(x => x.PlayerName)));
                UpdateStatus(response.GameStatus);
                UpdatePlayerList(response.AllPlayers);
            }
            else
            {
                _logger.LogError("Received invalid player joined response");
            }
        }

        private void OnPlayerRejoined(PlayerRejoinedResponse response)
        {
            if (response.IsValid)
            {
                _logger.LogInformation("Player joined | Player Guid: {PlayerGuid} | Player Name: {PlayerName}", response.PlayerGuid, response.PlayerName);
                _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers!.Select(x => x.PlayerName)));
                UpdateStatus(response.GameStatus);
                UpdatePlayerList(response.AllPlayers);
            }
            else
            {
                _logger.LogError("Received invalid player joined response");
            }
        }

        private void OnPlayerForfeit(PlayerForfeitedResponse response)
        {
            if (response.IsValid)
            {
                _logger.LogInformation("Player forfeit | Player Guid: {PlayerGuid} | Player Name: {PlayerName}", response.PlayerGuid, response.PlayerName);
                _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers!.Select(x => x.PlayerName)));
                UpdateStatus(response.GameStatus);
                UpdatePlayerList(response.AllPlayers);
            }
            else
            {
                _logger.LogError("Received invalid player forfeit response");
            }
        }

        private void OnForfeitGame(ForfeitGameResponse response)
        {
            if (response.IsValid)
            {
                _logger.LogInformation("Forfeit game");
                UpdateStatus(response.GameStatus);
                UpdatePlayerList(response.AllPlayers);
                GameForfeit?.Invoke();
            }
        }

        private void OnSubmitConfig(SubmitConfigResponse response)
        {
            if (response.IsValid)
            {
                _logger.LogInformation("Player submitted config");
            }
            else
            {
                _logger.LogError("Unable to submit config game: {Error}", response.Error);
                Error?.Invoke($"Unable to submit game: {response.Error}");
            }
        }

        private void OnPlayerSync(PlayerSyncResponse response)
        {
            if (response.IsValid)
            {
                _logger.LogInformation("Received state for {PlayerName}", response.PlayerState!.PlayerName);
                var previous = Players?.FirstOrDefault(x => x.Guid == response.PlayerState.Guid);
                if (previous != null) Players?.Remove(previous);
                Players?.Add(response.PlayerState);
                UpdateStatus(response.GameStatus);
                PlayerUpdated?.Invoke(response.PlayerState);
            }
            else
            {
                _logger.LogError("Error getting player sync value: {Error}", response.Error);
            }
        }

        private async Task MakeRequest(string methodName, object? argument = null, bool requireJoined = false)
        {
            if (!VerifyConnection()) return;
            if (requireJoined && !VerifyJoinedGame()) return;
            try
            {
                await _connection!.InvokeAsync(methodName, argument);
            }
            catch (WebSocketException e)
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

        private void UpdateStatus(MultiplayerGameStatus gameStatus)
        {
            if (gameStatus == GameStatus) return;
            GameStatus = gameStatus;
            GameStatusUpdated?.Invoke();
        }
    }
}
