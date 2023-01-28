using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Randomizer.Shared;
using Randomizer.Shared.Enums;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Server;

/// <summary>
/// Class for a single multiplayer game
/// </summary>
public class MultiplayerGame
{
    public static int GameCount => s_games.Count;
    public static int PlayerCount => s_playerConnections.Count;
    public static IEnumerable<MultiplayerGameState> GameStates => s_games.Values.Select(x => x.State);

    private static readonly Regex s_illegalPlayerNameCharacters = new(@"[^A-Z0-9\-]", RegexOptions.IgnoreCase);
    private static readonly Regex s_continousSpace = new(@" +");
    private static readonly ConcurrentDictionary<string, MultiplayerGame> s_games = new();
    private static readonly ConcurrentDictionary<string, MultiplayerPlayer> s_playerConnections = new();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid">The unique identifier for looking up the game</param>
    /// <param name="gameUrl">The url for players to use for connecting to the game</param>
    /// <param name="version">The SMZ3 version that the game is being generated on</param>
    /// <param name="type">The type of multiplayer game</param>
    /// <param name="saveToDatabase">If this database should be saved to the database</param>
    /// <param name="sendItemsOnComplete">If items in a player's world should be auto distributed on beating the game</param>
    private MultiplayerGame(string guid, string gameUrl, string version, MultiplayerGameType type, bool saveToDatabase, bool sendItemsOnComplete)
    {
        State = new MultiplayerGameState()
        {
            Guid = guid,
            Url = gameUrl,
            Version = version,
            Type = type,
            Status = MultiplayerGameStatus.Created,
            CreatedDate = DateTimeOffset.Now,
            LastMessage = DateTimeOffset.Now,
            Seed = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue).ToString(),
            SaveToDatabase = saveToDatabase,
            SendItemsOnComplete = sendItemsOnComplete
        };
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="state">Previous state</param>
    private MultiplayerGame(MultiplayerGameState state)
    {
        State = state;
    }

    public string Guid => State.Guid;
    public MultiplayerGameState State { get; }
    private readonly ConcurrentDictionary<string, MultiplayerPlayer> _players = new();
    public MultiplayerPlayer? AdminPlayer => _players.Values.FirstOrDefault(x => x.IsGameAdmin);
    public List<MultiplayerPlayerState> PlayerStates => _players.Values.Select(x => x.State).ToList();
    public List<string> PlayerGenerationData => _players.Values.Select(x => x.PlayerGenerationData).NonNull().ToList();


    #region Static Methods

    public static List<string> ExpireGamesInMemory(int expirationTime)
    {
        var expiredGuids = new List<string>();
        foreach (var game in s_games.Values)
        {
            var timeDiff = DateTimeOffset.Now - game.State.LastMessage;
            if (timeDiff.TotalMinutes > expirationTime)
            {
                foreach (var player in game._players.Values)
                {
                    if (!string.IsNullOrEmpty(player.ConnectionId))
                    {
                        s_playerConnections.TryRemove(player.ConnectionId, out _);
                    }
                }
                s_games.TryRemove(game.Guid, out _);
                expiredGuids.Add(game.Guid);
            }
        }

        return expiredGuids;
    }

    /// <summary>
    /// Creates a new multiplayer game
    /// </summary>
    /// <param name="playerName">The requested name of the admin player</param>
    /// <param name="phoneticName">The requested name of the admin player for tracker</param>
    /// <param name="playerConnectionId">The connection id of the admin player</param>
    /// <param name="gameType">The type of multiplayer game</param>
    /// <param name="baseUrl">The server's base url</param>
    /// <param name="version">The SMZ3 version that the game is being generated on</param>
    /// <param name="saveToDatabase">If this game needs to be saved to the database or not</param>
    /// <param name="sendItemsOnComplete">If items in a player's world should be auto distributed on beating the game</param>
    /// <param name="error">Output of any error messages during creating the new multiplayer game</param>
    /// <returns>The instance of the created Multiplayer game</returns>
    public static MultiplayerGame? CreateNewGame(string playerName, string phoneticName, string playerConnectionId, MultiplayerGameType gameType, string baseUrl, string version, bool saveToDatabase, bool sendItemsOnComplete, out string? error)
    {
        string guid;
        do
        {
            guid = System.Guid.NewGuid().ToString("N");
        } while (s_games.ContainsKey(guid));

        var game = new MultiplayerGame(guid, $"{baseUrl}?game={guid}", version, gameType, saveToDatabase, sendItemsOnComplete);

        if (!s_games.TryAdd(guid, game))
        {
            error = "Unable to create game";
            return null;
        }

        var playerGuid = System.Guid.NewGuid().ToString("N");
        var playerKey = System.Guid.NewGuid().ToString("N");
        var player =
            new MultiplayerPlayer(game, playerGuid, playerKey, CleanPlayerName(playerName), phoneticName,
                playerConnectionId) { State = { IsAdmin = true } };

        game.State.Players.Add(player.State);
        game._players[playerGuid] = player;
        s_playerConnections[playerConnectionId] = player;
        error = null;
        return game;
    }

