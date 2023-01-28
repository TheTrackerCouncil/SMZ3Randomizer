using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Server;

/// <summary>
/// SignalR hub for SMZ3 multiplayer
/// </summary>
public class MultiplayerHub : Hub
{
    private readonly ILogger<MultiplayerHub> _logger;
    private readonly string _serverUrl;
    private readonly MultiplayerDbService _dbService;

    public MultiplayerHub(ILogger<MultiplayerHub> logger, IOptions<SMZ3ServerSettings> options, MultiplayerDbService dbService)
    {
        _logger = logger;
        _serverUrl = options.Value.ServerUrl;
        _dbService = dbService;
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

        if (MultiplayerVersion.Id != request.MultiplayerVersion)
        {
            await SendErrorResponse("Multiplayer version mismatch. Please update your SMZ3 version.");
            return;
        }

        if (request.SaveToDatabase && !MultiplayerDbContext.IsSetup)
        {
            await SendErrorResponse("This server does not support async/multi-session games.");
            return;
        }

        var game = MultiplayerGame.CreateNewGame(request.PlayerName, request.PhoneticName, Context.ConnectionId, request.GameType, _serverUrl, request.RandomizerVersion, request.SaveToDatabase, request.SendItemsOnComplete, out var error);

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

        await _dbService.AddGameToDatabase(game.State);
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

        if (MultiplayerVersion.Id != request.MultiplayerVersion)
        {
            await SendErrorResponse("Multiplayer version mismatch. Please update your SMZ3 version.");
            return;
        }

        var game = MultiplayerGame.LoadGame(request.GameGuid) ?? _dbService.LoadGameFromDatabase(request.GameGuid);
        if (game == null)
        {
            await SendErrorResponse($"Unable to find game");
            return;
        }

        var player = game.JoinGame(request.PlayerName, request.PhoneticName, Context.ConnectionId, request.RandomizerVersion, out var error);
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

        await _dbService.AddPlayerToGame(game.State, player.State);
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

        if (MultiplayerVersion.Id != request.MultiplayerVersion)
        {
            await SendErrorResponse("Multiplayer version mismatch. Please update your SMZ3 version.");
            return;
        }

        var game = MultiplayerGame.LoadGame(request.GameGuid) ?? _dbService.LoadGameFromDatabase(request.GameGuid);
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

        await SendPlayerSyncResponse(playerToUpdate);

        await _dbService.SavePlayerState(game.State, playerToUpdate.State);

    }

    /// <summary>
    /// Updates a player's config and sends it to the players
    /// </summary>
    /// <param name="request"></param>
    public async Task UpdatePlayerConfig(UpdatePlayerConfigRequest request)
    {
        var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
        if (player == null)
        {
            await SendErrorResponse("Unable to find player");
            return;
        }

        var game = player.Game;

        var playerToUpdate = request.PlayerGuid != player.Guid
            ? player.Game.GetPlayer(request.PlayerGuid, null, false)
            : player;

        if (playerToUpdate == null)
        {
            await SendErrorResponse("Unable to find player");
            return;
        }

        LogInfo(player, $"Updating player config for {playerToUpdate.Guid}");

        game.UpdatePlayerConfig(playerToUpdate, request.Config);

        await SendPlayerSyncResponse(playerToUpdate);

        await _dbService.SavePlayerState(game.State, playerToUpdate.State);
    }

    /// <summary>
    /// Updates a player's world status
    /// </summary>
    /// <param name="request"></param>
    public async Task UpdatePlayerWorld(UpdatePlayerWorldRequest request)
    {
        var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
        if (player == null)
        {
            await SendErrorResponse("Unable to find player");
            return;
        }

        var game = player.Game;

        var playerToUpdate = request.PlayerGuid != player.Guid
            ? player.Game.GetPlayer(request.PlayerGuid, null, false)
            : player;

        if (playerToUpdate == null)
        {
            await SendErrorResponse("Unable to find player");
            return;
        }

        LogInfo(player, $"Updating player world for {playerToUpdate.Guid}");

        var worldUpdates = game.UpdatePlayerWorld(playerToUpdate, request.WorldState);

        if (request.Propogate) await SendPlayerSyncResponse(playerToUpdate);

        await _dbService.SavePlayerWorld(player.Game.State, playerToUpdate.State, worldUpdates);
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

        await _dbService.SaveGameState(game.State);
        await _dbService.SavePlayerStates(game.State, game.PlayerStates);
    }

    /// <summary>
    /// Forfeits a player from the game by either removing them completely if a game hasn't started or by marking
    /// them as forfeited if the game has already started so that the other players can take their items
    /// </summary>
    /// <param name="request"></param>
    public async Task ForfeitPlayerGame(ForfeitPlayerGameRequest request)
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

        if (playerToUpdate.State.HasCompleted || playerToUpdate.State.HasForfeited)
        {
            await SendErrorResponse("Player already ended game");
            return;
        }

        LogInfo(player, $"Forfeiting game for player {playerToUpdate.Guid}");

        player.Game.ForfeitPlayerGame(playerToUpdate, out var deleteFromDatabase, out var gameStatusUpdated);

