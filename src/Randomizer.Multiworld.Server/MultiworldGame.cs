using System.Collections.Concurrent;
using Randomizer.Data.Multiworld;
using Randomizer.Data.Options;
using Randomizer.Shared.Models;

namespace Randomizer.Multiworld.Server;

public class MultiworldGame
{
    private static readonly ConcurrentDictionary<string, MultiworldGame> s_games = new();

    private readonly ConcurrentDictionary<string, MultiworldPlayer> _players = new();

    public MultiworldGame(string guid)
    {
        Guid = guid;
        LastMessage = DateTime.Now;
    }

    public static (MultiworldGame Game, MultiworldPlayer Player)? CreateNewGame(string playerName, out string? error)
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
        var player = new MultiworldPlayer() { Guid = playerGuid, Key = playerKey, PlayerName = playerName};

        if (game.AddPlayer(player))
        {
            error = null;
            game.AdminPlayer ??= player;
            return (game, player);
        }

        error = "Unable to add player to game";
        return null;
    }

    public static MultiworldGame? LoadGame(string guid)
    {
        return s_games.TryGetValue(guid, out var game) ? game : null;
    }

    public string Guid { get; init; }

    public DateTime LastMessage { get; set; }

    public MultiworldPlayer? AdminPlayer { get; set; }

    public MultiworldPlayer? JoinGame(string playerName, out string? error)
    {
        error = null;

        if (_players.Values.Any(prevPlayer => prevPlayer.PlayerName == playerName))
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

        var player = new MultiworldPlayer() { Guid = guid, Key = key, PlayerName = playerName};

        if (_players.TryAdd(guid, player))
        {
            return player;
        }

        error = "Unable to add player to player list";
        return null;
    }

    public List<MultiworldPlayerState> StartGame(List<string> players, TrackerState trackerState)
    {
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

    public Dictionary<string, Config> Players => _players.ToDictionary(x => x.Key, x => x.Value.Config);

    public Dictionary<string, string> PlayerNames => _players.Values.ToDictionary(x => x.Guid, x => x.PlayerName);

    private bool AddPlayer(MultiworldPlayer player)
    {
        return _players.TryAdd(player.Guid, player);
    }
}