    /// <summary>
    /// Attempts to load a multiplayer game based on its guid
    /// </summary>
    /// <param name="guid">The game unique guid</param>
    /// <returns>The multiplayer game object, if found</returns>
    public static MultiplayerGame? LoadGame(string guid)
    {
        return s_games.TryGetValue(guid, out var game) ? game : null;
    }

    public static MultiplayerGame? LoadGameFromState(MultiplayerGameState? gameState)
    {
        if (gameState == null) return null;
        var game = new MultiplayerGame(gameState);
        if (!s_games.TryAdd(gameState.Guid, game)) return null;

        foreach (var playerState in gameState.Players)
        {
            game._players[playerState.Guid] = new MultiplayerPlayer(game, playerState);
        }
        return game;
    }

    /// <summary>
    /// Attempts to load a player based on the connection id
    /// </summary>
    /// <param name="connectionId">The player connection id to look up</param>
    /// <returns>The player object if found, otherwise null</returns>
    public static MultiplayerPlayer? LoadPlayer(string connectionId)
    {
        return s_playerConnections.TryGetValue(connectionId, out var player) ? player : null;
    }

    /// <summary>
    /// Marks a player as currently disconnected from a game
    /// </summary>
    /// <param name="connectionId">The player's connection id</param>
    /// <returns>The player object for the disconnected player, if found</returns>
    public static MultiplayerPlayer? PlayerDisconnected(string connectionId)
    {
        if (!s_playerConnections.Remove(connectionId, out var player)) return null;
        player.ConnectionId = "";
        player.State.IsConnected = false;
        return player;
    }
    #endregion Static Methods

    /// <summary>
    /// Adds a player to a multiplayer game
    /// </summary>
    /// <param name="playerName">The desired name of the player</param>
    /// <param name="phoneticName">The desired name of the player for tracker</param>
    /// <param name="playerConnectionId">The connection id of the player</param>
    /// <param name="version">The SMZ3 version the player is running on</param>
    /// <param name="error">Output of any error while trying to add the player to the game</param>
    /// <returns>The player object for the added player, if successfully added</returns>
    public MultiplayerPlayer? JoinGame(string playerName, string phoneticName, string playerConnectionId, string version, out string? error)
    {
        if (_players.Values.Any(prevPlayer => prevPlayer.State.PlayerName == playerName))
        {
            error = "Player name in use";
            return null;
        }

        if (State.Status != MultiplayerGameStatus.Created)
        {
            error = "Game already started";
            return null;
        }

        if (State.Version != version)
        {
            error = $"Player SMZ3 version of {version} does not match game SMZ3 version of {State.Version}";
            return null;
        }

        string guid;
        do
        {
            guid = System.Guid.NewGuid().ToString("N");
        } while (_players.ContainsKey(guid));

        var key = System.Guid.NewGuid().ToString("N");

        var player = new MultiplayerPlayer(this, guid, key, CleanPlayerName(playerName), phoneticName, playerConnectionId);
        _players[guid] = player;
        s_playerConnections[playerConnectionId] = player;
        State.LastMessage = DateTimeOffset.Now;
        error = null;
        return player;
    }