        await Clients.Group(player.Game.Guid).SendAsync("ForfeitPlayerGame",
            new ForfeitPlayerGameResponse(player.Game.State, playerToUpdate.State));

        if (deleteFromDatabase)
            await _dbService.DeletePlayerState(player.Game.State, playerToUpdate.State);
        else
            await _dbService.SavePlayerState(player.Game.State, playerToUpdate.State);
        if (gameStatusUpdated)
            await _dbService.SaveGameState(player.Game.State);

    }

    /// <summary>
    /// Marks a player as having completed the game
    /// </summary>
    /// <param name="request"></param>
    public async Task CompletePlayerGame(CompletePlayerGameRequest request)
    {
        var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
        if (player == null)
        {
            await SendErrorResponse("Unable to find player");
            return;
        }

        if (player.State.HasCompleted || player.State.HasForfeited)
        {
            await SendErrorResponse("Player already ended game");
            return;
        }

        LogInfo(player, $"Completing game for player {player.Guid}");

        player.Game.CompletePlayerGame(player, out var gameStatusUpdated);

        await Clients.Group(player.Game.Guid).SendAsync("CompletePlayerGame",
            new CompletePlayerGameResponse(player.Game.State, player.State));

        await _dbService.SavePlayerState(player.Game.State, player.State);
        if (gameStatusUpdated) await _dbService.SaveGameState(player.Game.State);
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

        await _dbService.SaveGameState(player.Game.State);
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

        var location = player.Game.TrackLocation(playerToUpdate, request.LocationId);

        await SendPlayerTrackedResponse(playerToUpdate, "TrackLocation",
            new TrackLocationResponse(playerToUpdate.Guid, request.LocationId));

        if (location != null) await _dbService.SaveLocationState(playerToUpdate.Game.State, location);
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

        var item = player.Game.TrackItem(playerToUpdate, request.ItemType, request.TrackedValue);

        await SendPlayerTrackedResponse(playerToUpdate, "TrackItem",
            new TrackItemResponse(playerToUpdate.Guid, request.ItemType, request.TrackedValue));

        if (item != null) await _dbService.SaveItemState(playerToUpdate.Game.State, item);
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

        var boss = player.Game.TrackBoss(playerToUpdate, request.BossType);

        await SendPlayerTrackedResponse(playerToUpdate, "TrackBoss",
            new TrackBossResponse(playerToUpdate.Guid, request.BossType));

        if (boss != null) await _dbService.SaveBossState(playerToUpdate.Game.State, boss);
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

        var dungeon = player.Game.TrackDungeon(playerToUpdate, request.DungeonName);

        await SendPlayerTrackedResponse(playerToUpdate, "TrackDungeon",
            new TrackDungeonResponse(playerToUpdate.Guid, request.DungeonName));

        if (dungeon != null) await _dbService.SaveDungeonState(playerToUpdate.Game.State, dungeon);
    }

    /// <summary>
    /// Pushes the world data for a player needed to generate the rom
    /// </summary>
    /// <param name="request"></param>
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
        player.Game.SetPlayerGenerationData(playerToUpdate, request.WorldId, request.PlayerGenerationData);
        await _dbService.SavePlayerState(playerToUpdate.Game.State, playerToUpdate.State);
    }

    /// <summary>
    /// Requests all of the player generation data for generating the rom. Called when the player was not
    /// connected when the game was started
    /// </summary>
    /// <param name="request"></param>
    public async Task RequestPlayerGenerationData(MultiplayerRequest request)
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

    /// <summary>
    /// Requests all of the player data from the server, either just for that player or to send out to all players
    /// </summary>
    /// <param name="request"></param>
    public async Task PlayerListSync(PlayerListSyncRequest request)
    {
        var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
        if (player == null)
        {
            await SendErrorResponse("Unable to find player");
            return;
        }

        if (request.SendToAllPlayers)
            LogInfo(player, "Player list requested to be sent to all players");
        else
            LogInfo(player, "Player list requested");

        var sendTo = request.SendToAllPlayers ? Clients.Group(player.Game.Guid) : Clients.Caller;
        await sendTo.SendAsync("PlayerListSync",
            new PlayerListSyncResponse(player.Game.State, player.Game.PlayerStates));
    }

    /// <summary>
    /// Requests an update from the player to make sure that they are still in sync. If no player guid is sent in
    /// the request, then all players will be requested
    /// </summary>
    /// <param name="request"></param>
    public async Task RequestPlayerUpdate(RequestPlayerUpdateRequest request)
    {
        var player = MultiplayerGame.LoadPlayer(Context.ConnectionId);
        if (player == null)
        {
            await SendErrorResponse("Unable to find player");
            return;
        }

        var playerToSendTo = request.PlayerGuid == null
            ? null
            : player.Game.GetPlayer(request.PlayerGuid, null, false);

        var sendTo = playerToSendTo == null ? Clients.Group(player.Game.Guid) : Clients.Client(playerToSendTo.ConnectionId);
        await sendTo.SendAsync("RequestPlayerUpdate", new RequestPlayerUpdateResponse());
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
