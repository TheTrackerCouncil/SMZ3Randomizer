using System.Collections.Concurrent;
using Randomizer.Data.Multiworld;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.Multiworld.Server;

public class MultiworldGame
{
    private static readonly ConcurrentDictionary<string, MultiworldGame> s_games = new();

    private static readonly ConcurrentDictionary<string, MultiworldPlayer> _playerConnections = new();

    private readonly ConcurrentDictionary<string, MultiworldPlayer> _players = new();

    public MultiworldGame(string guid)
    {
        Guid = guid;
        LastMessage = DateTime.Now;
    }

    public static (MultiworldGame Game, MultiworldPlayer Player)? CreateNewGame(string playerName, string playerConnectionId, out string? error)
    {
        string guid;
        do
        {
            guid = System.Guid.NewGuid().ToString();
        } while (s_games.ContainsKey(guid));

        var game = new MultiworldGame(guid);

        if (!s_games.TryAdd(guid, game))
        {
            error = "Unable to create game";
            return null;
        }

        var playerGuid = System.Guid.NewGuid().ToString();
        var playerKey = System.Guid.NewGuid().ToString();
        var player = new MultiworldPlayer(game, playerGuid, playerKey, playerName, playerConnectionId) { State =
        {
            IsAdmin = true
        } };

        game.AdminPlayer ??= player;
        game._players[playerGuid] = player;
        _playerConnections[playerConnectionId] = player;
        error = null;
        return (game, player);
    }

    public static MultiworldGame? LoadGame(string guid)
    {
        return s_games.TryGetValue(guid, out var game) ? game : null;
    }

    public string Guid { get; init; }

    public DateTime LastMessage { get; set; }

    public MultiworldPlayer? AdminPlayer { get; set; }

    public MultiworldPlayer? JoinGame(string playerName, string playerConnectionId, out string? error)
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

        var player = new MultiworldPlayer(this, guid, key, playerName, playerConnectionId);
        _players[guid] = player;
        _playerConnections[playerConnectionId] = player;
        error = null;
        return player;
    }

    public void RejoinGame(MultiworldPlayer player, string playerConnectionId)
    {
        _playerConnections[playerConnectionId] = player;
        player.ConnectionId = playerConnectionId;
        player.State.IsConnected = true;
    }

    public List<MultiworldPlayerState>? StartGame(List<string> players, TrackerState trackerState, out string? error)
    {
        error = "";
        var playerStates = new List<MultiworldPlayerState>();

        /*for (var i = 0; i < players.Count; i++)
        {
            var playerGuid = players[i];
            var player = _players[playerGuid];
            player.Id = i;
            var playerState = new MultiworldPlayerState(i, playerGuid,
                trackerState.LocationStates.Where(x => x.WorldId == player.Id),
                trackerState.ItemStates.Where(x => x.WorldId == player.Id),
                trackerState.BossStates.Where(x => x.WorldId == player.Id),
                trackerState.DungeonStates.Where(x => x.WorldId == player.Id)
            );
            playerStates.Add(playerState);
            player.State = playerState;
        }*/

        return playerStates;
    }

    public MultiworldPlayer? GetPlayer(string guid, string? key, bool verifyKey, bool verifyAdmin = false)
    {
        if (!_players.TryGetValue(guid, out var player)) return null;
        if (verifyKey && key != player.Key) return null;
        if (verifyAdmin && AdminPlayer != player) return null;
        return player;
    }

    public Dictionary<string, Config?> UpdatePlayerConfig(MultiworldPlayer player, Config config)
    {
        player.State.Config = config;
        var worldId = 0;
        foreach (var currentPlayer in _players.Values.Where(x => x.State.Config != null).OrderBy(x => x != AdminPlayer))
        {
            currentPlayer.State.WorldId = worldId;
            currentPlayer.State.Config!.Id = worldId;
            currentPlayer.State.Config!.PlayerGuid = currentPlayer.Guid;
            worldId++;
        }
        return PlayerConfigs;
    }

    public static MultiworldPlayer? PlayerDisconnected(string connectionId)
    {
        if (_playerConnections.Remove(connectionId, out var player))
        {
            player.State.IsConnected = false;
            return player;
        }

        return null;
    }

    public Dictionary<string, Config?> PlayerConfigs => _players.Values.ToDictionary(x => x.Guid, x => x.State.Config);

    public Dictionary<string, string> PlayerNames => _players.Values.ToDictionary(x => x.Guid, x => x.State.PlayerName);

    public List<MultiworldPlayerState> PlayerStates => _players.Values.Select(x => x.State).ToList();

    private bool AddPlayer(MultiworldPlayer player)
    {
        return _players.TryAdd(player.Guid, player);
    }
}
