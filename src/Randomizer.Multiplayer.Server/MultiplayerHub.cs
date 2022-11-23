using Microsoft.AspNetCore.SignalR;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Server
{
    /// <summary>
    /// SignalR hub for SMZ3 multiplayer
    /// </summary>
    public class MultiplayerHub : Hub
    {
        private readonly ILogger<MultiplayerHub> _logger;
        private readonly string _serverUrl;

        public MultiplayerHub(ILogger<MultiplayerHub> logger, IConfiguration configuration)
        {
            _logger = logger;
            _serverUrl = configuration.GetValue<string>("SMZ3:ServerUrl");
        }

        /// <summary>
        /// Removes disconnected players from a game
        /// </summary>
        /// <param name="exception"></param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var player = MultiplayerGame.PlayerDisconnected(Context.ConnectionId);
            if (player == null) return;
            _logger.LogInformation("Game: {GameGuid} | Player: {PlayerGuid} | Player disconnected", player.Game.State.Guid, player.Guid);
            await Clients.Group(player.Game.State.Guid)
                .SendAsync("PlayerSync", new PlayerSyncResponse(player.Game.State, player.State));
        }

        /// <summary>
        /// Creates a new multiplayer game
        /// </summary>
        /// <param name="request"></param>
        public async Task CreateGame(CreateGameRequest request)
        {
            if (string.IsNullOrEmpty(request.PlayerName))
            {
                await SendErrorResponse("Missing required attributes");
                return;
            }

            var game = MultiplayerGame.CreateNewGame(request.PlayerName, Context.ConnectionId, request.GameType, _serverUrl, "", out var error);

            if (game == null || game.AdminPlayer == null)
            {
                error ??= "Unknown error creating game";
                await SendErrorResponse(error);
                _logger.LogError("Error when creating game: {Error}", error);
                return;
            }

            var player = game.AdminPlayer;

            _logger.LogInformation("Game: {GameGuid} | Player: {PlayerGuid} | Game created", game.State.Guid, player.Guid);

            await Groups.AddToGroupAsync(Context.ConnectionId, game.State.Guid);

            await Clients.Caller.SendAsync("CreateGame",
                new CreateGameResponse(game.State,
                    player.Guid,
                    player.Key,
                    game.PlayerStates));
        }

        /// <summary>
        /// Joins a created multiplayer game
        /// </summary>
        /// <param name="request"></param>
        public async Task JoinGame(JoinGameRequest request)
        {
            if (string.IsNullOrEmpty(request.GameGuid) || string.IsNullOrEmpty(request.PlayerName))
            {
                await SendErrorResponse("Missing required attributes");
                return;
            }
            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse($"Unable to find game");
                return;
            }

            var player = game.JoinGame(request.PlayerName, Context.ConnectionId, request.Version, out var error);
            if (player == null)
            {
                error ??= "Unknown error joining game";
                await SendErrorResponse(error);
                _logger.LogError("Error when joining game: {Error}", error);
                return;
            }

            _logger.LogInformation("Game: {GameGuid} | Player:  {PlayerGuid} | Player joined", game.State.Guid, player.Guid);

            await Groups.AddToGroupAsync(Context.ConnectionId, request.GameGuid);

            await Clients.Caller.SendAsync("JoinGame",
                new JoinGameResponse(game.State, player.Guid, player.Key, game.PlayerStates));

            await SendPlayerSyncResponse(game, player, false);
        }

        /// <summary>
        /// Rejoins a player to a game that they have already joined to
        /// </summary>
        /// <param name="request"></param>
        public async Task RejoinGame(RejoinGameRequest request)
        {
            if (string.IsNullOrEmpty(request.GameGuid) || string.IsNullOrEmpty(request.PlayerGuid) || string.IsNullOrEmpty(request.PlayerKey))
            {
                await SendErrorResponse("Missing required attributes");
                return;
            }

            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse($"Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            game.RejoinGame(player, Context.ConnectionId);

            _logger.LogInformation("Game: {GameGuid} | Player:  {PlayerGuid} | Player rejoined", game.State.Guid, player.Guid);

            await Groups.AddToGroupAsync(Context.ConnectionId, request.GameGuid);

            await Clients.Caller.SendAsync("RejoinGame",
                new RejoinGameResponse(game.State, player.Guid, player.Key, game.PlayerStates));

            await SendPlayerSyncResponse(game, player, false);
        }

        /// <summary>
        /// Updates a player's state object
        /// </summary>
        /// <param name="request"></param>
        public async Task UpdatePlayerState(UpdatePlayerStateRequest request)
        {
            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse("Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            if (request.State.Guid != request.PlayerGuid)
            {
                player = game.GetPlayer(request.State.Guid, null, false);
                if (player == null)
                {
                    await SendErrorResponse("Unable to find player");
                    return;
                }
            }

            game.UpdatePlayerState(player, request.State);

            _logger.LogInformation("Game: {GameGuid} | Player: {PlayerGuid} | Player state updated", game.State.Guid, player.Guid);

            await Clients.Groups(request.GameGuid)
                .SendAsync("PlayerSync", new PlayerSyncResponse(game.State, player.State));
        }

        /// <summary>
        /// Starts the game by providing seed details for players to generate the roms
        /// </summary>
        /// <param name="request"></param>
        public async Task StartGame(StartGameRequest request)
        {
            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse("Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true, true);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            var successful = game.StartGame(request.Seed, request.ValidationHash, out var error);

            if (!successful)
            {
                await SendErrorResponse(error);
                return;
            }

            _logger.LogInformation("Starting game");

            await Clients.Group(request.GameGuid)
                .SendAsync("PlayerListSync", new PlayerListSyncResponse(game.State, game.PlayerStates));
        }

        /// <summary>
        /// Forfeits a player from the game by either removing them completely if a game hasn't started or by marking
        /// them as forfeited if the game has already started so that the other players can take their items
        /// </summary>
        /// <param name="request"></param>
        public async Task ForfeitGame(ForfeitGameRequest request)
        {
            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse("Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            if (!player.State.IsAdmin && player.Guid != request.ForfeitPlayerGuid)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            if (player.Guid != request.ForfeitPlayerGuid)
            {
                player = game.GetPlayer(request.ForfeitPlayerGuid, null, false);
                if (player == null)
                {
                    await SendErrorResponse("Unable to find player");
                    return;
                }
            }

            game.ForfeitPlayer(player);

            await Clients.Client(player.ConnectionId).SendAsync("ForfeitGame", new ForfeitGameResponse(game.State));

            await SendPlayerSyncResponse(game, player, false);
        }

        /// <summary>
        /// Updates a game's status
        /// </summary>
        /// <param name="request"></param>
        public async Task UpdateGameStatus(UpdateGameStatusRequest request)
        {
            var game = MultiplayerGame.LoadGame(request.GameGuid);
            if (game == null)
            {
                await SendErrorResponse("Unable to find game");
                return;
            }

            var player = game.GetPlayer(request.PlayerGuid, request.PlayerKey, true, true);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            game.UpdateGameStatus(request.GameStatus);

            await Clients.Group(game.State.Guid).SendAsync("UpdateGameStatus", new UpdateGameStatusResponse(game.State));
        }

        /// <summary>
        /// Sends players a particular player's state
        /// </summary>
        /// <param name="game">The game that the message is being sent to</param>
        /// <param name="player">The player state that is being sent out</param>
        /// <param name="allPlayers">If all players should receive the notification or if only players excluding the player whose state is being sent out</param>
        private async Task SendPlayerSyncResponse(MultiplayerGame game, MultiplayerPlayer player, bool allPlayers = true)
        {
            var sendTo = allPlayers ? Clients.Group(game.State.Guid) : Clients.GroupExcept(game.State.Guid, player.ConnectionId);
            await sendTo.SendAsync("PlayerSync", new PlayerSyncResponse(game.State, player.State));
        }

        /// <summary>
        /// Sends the active connected player an error message
        /// </summary>
        /// <param name="error">The error to display</param>
        private async Task SendErrorResponse(string? error)
        {
            await Clients.Caller.SendAsync("Error", new ErrorResponse(error ?? "Unknown Error"));
        }

    }
}
