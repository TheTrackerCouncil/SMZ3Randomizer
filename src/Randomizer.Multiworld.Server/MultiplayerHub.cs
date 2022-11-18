using Microsoft.AspNetCore.SignalR;
using Randomizer.Data.Multiplayer;

namespace Randomizer.Multiplayer.Server
{
    public class MultiplayerHub : Hub
    {
        private readonly ILogger<MultiplayerHub> _logger;
        private readonly string _serverUrl;

        public MultiplayerHub(ILogger<MultiplayerHub> logger, IConfiguration configuration)
        {
            _logger = logger;
            _serverUrl = configuration.GetValue<string>("SMZ3:ServerUrl");
        }

        public async Task CreateGame(CreateGameRequest request)
        {
            if (string.IsNullOrEmpty(request.PlayerName))
            {
                await SendErrorResponse<CreateGameResponse>("CreateGame", "Missing required attributes");
                return;
            }

            var results = MultiplayerGame.CreateNewGame(request.PlayerName, Context.ConnectionId, out var error);

            if (results == null)
            {
                error ??= "Unknown error creating game";
                await SendErrorResponse<CreateGameResponse>("CreateGame", error);
                _logger.LogError("Error when creating game: {Error}", error);
                return;
            }

            _logger.LogInformation("Game: {GameGuid} | Player: {PlayerGuid} | Game created", results.Value.Game.Guid, results.Value.Player.Guid);

            await Groups.AddToGroupAsync(Context.ConnectionId, results.Value.Game.Guid);

            await Clients.Caller.SendAsync("CreateGame", new CreateGameResponse()
            {
                IsSuccessful = true,
                GameGuid = results.Value.Game.Guid,
                PlayerGuid = results.Value.Player.Guid,
                PlayerKey = results.Value.Player.Key,
                AllPlayers = results.Value.Game.PlayerStates,
                GameUrl = $"{_serverUrl}?game={results.Value.Game.Guid}",
            });
        }

        public async Task JoinGame(JoinGameRequest request)
        {
            if (string.IsNullOrEmpty(request.GameGuid) || string.IsNullOrEmpty(request.PlayerName))
            {
                await SendErrorResponse<JoinGameResponse>("JoinGame", "Missing required attributes");
                return;
            }
            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse<JoinGameResponse>("JoinGame", $"Unable to find game");
                return;
            }

            var player = game.JoinGame(request.PlayerName, Context.ConnectionId, out var error);
            if (player == null)
            {
                error ??= "Unknown error joining game";
                await SendErrorResponse<JoinGameResponse>("JoinGame", error);
                _logger.LogError("Error when joining game: {Error}", error);
                return;
            }

            _logger.LogInformation("Game: {GameGuid} | Player:  {PlayerGuid} | Player joined", game.Guid, player.Guid);

            await Groups.AddToGroupAsync(Context.ConnectionId, request.GameGuid);

            await Clients.Caller.SendAsync("JoinGame", new JoinGameResponse()
            {
                IsSuccessful = true,
                PlayerGuid = player.Guid,
                PlayerKey = player.Key,
                AllPlayers = game.PlayerStates,
                GameUrl = $"{_serverUrl}?game={game.Guid}",
            });

            await Clients.OthersInGroup(request.GameGuid).SendAsync("PlayerJoined", new PlayerJoinedResponse()
            {
                IsSuccessful = true,
                PlayerGuid = player.Guid,
                PlayerName = player.State.PlayerName,
                AllPlayers = game.PlayerStates
            });
        }

