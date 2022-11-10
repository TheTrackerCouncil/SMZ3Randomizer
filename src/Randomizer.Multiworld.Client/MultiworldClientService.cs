using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Randomizer.Data.Multiworld;
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

            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            _connection.On<CreateGameResponse>("CreateGame", (response) =>
            {
                if (response.IsSuccessful)
                {
                    _logger.LogInformation("Game Created | Game Id: {GameGuid} | Player Guid: {PlayergGuid} | Player Key: {PlayerKey}",
                        response.GameGuid, response.PlayerGuid, response.PlayerKey);
                    OnGameCreated?.Invoke(response.GameGuid);
                    CurrentGameGuid = response.GameGuid;
                    CurrentPlayerGuid = response.PlayerGuid;
                    CurrentPlayerKey = response.PlayerKey;
                }
                else
                {
                    _logger.LogError("Unable to create game: {Error}", response.Error);
                }
            });

            _connection.On<JoinGameResponse>("JoinGame", (response) =>
            {
                if (response.IsSuccessful)
                {
                    _logger.LogInformation("Game joined | Player Guid: {PlayerGuid} | Player Key: {PlayerKey}", response.PlayerGuid, response.PlayerKey);
                }
                else
                {
                    _logger.LogError("Unable to join game: {Error}", response.Error);
                }
            });

            _connection.On<PlayerJoinedResponse>("PlayerJoined", (response) =>
            {
                _logger.LogInformation("Player joined | Player Guid: {PlayerGuid} | Player Name: {PlayerName}", response.PlayerGuid, response.PlayerName);
                _logger.LogInformation("All players: {AllPlayers}", string.Join(", ", response.AllPlayers.Values));
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Unable to connect to {Url}", url);

            }

            if (_connection.State == HubConnectionState.Connected)
            {
                _logger.LogInformation("Connected to {Url}", url);
                OnConnected?.Invoke();
            }
        }

        public event MultiworldConnectedEventHandler? OnConnected;

        public event MultiworldGameCreatedEventHandler? OnGameCreated;

        public string? CurrentGameGuid { get; private set; }
        public string? CurrentPlayerGuid { get; private set; }
        public string? CurrentPlayerKey { get; private set; }

        public async Task CreateGame(string playerName)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("Not connected");
                return;
            }

            await _connection.InvokeAsync("CreateGame", new CreateGameRequest(playerName));
        }

        public async Task JoinGame(string gameGuid, string playerName)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                Console.WriteLine("Not connected");
                return;
            }

            await _connection.InvokeAsync("JoinGame", new JoinGameRequest(gameGuid, playerName));
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
    }
}
