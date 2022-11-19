using System.Collections.Concurrent;
using Randomizer.Shared.Multiplayer;
using Randomizer.Shared.Models;

namespace Randomizer.Multiplayer.Server;

public class MultiplayerGame
{
    private static readonly ConcurrentDictionary<string, MultiplayerGame> s_games = new();

    private static readonly ConcurrentDictionary<string, MultiplayerPlayer> s_playerConnections = new();

    private readonly ConcurrentDictionary<string, MultiplayerPlayer> _players = new();

    public MultiplayerGame(string guid, MultiplayerGameType type)
    {
        Guid = guid;
        LastMessage = DateTime.Now;
        Type = type;
    }

    public static (MultiplayerGame Game, MultiplayerPlayer Player)? CreateNewGame(string playerName, string playerConnectionId, MultiplayerGameType gameType, out string? error)
    {
        string guid;
        do
        {
            guid = System.Guid.NewGuid().ToString();
        } while (s_games.ContainsKey(guid));

        var game = new MultiplayerGame(guid, gameType);

        if (!s_games.TryAdd(guid, game))
        {
            error = "Unable to create game";
            return null;
        }

        var playerGuid = System.Guid.NewGuid().ToString();
        var playerKey = System.Guid.NewGuid().ToString();
        var player = new MultiplayerPlayer(game, playerGuid, playerKey, playerName, playerConnectionId) { State =
        {
            IsAdmin = true
        } };

        game.AdminPlayer ??= player;
        game._players[playerGuid] = player;
        s_playerConnections[playerConnectionId] = player;
        error = null;
        return (game, player);
    }

    public static MultiplayerGame? LoadGame(string guid)
    {
        return s_games.TryGetValue(guid, out var game) ? game : null;
    }

    public string Guid { get; init; }

    public DateTime LastMessage { get; set; }

    public MultiplayerPlayer? AdminPlayer { get; set; }

    public bool HasGameStarted => Status != MultiplayerGameStatus.Created;

    public MultiplayerGameStatus Status { get; private set; }

    public MultiplayerGameType Type { get; init; }

    public MultiplayerPlayer? JoinGame(string playerName, string playerConnectionId, out string? error)
    {
        if (_players.Values.Any(prevPlayer => prevPlayer.State.PlayerName == playerName))
        {
            error = "Player name in use";
            return null;
        }

        string guid;
        do
        {
            guid = System.Guid.NewGuid().ToString();
        } while (_players.ContainsKey(guid));

        var key = System.Guid.NewGuid().ToString();

        var player = new MultiplayerPlayer(this, guid, key, playerName, playerConnectionId);
        _players[guid] = player;
        s_playerConnections[playerConnectionId] = player;
        error = null;
        return player;
    }

    public void RejoinGame(MultiplayerPlayer player, string playerConnectionId)
    {
        s_playerConnections[playerConnectionId] = player;
        player.ConnectionId = playerConnectionId;
        player.State.IsConnected = true;
        UpdatePlayerStatus(player);
    }

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

        if (!HasGameStarted)
        {
            s_playerConnections.TryRemove(player.ConnectionId, out _);
            _players.TryRemove(player.Guid, out _);
        }
        else
        {
            UpdatePlayerStatus(player);
        }
    }

    public List<MultiplayerPlayerState>? StartGame(List<MultiplayerPlayerState> players, out string? error)
    {
        error = "";

        if (players.Count != _players.Count)
        {
            error = "Provided player states does not match game players";
            return null;
        }

        if (!players.All(x => _players.ContainsKey(x.Guid)))
        {
            error = "Provided player states does not match game players";
            return null;
        }

        foreach (var player in _players.Values)
        {
            player.State = players.Single(x => x.Guid == player.Guid);
        }
        var playerStates = new List<MultiplayerPlayerState>();
        Status = MultiplayerGameStatus.Started;
        return playerStates;
    }

    public MultiplayerPlayer? GetPlayer(string guid, string? key, bool verifyKey, bool verifyAdmin = false)
    {
        if (!_players.TryGetValue(guid, out var player)) return null;
        if (verifyKey && key != player.Key) return null;
        if (verifyAdmin && AdminPlayer != player) return null;
        return player;
    }

    public Dictionary<string, string?> UpdatePlayerConfig(MultiplayerPlayer player, string config)
    {
        player.State.Config = config;
        UpdatePlayerStatus(player);
        return PlayerConfigs;
    }

    public static MultiplayerPlayer? PlayerDisconnected(string connectionId)
    {
        if (s_playerConnections.Remove(connectionId, out var player))
        {
            player.State.IsConnected = false;
            player.Game.UpdatePlayerStatus(player);
            return player;
        }

        return null;
    }

    public Dictionary<string, string?> PlayerConfigs => _players.Values.ToDictionary(x => x.Guid, x => x.State.Config);

    public Dictionary<string, string> PlayerNames => _players.Values.ToDictionary(x => x.Guid, x => x.State.PlayerName);

    public List<MultiplayerPlayerState> PlayerStates => _players.Values.Select(x => x.State).ToList();

    private bool AddPlayer(MultiplayerPlayer player)
    {
        return _players.TryAdd(player.Guid, player);
    }

    private void UpdatePlayerStatus(MultiplayerPlayer player)
    {
        if (player.State.Config == null) player.State.Status = MultiplayerPlayerStatus.ConfigPending;
        else if (!player.State.IsConnected) player.State.Status = MultiplayerPlayerStatus.Disconnected;
        else if (player.State.HasForfeited) player.State.Status = MultiplayerPlayerStatus.Forfeit;
        else if (player.State.HasCompleted) player.State.Status = MultiplayerPlayerStatus.Completed;
        else player.State.Status = Status == MultiplayerGameStatus.Created
            ? MultiplayerPlayerStatus.Ready
            : MultiplayerPlayerStatus.Playing;
    }
}