        public async Task RejoinGame(RejoinGameRequest request)
        {
            if (string.IsNullOrEmpty(request.GameGuid) || string.IsNullOrEmpty(request.PlayerGuid) || string.IsNullOrEmpty(request.PlayerKey))
            {
                await SendErrorResponse<JoinGameResponse>("JoinGame", "Missing required attributes");
                return;
            }

            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse<JoinGameResponse>("JoinGame", $"Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true);
            if (player == null)
            {
                await SendErrorResponse<SubmitConfigResponse>("SubmitConfig", "Unable to find player");
                return;
            }

            game.RejoinGame(player, Context.ConnectionId);

            _logger.LogInformation("Game: {GameGuid} | Player:  {PlayerGuid} | Player rejoined", game.Guid, player.Guid);

            await Groups.AddToGroupAsync(Context.ConnectionId, request.GameGuid);

            await Clients.Caller.SendAsync("RejoinGame", new RejoinGameResponse()
            {
                IsSuccessful = true,
                PlayerGuid = player.Guid,
                PlayerKey = player.Key,
                AllPlayers = game.PlayerStates,
                GameUrl = $"{_serverUrl}?game={game.Guid}",
            });

            await Clients.OthersInGroup(request.GameGuid).SendAsync("PlayerRejoined", new PlayerJoinedResponse()
            {
                IsSuccessful = true,
                PlayerGuid = player.Guid,
                PlayerName = player.State.PlayerName,
                AllPlayers = game.PlayerStates
            });
        }

        public async Task SubmitConfig(SubmitConfigRequest request)
        {
            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse<SubmitConfigResponse>("SubmitConfig", "Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true);
            if (player == null)
            {
                await SendErrorResponse<SubmitConfigResponse>("SubmitConfig", "Unable to find player");
                return;
            }

            _logger.LogInformation("Game: {GameGuid} | Player: {PlayerGuid} | Config submitted", game.Guid, player.Guid);

            var configs = game.UpdatePlayerConfig(player, request.Config);

            await Clients.Caller.SendAsync("SubmitConfig", new SubmitConfigResponse() { IsSuccessful = true });

            await Clients.Groups(request.GameGuid).SendAsync("PlayerSync", new PlayerSyncResponse()
            {
                IsSuccessful = true,
                PlayerState = player.State
            });
        }

        public async Task StartGame(StartGameRequest request)
        {
            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse<StartGameResponse>("StartGame", "Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true, true);
            if (player == null)
            {
                await SendErrorResponse<StartGameResponse>("StartGame", "Unable to find player");
                return;
            }

            var playerStates = game.StartGame(request.PlayerGuids, request.TrackerState, out var error);
            await Clients.Group(request.GameGuid).SendAsync("StartGame", new StartGameResponse()
            {
                IsSuccessful = true,
                Players = playerStates
            });
        }

        public async Task ForfeitGame(ForfeitGameRequest request)
        {
            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse<ForfeitGameResponse>("ForfeitPlayer", "Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true);
            if (player == null)
            {
                await SendErrorResponse<ForfeitGameResponse>("ForfeitPlayer", "Unable to find player");
                return;
            }

            if (!player.State.IsAdmin && player.Guid != request.ForfeitPlayerGuid)
            {
                await SendErrorResponse<ForfeitGameResponse>("ForfeitPlayer", "Unable to find player");
                return;
            }

            if (player.Guid != request.ForfeitPlayerGuid)
            {
                player = game.GetPlayer(request.ForfeitPlayerGuid, null, false);
                if (player == null)
                {
                    await SendErrorResponse<ForfeitGameResponse>("ForfeitPlayer", "Unable to find player");
                    return;
                }
            }

            game.ForfeitPlayer(player);

            await Clients.Client(player.ConnectionId).SendAsync("ForfeitGame", new ForfeitGameResponse()
            {
                IsSuccessful = true,
                ForfeitPlayerGuid = request.ForfeitPlayerGuid,
                AllPlayers = game.PlayerStates
            });

            await Clients.GroupExcept(request.GameGuid, player.ConnectionId).SendAsync("PlayerForfeited", new PlayerForfeitedResponse()
            {
                IsSuccessful = true,
                PlayerGuid = player.Guid,
                PlayerName = player.State.PlayerName,
                AllPlayers = game.PlayerStates
            });
        }

        private async Task SendErrorResponse<T>(string method, string error) where T : MultiplayerResponse, new()
        {
            var response = new T() { IsSuccessful = false, Error = error };
            await Clients.Caller.SendAsync(method, response);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var player = MultiplayerGame.PlayerDisconnected(Context.ConnectionId);
            if (player == null) return;
            _logger.LogInformation("Game: {GameGuid} | Player: {PlayerGuid} | Player disconnected", player.Game.Guid, player.Guid);
            await Clients.Group(player.Game.Guid).SendAsync("PlayerSync", new PlayerSyncResponse()
            {
                IsSuccessful = true,
                PlayerState = player.State
            });
        }
    }
}
