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
    private static readonly Regex s_illegalCharacters = new(@"[^A-Z0-9\-]", RegexOptions.IgnoreCase);
    private static readonly Regex s_continousSpace = new(@" +");
    private static readonly ConcurrentDictionary<string, MultiplayerGame> s_games = new();
    private static readonly ConcurrentDictionary<string, MultiplayerPlayer> s_playerConnections = new();
    private readonly ConcurrentDictionary<string, MultiplayerPlayer> _players = new();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="guid">The unique identifier for looking up the game</param>
    /// <param name="gameUrl">The url for players to use for connecting to the game</param>
    /// <param name="version">The SMZ3 version that the game is being generated on</param>
    /// <param name="type">The type of multiplayer game</param>
    private MultiplayerGame(string guid, string gameUrl, string version, MultiplayerGameType type)
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
            Seed = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, int.MaxValue).ToString()
        };
    }

    public string Guid => State.Guid;
    public MultiplayerGameState State { get; }
    public MultiplayerPlayer? AdminPlayer { get; private set; }
    public List<MultiplayerPlayerState> PlayerStates => _players.Values.Select(x => x.State).ToList();
    public List<string> PlayerGenerationData => _players.Values.Select(x => x.PlayerGenerationData).NonNull().ToList();


    #region Static Methods

    public static int ExpireGames(int expirationTime)
    {
        var amountExpired = 0;
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
                amountExpired++;
            }
        }

        return amountExpired;
    }

    /// <summary>
    /// Creates a new multiplayer game
    /// </summary>
    /// <param name="playerName">The requested name of the admin player</param>
    /// <param name="playerConnectionId">The connection id of the admin player</param>
    /// <param name="gameType">The type of multiplayer game</param>
    /// <param name="baseUrl">The server's base url</param>
    /// <param name="version">The SMZ3 version that the game is being generated on</param>
    /// <param name="error">Output of any error messages during creating the new multiplayer game</param>
    /// <returns>The instance of the created Multiplayer game</returns>
    public static MultiplayerGame? CreateNewGame(string playerName, string playerConnectionId, MultiplayerGameType gameType, string baseUrl, string version, out string? error)
    {
        string guid;
        do
        {
            guid = System.Guid.NewGuid().ToString("N");
        } while (s_games.ContainsKey(guid));

        var game = new MultiplayerGame(guid, $"{baseUrl}?game={guid}", version, gameType);

        if (!s_games.TryAdd(guid, game))
        {
            error = "Unable to create game";
            return null;
        }

        var playerGuid = System.Guid.NewGuid().ToString("N");
        var playerKey = System.Guid.NewGuid().ToString("N");
        var player = new MultiplayerPlayer(game, playerGuid, playerKey, CleanPlayerName(playerName), playerConnectionId) { State =
        {
            IsAdmin = true
        } };

        game.AdminPlayer ??= player;
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
        player.Game.UpdatePlayerStatus(player);
        return player;
    }
    #endregion Static Methods

    /// <summary>
    /// Adds a player to a multiplayer game
    /// </summary>
    /// <param name="playerName">The desired name of the player</param>
    /// <param name="playerConnectionId">The connection id of the player</param>
    /// <param name="version">The SMZ3 version the player is running on</param>
    /// <param name="error">Output of any error while trying to add the player to the game</param>
    /// <returns>The player object for the added player, if successfully added</returns>
    public MultiplayerPlayer? JoinGame(string playerName, string playerConnectionId, string version, out string? error)
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

        var player = new MultiplayerPlayer(this, guid, key, CleanPlayerName(playerName), playerConnectionId);
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
        UpdatePlayerStatus(player);
    }

    /// <summary>
    /// Forfeits a player by either removing them from the game if the game hasn't started yet or by marking the
    /// player as forfeit so players can collect their items if the game has already started
    /// </summary>
    /// <param name="player">The player to mark as forfeit</param>
    public void ForfeitPlayer(MultiplayerPlayer player)
    {
        player.State.HasForfeited = true;
        if (player.State.IsAdmin)
        {
            player.State.IsAdmin = false;
            var newAdmin = _players.Values.FirstOrDefault(x => !x.State.HasForfeited);
            if (newAdmin != null)
            {
                newAdmin.State.IsAdmin = true;
                AdminPlayer = newAdmin;
            }
        }

        State.LastMessage = DateTimeOffset.Now;

        if (!State.HasGameStarted)
        {
            s_playerConnections.TryRemove(player.ConnectionId, out _);
            _players.TryRemove(player.Guid, out _);
        }
        else
        {
            UpdatePlayerStatus(player);
        }
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
        UpdatePlayerStatus(player);
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
    /// <param name="player"></param>
    /// <param name="playerGenerationData"></param>
    /// <returns></returns>
    public void SetPlayerGenerationData(MultiplayerPlayer player, string playerGenerationData)
    {
        player.PlayerGenerationData = playerGenerationData;
        State.LastMessage = DateTimeOffset.Now;
    }

    /// <summary>
    /// Marks a location as accessed
    /// </summary>
    /// <param name="player"></param>
    /// <param name="locationId"></param>
    public void TrackLocation(MultiplayerPlayer player, int locationId)
    {
        player.State.TrackLocation(locationId);
        State.LastMessage = DateTimeOffset.Now;
    }

    /// <summary>
    /// Updates the amount of an item that have been retrieved
    /// </summary>
    /// <param name="player"></param>
    /// <param name="type"></param>
    /// <param name="trackedValue"></param>
    public void TrackItem(MultiplayerPlayer player, ItemType type, int trackedValue)
    {
        player.State.TrackItem(type, trackedValue);
        State.LastMessage = DateTimeOffset.Now;
    }

    /// <summary>
    /// Marks a boss as defeated
    /// </summary>
    /// <param name="player"></param>
    /// <param name="type"></param>
    public void TrackBoss(MultiplayerPlayer player, BossType type)
    {
        player.State.TrackBoss(type);
        State.LastMessage = DateTimeOffset.Now;
    }

    /// <summary>
    /// Marks a dungeon as completed
    /// </summary>
    /// <param name="player"></param>
    /// <param name="name"></param>
    public void TrackDungeon(MultiplayerPlayer player, string name)
    {
        player.State.TrackDungeon(name);
        State.LastMessage = DateTimeOffset.Now;
    }

    /// <summary>
    /// Updates a player's status based on both
    /// </summary>
    /// <param name="player">The player object to update</param>
    private void UpdatePlayerStatus(MultiplayerPlayer player)
    {
        if (player.State.Config == null) player.State.Status = MultiplayerPlayerStatus.ConfigPending;
        else if (!player.State.IsConnected) player.State.Status = MultiplayerPlayerStatus.Disconnected;
        else if (player.State.HasForfeited) player.State.Status = MultiplayerPlayerStatus.Forfeit;
        else if (player.State.HasCompleted) player.State.Status = MultiplayerPlayerStatus.Completed;
        else player.State.Status = State.Status == MultiplayerGameStatus.Created
            ? MultiplayerPlayerStatus.Ready
            : MultiplayerPlayerStatus.Playing;
    }

    private static string CleanPlayerName(string name)
    {
        name = s_illegalCharacters.Replace(name, " ");
        name = s_continousSpace.Replace(name, " ");
        return name.Trim();
    }
}