    /// <summary>
    /// Marks a player as reconnected back to a game
    /// </summary>
    /// <param name="player">The player object</param>
    /// <param name="playerConnectionId">The new connection id for the player</param>
    public void RejoinGame(MultiplayerPlayer player, string playerConnectionId)
    {
        s_playerConnections[playerConnectionId] = player;
        player.ConnectionId = playerConnectionId;
        player.State.IsConnected = true;
        State.LastMessage = DateTimeOffset.Now;
    }

    /// <summary>
    /// Forfeits a player by either removing them from the game if the game hasn't started yet or by marking the
    /// player as forfeit so players can collect their items if the game has already started
    /// </summary>
    /// <param name="player">The player to mark as forfeit</param>
    /// <param name="deleteFromDatabase">Set to true when a player forfeits before the game starts</param>
    /// <param name="gameStatusUpdated">Set to true when a game is set to complete after the final player forfeiting</param>
    public void ForfeitPlayerGame(MultiplayerPlayer player, out bool deleteFromDatabase, out bool gameStatusUpdated)
    {
        player.State.Status = MultiplayerPlayerStatus.Forfeit;
        if (player.State.IsAdmin)
        {
            player.State.IsAdmin = false;
            var newAdmin = _players.Values.FirstOrDefault(x => x.State is { HasForfeited: false, HasCompleted: false });
            if (newAdmin != null)
            {
                newAdmin.State.IsAdmin = true;
            }
        }

        if (_players.Values.All(x => x.State is { HasCompleted: true, HasForfeited: true }))
        {
            State.Status = MultiplayerGameStatus.Completed;
            gameStatusUpdated = true;
        }
        else
        {
            gameStatusUpdated = false;
        }

        State.LastMessage = DateTimeOffset.Now;

        if (!State.HasGameStarted)
        {
            s_playerConnections.TryRemove(player.ConnectionId, out _);
            _players.TryRemove(player.Guid, out _);
            State.Players.Remove(player.State);
            deleteFromDatabase = true;
        }
        else
        {
            deleteFromDatabase = false;
        }
    }

    /// <summary>
    /// Forfeits a player by either removing them from the game if the game hasn't started yet or by marking the
    /// player as forfeit so players can collect their items if the game has already started
    /// </summary>
    /// <param name="player">The player to mark as forfeit</param>
    /// <param name="gameStatusUpdated">Set to true when the game is set to complete after the final player completes the game</param>
    public void CompletePlayerGame(MultiplayerPlayer player, out bool gameStatusUpdated)
    {
        player.State.Status = MultiplayerPlayerStatus.Completed;
        if (player.State.IsAdmin)
        {
            player.State.IsAdmin = false;
            var newAdmin = _players.Values.FirstOrDefault(x => x.State is { HasForfeited: false, HasCompleted: false });
            if (newAdmin != null)
            {
                newAdmin.State.IsAdmin = true;
            }
        }

        if (_players.Values.All(x => x.State is { HasCompleted: true, HasForfeited: true }))
        {
            State.Status = MultiplayerGameStatus.Completed;
            gameStatusUpdated = true;
        }
        else
        {
            gameStatusUpdated = false;
        }

        State.LastMessage = DateTimeOffset.Now;
    }

    /// <summary>
    /// Marks a game as having started by setting its seed and validation hash for all players to generate their roms
    /// </summary>
    /// <param name="validationHash"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public bool StartGame(string validationHash, out string? error)
    {
        error = "";

        if (PlayerStates.Any(x => x.Config == null))
        {
            error = "One or more players is missing a config";
            return false;
        }

        if (PlayerGenerationData.Count != PlayerStates.Count)
        {
            error = "One or more players is missing generation data";
            return false;
        }

        foreach (var state in PlayerStates.Where(x => !string.IsNullOrEmpty(x.GenerationData)))
        {
            state.Status = MultiplayerPlayerStatus.Playing;
        }

        State.ValidationHash = validationHash;
        State.Status = MultiplayerGameStatus.Started;
        State.LastMessage = DateTimeOffset.Now;
        return true;
    }

