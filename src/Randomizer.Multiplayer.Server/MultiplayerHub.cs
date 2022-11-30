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
            LogInfo(player, "Player disconnected");
            await SendPlayerSyncResponse(player, false);
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

            var game = MultiplayerGame.CreateNewGame(request.PlayerName, Context.ConnectionId, request.GameType, _serverUrl, request.Version, out var error);

            if (game?.AdminPlayer == null)
            {
                error ??= "Unknown error creating game";
                await SendErrorResponse(error);
                _logger.LogError("Error when creating game: {Error}", error);
                return;
            }

            var player = game.AdminPlayer;

            LogInfo(player, "Game created");

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

            LogInfo(player, "Player joined game");

            await Groups.AddToGroupAsync(Context.ConnectionId, request.GameGuid);

            await Clients.Caller.SendAsync("JoinGame",
                new JoinGameResponse(game.State, player.Guid, player.Key, game.PlayerStates));

            await SendPlayerSyncResponse(player, false);
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

            LogInfo(player, "Player rejoined game");

            game.RejoinGame(player, Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, request.GameGuid);

            await Clients.Caller.SendAsync("RejoinGame",
                new RejoinGameResponse(game.State, player.Guid, player.Key, game.PlayerStates));

            await SendPlayerSyncResponse(player, false);
        }

        /// <summary>
        /// Updates a player's state object
        /// </summary>
        /// <param name="request"></param>
        public async Task UpdatePlayerState(UpdatePlayerStateRequest request)
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            var game = player.Game;

            var playerToUpdate = request.State.Guid != player.Guid
                ? player.Game.GetPlayer(request.State.Guid, null, false)
                : player;

            if (playerToUpdate == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            LogInfo(player, $"Updating player state for {playerToUpdate.Guid}");

            game.UpdatePlayerState(playerToUpdate, request.State);

            await SendPlayerSyncResponse(player);
        }

        /// <summary>
        /// Starts the game by providing seed details for players to generate the roms
        /// </summary>
        /// <param name="request"></param>
        public async Task StartGame(StartGameRequest request)
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            if (!player.IsGameAdmin)
            {
                await SendErrorResponse("Player does not have access to start the game");
                return;
            }

            var game = player.Game;
            var successful = game.StartGame(request.ValidationHash, out var error);

            if (!successful)
            {
                await SendErrorResponse(error);
                return;
            }

            LogInfo(player, "Starting game");

            await Clients.Group(player.Game.Guid).SendAsync("StartGame",
                new StartGameResponse(player.Game.State, player.Game.PlayerStates,
                    player.Game.PlayerGenerationData));
        }

        /// <summary>
        /// Forfeits a player from the game by either removing them completely if a game hasn't started or by marking
        /// them as forfeited if the game has already started so that the other players can take their items
        /// </summary>
        /// <param name="request"></param>
        public async Task ForfeitGame(ForfeitGameRequest request)
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            if (player.Guid != request.PlayerGuid && !player.IsGameAdmin)
            {
                await SendErrorResponse("Player does not have access to forfeit another player");
                return;
            }

            var playerToUpdate = request.PlayerGuid != player.Guid
                ? player.Game.GetPlayer(request.PlayerGuid, null, false)
                : player;

            if (playerToUpdate == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            LogInfo(player, $"Forfeiting player {playerToUpdate.Guid}");

            player.Game.ForfeitPlayer(playerToUpdate);

            await Clients.Client(playerToUpdate.ConnectionId).SendAsync("ForfeitGame", new ForfeitGameResponse(player.Game.State));

            await SendPlayerSyncResponse(playerToUpdate, false);
        }

        /// <summary>
        /// Updates a game's status
        /// </summary>
        /// <param name="request"></param>
        public async Task UpdateGameState(UpdateGameStateRequest request)
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            if (!player.IsGameAdmin)
            {
                await SendErrorResponse("Player does not have access to update the multiplayer game state");
                return;
            }

            LogInfo(player, $"Update game status");

            if (request.GameStatus != null) player.Game.UpdateGameStatus(request.GameStatus.Value);

            await Clients.Group(player.Game.Guid).SendAsync("UpdateGameState", new UpdateGameStateResponse(player.Game.State));
        }

        /// <summary>
        /// Request to mark a specific location as tracked
        /// </summary>
        /// <param name="request"></param>
        public async Task TrackLocation(TrackLocationRequest request)
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            var playerToUpdate = request.PlayerGuid != player.Guid
                ? player.Game.GetPlayer(request.PlayerGuid, null, false)
                : player;

            if (playerToUpdate == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            LogInfo(player, $"Tracked location {request.LocationId} for {playerToUpdate.Guid}");

            playerToUpdate.State.TrackLocation(request.LocationId);

            await SendPlayerTrackedResponse(player, "TrackLocation",
                new TrackLocationResponse(player.Guid, request.LocationId));
        }

        /// <summary>
        /// Request to mark a specific item as tracked
        /// </summary>
        /// <param name="request"></param>
        public async Task TrackItem(TrackItemRequest request)
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            var playerToUpdate = request.PlayerGuid != player.Guid
                ? player.Game.GetPlayer(request.PlayerGuid, null, false)
                : player;

            if (playerToUpdate == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            LogInfo(player, $"Tracked item {request.ItemType} as value {request.TrackedValue} for {playerToUpdate.Guid}");

            playerToUpdate.State.TrackItem(request.ItemType, request.TrackedValue);

            await SendPlayerTrackedResponse(player, "TrackItem",
                new TrackItemResponse(player.Guid, request.ItemType, request.TrackedValue));
        }

        /// <summary>
        /// Request to mark a specific boss as tracked
        /// </summary>
        /// <param name="request"></param>
        public async Task TrackBoss(TrackBossRequest request)
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            var playerToUpdate = request.PlayerGuid != player.Guid
                ? player.Game.GetPlayer(request.PlayerGuid, null, false)
                : player;

            if (playerToUpdate == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            LogInfo(player, $"Tracked boss {request.BossType} for {playerToUpdate.Guid}");

            playerToUpdate.State.TrackBoss(request.BossType);

            await SendPlayerTrackedResponse(player, "TrackBoss",
                new TrackBossResponse(player.Guid, request.BossType));
        }

        /// <summary>
        /// Request to mark a specific dungeon as tracked
        /// </summary>
        /// <param name="request"></param>
        public async Task TrackDungeon(TrackDungeonRequest request)
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            var playerToUpdate = request.PlayerGuid != player.Guid
                ? player.Game.GetPlayer(request.PlayerGuid, null, false)
                : player;

            if (playerToUpdate == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            LogInfo(player, $"Tracked dungeon {request.DungeonName} for {playerToUpdate.Guid}");

            playerToUpdate.State.TrackDungeon(request.DungeonName);

            await SendPlayerTrackedResponse(player, "TrackDungeon",
                new TrackDungeonResponse(player.Guid, request.DungeonName));
        }

        public async Task SubmitPlayerGenerationData(SubmitPlayerGenerationDataRequest request)
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player is not { IsGameAdmin: true })
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            var playerToUpdate = request.PlayerGuid != player.Guid
                ? player.Game.GetPlayer(request.PlayerGuid, null, false)
                : player;

            if (playerToUpdate == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            LogInfo(player, $"Received generation data for {playerToUpdate.Guid}");
            playerToUpdate.PlayerGenerationData = request.PlayerGenerationData;
        }

        public async Task RequestPlayerGenerationData()
        {
            var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
            if (player == null)
            {
                await SendErrorResponse("Unable to find player");
                return;
            }

            if (player.Game.State.Status != MultiplayerGameStatus.Started)
            {
                await SendErrorResponse("Game state invalid");
                return;
            }

            LogInfo(player, $"Player requested generation data");
            await Clients.Caller.SendAsync("StartGame",
                new StartGameResponse(player.Game.State, player.Game.PlayerStates,
                    player.Game.PlayerGenerationData));
        }

        private async Task SendPlayerTrackedResponse(MultiplayerPlayer player, string method, object message, bool allPlayers = true)
        {
            var game = player.Game;
            var sendTo = allPlayers ? Clients.Group(game.State.Guid) : Clients.OthersInGroup(game.State.Guid);
            await sendTo.SendAsync(method, message);
        }

        /// <summary>
        /// Sends players a particular player's state
        /// </summary>
        /// <param name="player">The player state that is being sent out</param>
        /// <param name="allPlayers">If all players should receive the notification or if only players excluding the player whose state is being sent out</param>
        private async Task SendPlayerSyncResponse(MultiplayerPlayer player, bool allPlayers = true)
        {
            var game = player.Game;
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

        private void LogInfo(MultiplayerGame game, string message) => _logger.LogInformation("Game {GameGuid} | {Message}", game.Guid, message);

        private void LogInfo(MultiplayerPlayer player, string message) => _logger.LogInformation("Game {GameGuid} | Player {PlayerGuid} | {Message}", player.Game.Guid, player.Guid, message);

    }
}
