using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Multiworld;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.Multiworld.Client
{
    public class MultiworldClientService
    {
        private readonly ILogger<MultiworldClientService> _logger;
        private HubConnection? _connection;

        public MultiworldClientService(ILogger<MultiworldClientService> logger)
        {
            _logger = logger;
        }

        public event ConnectedEventHandler? Connected;
        public event GameCreatedEventHandler? GameCreated;
        public event GameJoinedEventHandler? GameJoined;
        public event GameRejoinedEventHandler? GameRejoined;
        public event ErrorEventHandler? Error;
        public event PlayerJoinedEventHandler? PlayerJoined;
        public event PlayerRejoinedEventHandler? PlayerRejoined;
        public event PlayerSyncEventHandler? PlayerSync;
        public event ConnectionClosedEventHandler? ConnectionClosed;

        public string? CurrentGameGuid { get; private set; }
        public string? CurrentPlayerGuid { get; private set; }
        public string? CurrentPlayerKey { get; private set; }
        public List<MultiworldPlayerState>? Players { get; set; }
        public MultiworldPlayerState? LocalPlayer => Players?.FirstOrDefault(x => x.Guid == CurrentPlayerGuid);
        public string? GameUrl { get; private set; }
        public string? ConnectionUrl { get; private set; }

        public async Task Connect(string url)
        {
            //.WithUrl("http://www.celestialrealm.net:12000/chat")
            //.WithUrl("http://127.0.0.1:5291/multiworld")
            //.WithUrl("https://localhost:7050/multiworld")

            if (_connection != null && url == ConnectionUrl)
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

            _connection.On<PlayerJoinedResponse>("PlayerJoined", OnPlayerJoined);

            _connection.On<PlayerRejoinedResponse>("PlayerRejoined", OnPlayerRejoined);

            _connection.On<SubmitConfigResponse>("SubmitConfig", OnSubmitConfig);

            _connection.On<PlayerSyncResponse>("PlayerSync", OnPlayerSync);

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
            ConnectionClosed?.Invoke(arg);
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

            if (!IsConnected) return;
            await _connection!.DisposeAsync();
        }

        public async Task CreateGame(string playerName)
        {
            await MakeRequest("CreateGame", new CreateGameRequest(playerName));
        }

        public async Task JoinGame(string gameGuid, string playerName)
        {
            CurrentGameGuid = gameGuid;
            await MakeRequest("JoinGame", new JoinGameRequest(gameGuid, playerName));
        }

        public async Task SubmitConfig(Config config)
        {
            await MakeRequest("SubmitConfig", new SubmitConfigRequest(CurrentGameGuid!, CurrentPlayerGuid!, CurrentPlayerKey!, config), true);
        }

        public async Task RejoinGame(string? gameGuid = null, string? playerGuid = null, string? playerKey = null)
        {
            if (!string.IsNullOrEmpty(gameGuid))
                CurrentGameGuid = gameGuid;
            if (!string.IsNullOrEmpty(playerGuid))
                CurrentGameGuid = playerGuid;
            if (!string.IsNullOrEmpty(playerKey))
                CurrentGameGuid = playerKey;
            await MakeRequest("RejoinGame", new RejoinGameRequest(CurrentGameGuid!, CurrentPlayerGuid!, CurrentPlayerKey!));
        }

        public async Task StartGame(List<string> playerGuids, TrackerState trackerState)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("Not connected");
                return;
            }

            await MakeRequest("StartGame", new StartGameRequest(playerGuids, trackerState));
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
                GameCreated?.Invoke(response.GameGuid!, response.PlayerGuid!, response.PlayerKey!);
                CurrentGameGuid = response.GameGuid;
                CurrentPlayerGuid = response.PlayerGuid;
                CurrentPlayerKey = response.PlayerKey;
                Players = response.AllPlayers;
                GameUrl = response.GameUrl;
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
                Players = response.AllPlayers;
                GameUrl = response.GameUrl;
                GameJoined?.Invoke(response.PlayerGuid!, response.PlayerKey!);
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
                Players = response.AllPlayers;
                GameUrl = response.GameUrl;
                GameRejoined?.Invoke();
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
                Players = response.AllPlayers;
                PlayerJoined?.Invoke();
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
                Players = response.AllPlayers;
                PlayerRejoined?.Invoke();
            }
            else
            {
                _logger.LogError("Received invalid player joined response");
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
                PlayerSync?.Invoke(previous, response.PlayerState!);
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
    }
}