    /// <summary>
    /// Retrieves a player object from the game
    /// </summary>
    /// <param name="guid">The player unique identifier</param>
    /// <param name="key">The key to verify the player's identity</param>
    /// <param name="verifyKey">If the player object should only be returned if the provided key matches</param>
    /// <param name="verifyAdmin">If the player object should only be returned if they are the admin player</param>
    /// <returns></returns>
    public MultiplayerPlayer? GetPlayer(string guid, string? key, bool verifyKey, bool verifyAdmin = false)
    {
        if (!_players.TryGetValue(guid, out var player)) return null;
        if (verifyKey && key != player.Key) return null;
        if (verifyAdmin && AdminPlayer != player) return null;
        return player;
    }

    /// <summary>
    /// Updates the player's state and status
    /// </summary>
    /// <param name="player">The player object to update</param>
    /// <param name="state">The new state for the player</param>
    public void UpdatePlayerState(MultiplayerPlayer player, MultiplayerPlayerState state)
    {
        player.State = state;
    }

    /// <summary>
    /// Updates the player's config
    /// </summary>
    /// <param name="player">The player object to update</param>
    /// <param name="config">The new config for the player</param>
    public void UpdatePlayerConfig(MultiplayerPlayer player, string config)
    {
        player.State.Config = config;
        if (!State.HasGameStarted) player.State.Status = MultiplayerPlayerStatus.Ready;
    }

    /// <summary>
    /// Updates the player world
    /// </summary>
    /// <param name="player">The player object to update</param>
    /// <param name="state">The new state for the player</param>
    /// <returns>A collection of all objects that were updated for the player</returns>
    public PlayerWorldUpdates UpdatePlayerWorld(MultiplayerPlayer player, MultiplayerWorldState state)
    {
        var updates = player.State.SyncPlayerWorld(state);
        if (updates.HasUpdates) State.LastMessage = DateTimeOffset.Now;
        return updates;
    }

    /// <summary>
    /// Updates the game's status
    /// </summary>
    /// <param name="status">The new status for the game</param>
    public void UpdateGameStatus(MultiplayerGameStatus status)
    {
        State.LastMessage = DateTimeOffset.Now;
        State.Status = status;
    }

    /// <summary>
    /// Updates a player's generation data and returns true if all players have been submitted
    /// </summary>
    /// <param name="player">The player to modify</param>
    /// <param name="worldId">The world id of the player</param>
    /// <param name="playerGenerationData">The string representation of the player generation data</param>
    /// <returns></returns>
    public void SetPlayerGenerationData(MultiplayerPlayer player, int worldId, string playerGenerationData)
    {
        player.State.WorldId = worldId;
        player.State.GenerationData = playerGenerationData;
        State.LastMessage = DateTimeOffset.Now;
    }

    /// <summary>
    /// Marks a location as accessed
    /// </summary>
    /// <param name="player"></param>
    /// <param name="locationId"></param>
    public MultiplayerLocationState? TrackLocation(MultiplayerPlayer player, int locationId)
    {
        var location = player.State.TrackLocation(locationId);
        State.LastMessage = DateTimeOffset.Now;
        return location;
    }

    /// <summary>
    /// Updates the amount of an item that have been retrieved
    /// </summary>
    /// <param name="player"></param>
    /// <param name="type"></param>
    /// <param name="trackedValue"></param>
    public MultiplayerItemState? TrackItem(MultiplayerPlayer player, ItemType type, int trackedValue)
    {
        var item = player.State.TrackItem(type, trackedValue);
        State.LastMessage = DateTimeOffset.Now;
        return item;
    }

    /// <summary>
    /// Marks a boss as defeated
    /// </summary>
    /// <param name="player"></param>
    /// <param name="type"></param>
    public MultiplayerBossState? TrackBoss(MultiplayerPlayer player, BossType type)
    {
        var boss = player.State.TrackBoss(type);
        State.LastMessage = DateTimeOffset.Now;
        return boss;
    }

    /// <summary>
    /// Marks a dungeon as completed
    /// </summary>
    /// <param name="player"></param>
    /// <param name="name"></param>
    public MultiplayerDungeonState? TrackDungeon(MultiplayerPlayer player, string name)
    {
        var dungeon = player.State.TrackDungeon(name);
        State.LastMessage = DateTimeOffset.Now;
        return dungeon;
    }

    private static string CleanPlayerName(string name)
    {
        name = s_illegalPlayerNameCharacters.Replace(name, " ");
        name = s_continousSpace.Replace(name, " ");
        return name.Trim();
    }
}
