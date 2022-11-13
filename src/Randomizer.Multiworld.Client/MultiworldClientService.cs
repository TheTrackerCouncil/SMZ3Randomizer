using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Multiworld;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.Multiworld.Client
{
    public class MultiworldClientService
    {

        private HubConnection? _connection;
        private ILogger<MultiworldClientService> _logger;

        public MultiworldClientService(ILogger<MultiworldClientService> logger)
        {
            _logger = logger;
        }

        public async Task Connect(string url)
        {
            //.WithUrl("http://www.celestialrealm.net:12000/chat")
            //.WithUrl("http://127.0.0.1:5291/multiworld")
            //.WithUrl("https://localhost:7050/multiworld")

            if (_connection != null && _connection.State != HubConnectionState.Disconnected)
            {
                await _connection.DisposeAsync();
            }

            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            _connection.On<CreateGameResponse>("CreateGame", (response) =>
            {
                if (response.IsValid)
                {
                    _logger.LogInformation("Game Created | Game Id: {GameGuid} | Player Guid: {PlayergGuid} | Player Key: {PlayerKey}",
                        response.GameGuid, response.PlayerGuid, response.PlayerKey);
                    _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers!.Select(x => x.PlayerName)));
                    GameCreated?.Invoke(response.GameGuid!, response.PlayerGuid!, response.PlayerKey!);
                    CurrentGameGuid = response.GameGuid;
                    CurrentPlayerGuid = response.PlayerGuid;
                    CurrentPlayerKey = response.PlayerKey;
                    Players = response.AllPlayers;
                }
                else
                {
                    _logger.LogError("Unable to join game: {Error}", response.Error);
                    Error?.Invoke($"Unable to join game: {response.Error}");
                }
            });

            _connection.On<JoinGameResponse>("JoinGame", (response) =>
            {
                if (response.IsValid)
                {
                    _logger.LogInformation("Game joined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
                    _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers!.Select(x => x.PlayerName)));
                    GameJoined?.Invoke(response.PlayerGuid!, response.PlayerKey!);
                    CurrentPlayerGuid = response.PlayerGuid;
                    CurrentPlayerKey = response.PlayerKey;
                    Players = response.AllPlayers;
                }
                else
                {
                    _logger.LogError("Unable to join game: {Error}", response.Error);
                    Error?.Invoke($"Unable to join game: {response.Error}");
                }
            });

            _connection.On<PlayerJoinedResponse>("PlayerJoined", (response) =>
            {
                if (response.IsValid)
                {
                    _logger.LogInformation("Player joined | Player Guid: {PlayerGuid} | Player Name: {PlayerName}", response.PlayerGuid, response.PlayerName);
                    _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers!.Select(x => x.PlayerName)));
                    Players = response.AllPlayers;
                }
                else
                {
                    _logger.LogError("Received invalid player joined response");
                }
            });

            _connection.On<SubmitConfigResponse>("SubmitConfig", (response) =>
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
            });

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
                _logger.LogInformation("Unable to connect to {Url} - Connection state: {State}", url, _connection.State);
                Error?.Invoke($"Unable to connect to {url}");
            }
        }

        public event MultiworldConnectedEventHandler? Connected;
        public event MultiworldGameCreatedEventHandler? GameCreated;
        public event MultiworldGameJoinedEventHandler? GameJoined;
        public event MultiworldErrorEventHandler? Error;

        public string? CurrentGameGuid { get; private set; }
        public string? CurrentPlayerGuid { get; private set; }
        public string? CurrentPlayerKey { get; private set; }
        public List<MultiworldPlayerState>? Players { get; set; }
        public MultiworldPlayerState? LocalPlayer { get; set; }

        public async Task Disconnect()
        {
            CurrentGameGuid = null;
            CurrentPlayerGuid = null;
            CurrentPlayerKey = null;
            Players = null;

            if (IsConnected()) return;
            await _connection!.DisposeAsync();
        }

        public async Task CreateGame(string playerName)
        {
            if (!IsConnected()) return;

            await _connection!.InvokeAsync("CreateGame", new CreateGameRequest(playerName));
        }

        public async Task JoinGame(string gameGuid, string playerName)
        {
            if (!IsConnected()) return;

            CurrentGameGuid = gameGuid;
            await _connection!.InvokeAsync("JoinGame", new JoinGameRequest(gameGuid, playerName));
        }

        public async Task SubmitConfig(Config config)
        {
            if (!IsConnected() || !HasJoinedGame()) return;

            await _connection!.InvokeAsync("SubmitConfig", new SubmitConfigRequest(CurrentGameGuid!, CurrentPlayerGuid!, CurrentPlayerKey!, config));
        }

        public async Task RejoinGame(string gameGuid, string playerGuid)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("Not connected");
                return;
            }

            await _connection.InvokeAsync("JoinGame", new JoinGameRequest(gameGuid, playerGuid));
        }

        public async Task StartGame(List<string> playerGuids, TrackerState trackerState)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("Not connected");
                return;
            }

            await _connection.InvokeAsync("StartGame", new StartGameRequest(playerGuids, trackerState));
        }

        public async Task Test()
        {
            var connection = new HubConnectionBuilder()
                //.WithUrl("http://www.celestialrealm.net:12000/chat")
                //.WithUrl("http://127.0.0.1:5291/multiworld")
                .WithUrl("https://localhost:7050/multiworld")
                .Build();

            connection.On<string, string>("CreateGame", (user, message) =>
            {
                Console.WriteLine("Test");
            });

            await connection.StartAsync();

            Console.WriteLine("Started");



            await connection.InvokeAsync("CreateGame");

            Console.ReadLine();

            //connection.
        }


        public bool IsConnected()
        {
            if (_connection is not { State: HubConnectionState.Connected })
            {
                _logger.LogWarning("Not connected");
                return false;
            }

            return true;
        }

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
    }
}
