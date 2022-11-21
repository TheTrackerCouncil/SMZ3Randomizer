using System.Collections.Concurrent;
using Randomizer.Shared.Multiplayer;

namespace Randomizer.Multiplayer.Server;

public class MultiplayerGame
{
    private static readonly ConcurrentDictionary<string, MultiplayerGame> s_games = new();

    private static readonly ConcurrentDictionary<string, MultiplayerPlayer> s_playerConnections = new();

    private readonly ConcurrentDictionary<string, MultiplayerPlayer> _players = new();

    public MultiplayerGame(string guid, string gameUrl, MultiplayerGameType type)
    {
        State = new MultiplayerGameState()
        {
            Guid = guid, Url = gameUrl ,Type = type, Status = MultiplayerGameStatus.Created, LastMessage = DateTime.Now,
        };
    }

    public static (MultiplayerGame Game, MultiplayerPlayer Player)? CreateNewGame(string playerName, string playerConnectionId, MultiplayerGameType gameType, string baseUrl, out string? error)
    {
        string guid;
        do
        {
            guid = Guid.NewGuid().ToString("N");
        } while (s_games.ContainsKey(guid));

        var game = new MultiplayerGame(guid, $"{baseUrl}?game={guid}", gameType);

        if (!s_games.TryAdd(guid, game))
        {
            error = "Unable to create game";
            return null;
        }

        var playerGuid = Guid.NewGuid().ToString("N");
        var playerKey = Guid.NewGuid().ToString("N");
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

    public MultiplayerGameState State { get; init; }

    public MultiplayerPlayer? AdminPlayer { get; set; }

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
            guid = Guid.NewGuid().ToString("N");
        } while (_players.ContainsKey(guid));

        var key = Guid.NewGuid().ToString("N");

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

    public bool StartGame(string seed, string validationHash, out string? error)
    {
        error = "";

        if (PlayerStates.Any(x => x.Config == null))
        {
            error = "One or more players is missing a config";
            return false;
        }

        State.Seed = seed;
        State.ValidationHash = validationHash;
        State.Status = MultiplayerGameStatus.Started;
        return true;
    }

    public MultiplayerPlayer? GetPlayer(string guid, string? key, bool verifyKey, bool verifyAdmin = false)
    {
        if (!_players.TryGetValue(guid, out var player)) return null;
        if (verifyKey && key != player.Key) return null;
        if (verifyAdmin && AdminPlayer != player) return null;
        return player;
    }

    public Dictionary<string, string?> UpdatePlayerState(MultiplayerPlayer player, MultiplayerPlayerState state)
    {
        player.State = state;
        UpdatePlayerStatus(player);
        return PlayerConfigs;
    }

    public void UpdateGameStatus(MultiplayerGameStatus status)
    {
        State.Status = status;
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
        else player.State.Status = State.Status == MultiplayerGameStatus.Created
            ? MultiplayerPlayerStatus.Ready
            : MultiplayerPlayerStatus.Playing;
    }
}
